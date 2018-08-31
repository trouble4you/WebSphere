using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Opc.Da;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;

namespace WebSphere.ClientOPC
{

    class OpcDaPoller : DataPoller
    {
        Opc.Da.Server server;
        Opc.URL url;
        List<Opc.Da.Item> _tags;
        private Opc.Da.SubscriptionState _groupWriteState;
        Opc.Da.Subscription groupWrite;
        private static readonly Logging logger = new Logging();
        // private static Logger logger = LogManager.GetCurrentClassLogger();
        bool checkLastPoll = true;
        bool restart = false;

        public override bool IsConnected()
        {
            return server.IsConnected;
        }

        /// <summary>
        /// Обработчик обновления тегов по подписке
        /// </summary>
        private void OnNotification(object group, object hReq, Opc.Da.ItemValueResult[] items)
        {
            LastPoll = DateTime.Now;
            for (int i = 0; i < items.GetLength(0); i++)
            {
                var tag = new TagId();
                //Найдем тег с списке 
                tag = TagListBackup.FirstOrDefault(x => x.TagName == items[i].ItemName) ??
                      new TagId { TagName = items[i].ItemName, PollerId = PollerId };

                if (items[i].Quality.GetCode() >= 192)
                {
                    string value = items[i].Value.ToString();

                    OnUpdate(tag, value, items[i].Timestamp, items[i].Quality.GetCode());
                }
                // проверка на то, что качество у нас Good или его производные
                //    if (items[i].Quality.GetCode() >= 192)
                //    {
                //      if (items[i].Value.IsDiscreteType())
                //      {
                //          bool value = items[i].Value.ToString() == "True";
                //          OnUpdateDiscrete(tag, value, items[i].Timestamp, items[i].Quality.GetCode());
                //      }
                //      else
                //          if (items[i].Value.IsAnalogType())
                //          {
                //              // я дичайше извиняюсь за этот и вышестоящий костыли, но более приличного способа кастануть ЭТО я не нашел
                //              float value = float.Parse(items[i].Value.ToString());
                //              OnUpdateAnalog(tag, value, items[i].Timestamp, items[i].Quality.GetCode());
                //          }
                //    }
                else
                    OnUpdate(tag, null, items[i].Timestamp, items[i].Quality.GetCode());
            }
        }


        /// <summary>
        /// Метод рекурсивного поиска тегов в UA структурах и добавление их в disoveredTags
        /// </summary> 
        /// <param name="id">объект который надо просканировать</param>                    
        private int Browse(Opc.ItemIdentifier id = null)
        {
            try
            {
                var filters = new Opc.Da.BrowseFilters { BrowseFilter = Opc.Da.browseFilter.all };
                Opc.Da.BrowsePosition pos = null;
                var elements = server.Browse(id, filters, out pos);
                if (elements != null)
                    foreach (Opc.Da.BrowseElement element in elements)
                    {
                        if (element.IsItem)
                        { _tags.Add(new Opc.Da.Item { ItemName = element.ItemName }); }
                        if (element.HasChildren)
                        {
                            Browse(new Opc.ItemIdentifier(element.ItemPath, element.ItemName));
                        }
                    }

                return 0;
            }


            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Добавление тегов к подписке
        /// </summary>
        public override void AddTags(List<TagId> taglist)
        {
            TagListBackup = taglist;
            if (!Activated) return;
            var subList = new List<Opc.Da.Item>();
            if (!SubAll)
                foreach (var tag in taglist)
                {

                    if (_tags.Exists(d => d.ItemName == tag.TagName))
                    {   // TagListBackup.Add(tag);
                        subList.Add(new Opc.Da.Item { ItemName = tag.TagName });
                        logger.Logged("Info", "#" + PollerId + ": добавляем тег '" + tag.TagName + "' в подписку", "OpcDaPoller", "AddTags");
                    }
                    else
                    {
                        logger.Logged("Error", "#" + PollerId + ": тег '" + tag.TagName + "' не обнаружен на сервере", "OpcUaPoller", "AddTags");
                    }
                }
            else
            {
                logger.Logged("Info", "#" + PollerId + ": добавляем все теги сервера в подписку", "OpcDaPoller", "AddTags");
                subList = _tags;
            }
            try
            {
                var groupState = new Opc.Da.SubscriptionState { Name = "Group", Active = true };
                var @group = (Opc.Da.Subscription)server.CreateSubscription(groupState);
                @group.AddItems(subList.ToArray());
                @group.DataChanged += new Opc.Da.DataChangedEventHandler(OnNotification);
                logger.Logged("Info", "Добавлено {" + subList.Count() + "}  тегов для контроля с OPC DA сервера #{" + PollerId + "}", "", "");
            }
            catch (Exception ex)
            {
                logger.Logged("Error", "Не удалось добавить теги для контроля OPC DA сервером #" + PollerId + ": " + ex.Message, "", "");
            }
        }

        public void Shutdown(string reason)
        {
            logger.Logged("Warn", "Сервер сообщил о прекращении работы. Причина: " + reason, "", "");
            Uninitialize();
        }

        public override bool Initialize()
        {
            return Initialize(ConnString, PollerId);
        }

        public override bool Initialize(string connectionString, int newPollerId)
        {
            bool result = false;
            ConnString = connectionString;

            try
            {
                PollerId = newPollerId;
                url = new Opc.URL(connectionString);
                server = null;
                var fact = new OpcCom.Factory();
                server = new Opc.Da.Server(fact, null);
                server.Connect(url, new Opc.ConnectData(new System.Net.NetworkCredential()));
                server.ServerShutdown += Shutdown;
                _tags = new List<Opc.Da.Item>();
                Browse();

                _groupWriteState = new Opc.Da.SubscriptionState();
                _groupWriteState.Name = "GroupWrite";
                _groupWriteState.Active = false;

                groupWrite = (Opc.Da.Subscription)server.CreateSubscription(_groupWriteState);
                Activated = true;
                logger.Logged("Info", "Состояние сервера: " + server.GetStatus().StatusInfo, "", "");
                result = true;
            }
            catch (Exception ex)
            {
                logger.Logged("Error", "Не удалось подключиться к OPC DA серверу " + connectionString + ": " + ex.Message, "", "");
                // logger.Logged("Warn", "Повторная попытка через 5 секунд...", "", "");
                //Thread.Sleep(5000);
            }

            return result;
        }

        public override bool Uninitialize()
        {
            if (Activated && server != null)
                try
                {
                    logger.Logged("Info", "Отключаемся от сервера #" + PollerId + "...", "", "");
                    Activated = false;
                    TagListBackup.Clear();
                    server.Disconnect();
                    server = null;


                }
                catch (Exception ex)
                {
                    logger.Logged("Error", "Не удалось корректно отключиться от OPC DA сервера #" + PollerId + ": " + ex.Message, "", "");
                }

            return true;
        }
        public override bool WriteTag(TagId tag, string value)
        {
            try
            {
                groupWrite.RemoveItems(groupWrite.Items);
                List<Item> writeList = new List<Item>();
                List<ItemValue> valueList = new List<ItemValue>();

                Item itemToWrite = new Item();
                itemToWrite.ItemName = tag.TagName;
                ItemValue itemValue = new ItemValue(itemToWrite);
                itemValue.Value = value;

                writeList.Add(itemToWrite);
                valueList.Add(itemValue);
                groupWrite.AddItems(writeList.ToArray());
                for (int i = 0; i < valueList.Count; i++)
                    valueList[i].ServerHandle = groupWrite.Items[i].ServerHandle;

                groupWrite.Write(valueList.ToArray());
                return true;
            }
            catch (Exception ex)
            {
                logger.Logged("Error", "Не удалось записать значние OPC DA сервера {" + tag.TagName + "[" + value + "]}: {" + ex.Message + "}", "OpcDaPoller", "WriteTag");
                return false;
            }
        }

        public override string ReadOpcTag(TagId tag)
        {
            throw new NotImplementedException();
        }

    }
}
