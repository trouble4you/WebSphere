using System;
using System.Collections.Generic; 
using System.Threading;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete; 
using Opc.Ua;
using Opc.Ua.Client;
using Subscription = Opc.Ua.Client.Subscription;

namespace WebSphere.ClientOPC
{
    internal class UaPoller : DataPoller
    {
        private static Logging logger = new Logging();
        private Dictionary<string, string> discoveredTags = new Dictionary<string, string>();
        private static ApplicationConfiguration config = null;
        private Session session;
        private static bool stackInitialized = false; 
        /// <summary>
        /// Проверка наличия подключения
        /// </summary>
        public override bool IsConnected()
        {
            return session.Connected;
        }

        /// <summary>
        /// Инициализация OPC UA стэка
        /// </summary>
        public void InitializeStack()
        {
            try
            {
                logger.Logged("Info", "Инициализируем OPC UA стэк...", "OpcUaPoller", "InitializeStack");
                config = new ApplicationConfiguration()
                {
                    ApplicationName = "kDataLogger",
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration =
                        new SecurityConfiguration
                        {
                            ApplicationCertificate =
                                new CertificateIdentifier
                                {
                                    StoreType = @"Windows",
                                    StorePath = @"CurrentUser\My",
                                    SubjectName =
                                        Utils.Format(@"CN={0}, DC={1}", "ConverterSystems.TestClient",
                                            System.Net.Dns.GetHostName())
                                },
                            TrustedPeerCertificates =
                                new CertificateTrustList
                                {
                                    StoreType = @"Windows",
                                    StorePath = @"CurrentUser\TrustedPeople",
                                },
                            NonceLength = 32,
                            AutoAcceptUntrustedCertificates = true
                        },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas {OperationTimeout = 15000},
                    ClientConfiguration = new ClientConfiguration {DefaultSessionTimeout = 60000}
                };
                config.ApplicationUri = "urn:Eksiton:Sfera";
                config.Validate(ApplicationType.Client);
                config.CertificateValidator.CertificateValidation +=
                    (s, e) => { if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted) e.Accept = true; };


                stackInitialized = true;
                logger.Logged("Info", "OPC UA стэк инициализирован успешно", "OpcUaPoller", "InitializeStack");
            }

            catch (Exception ex)
            {
                logger.Logged("Error", "Не удалось корректно инициализировать OPC UA стек:" + ex.Message, "OpcUaPoller",
                    "InitializeStack");
            }
        }

        /// <summary>
        /// Повторная инициализация соединения с сервером
        /// </summary>
        public override bool Initialize()
        {
            return Initialize(ConnString, PollerId);
        }

        /// <summary>
        /// Первичная инициализация соединения с сервером
        /// </summary>
        public override bool Initialize(string connection_string, int new_poller_id)
        {
            bool result = false;
            ConnString = connection_string;

            if (!stackInitialized)
                InitializeStack();

            try
            {
                PollerId = new_poller_id;
                logger.Logged("Info", "Открываем сессию к серверу OPC UA #" + PollerId, "OpcUaPoller", "Initialize");
                session = Session.Create(config,
                    new ConfiguredEndpoint(null, new EndpointDescription(connection_string)), true,
                    "kDataLogger #" + PollerId, 60000, null, null);
                if (session.Connected)
                    logger.Logged("Info", "Соединены с сервером OPC UA #{0}" + PollerId, "OpcUaPoller", "Initialize");
                else
                    logger.Logged("Error", "Отсутствует соединение с сервером OPC UA #{0}" + PollerId, "OpcUaPoller",
                        "Initialize");
                DiscoverTags(session, null, "");
                Activated = true;
                result = true;
            }

            catch (Exception ex)
            {
                session = null;
                logger.Logged("Error",
                    "Не удалось подключиться к OPC UA серверу " + connection_string + ": " + ex.Message, "OpcUaPoller",
                    "Initialize");
                logger.Logged("Warn", "Повторная попытка через 5 секунд...", "OpcUaPoller", "Initialize");
                Thread.Sleep(5000);
            }

            return result;
        }

        /// <summary>
        /// Закрытие соединения с сервером
        /// </summary>
        public override bool Uninitialize()
        {
            if (Activated & session != null)
                try
                {
                    logger.Logged("Info", "Отключаемся от сервера #" + PollerId + "...", "OpcUaPoller", "Uninitialize");
                    Activated = false;
                    TagListBackup.Clear();
                    session.CloseSession(null, true);
                    session = null;
                }
                catch (Exception ex)
                {
                    logger.Logged("Error",
                        "Не удалось корректно отключиться от OPC UA сервера #" + PollerId + ": " + ex.Message,
                        "OpcUaPoller", "Uninitialize");
                }
            return true;
        }

        /// <summary>
        /// Метод рекурсивного поиска тегов в UA структурах и добавление их в disoveredTags
        /// </summary>
        /// <param name="session">объект сессии</param>
        /// <param name="refd">ссылка на структуру которую надо просканировать</param>
        /// <param name="upper_node">отображаемое название родительского узла</param>
        /// 
        private void DiscoverTags(Session session, ReferenceDescription refd, string upper_node)
        {
            ReferenceDescriptionCollection references;
            Byte[] cp;
            try
            {
                if (refd == null)
                    session.Browse(null, null, ObjectIds.ObjectsFolder, 0u, BrowseDirection.Forward,
                        ReferenceTypeIds.HierarchicalReferences, true,
                        (uint) NodeClass.Variable | (uint) NodeClass.Object | (uint) NodeClass.Method, out cp,
                        out references);
                else
                    session.Browse(null, null, ExpandedNodeId.ToNodeId(refd.NodeId, session.NamespaceUris), 0u,
                        BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true,
                        (uint) NodeClass.Variable | (uint) NodeClass.Object | (uint) NodeClass.Method, out cp,
                        out references);

                foreach (var rd in references)
                {
                    string full_name;
                    if (upper_node == "")
                        full_name = rd.DisplayName.ToString();
                    else
                        full_name = upper_node + "." + rd.DisplayName;

                    DiscoverTags(session, rd, full_name);

                    if (rd.NodeClass == NodeClass.Variable)
                    {
                        discoveredTags[full_name] = rd.NodeId.ToString();
                        logger.Logged("Info", "#" + PollerId + ": обнаружен тег " + full_name, "OpcUaPoller",
                            "DiscoverTags");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Logged("Error",
                    "Не удалось получить список тегов от OPC UA сервера #" + PollerId + ": " + ex.Message, "OpcUaPoller",
                    "DiscoverTags");
            }
        }

        /// <summary>
        /// Запись в тег
        /// </summary>
        public override bool WriteTag(TagId tag, string value)
        {
            string identifier = "";
            if (discoveredTags.ContainsKey(tag.TagName))
                identifier = discoveredTags[tag.TagName];
            else
            {
                logger.Logged("Error", "#" + PollerId + ": тег '" + tag.TagName + "' не обнаружен на сервере",
                    "OpcUaPoller", "WriteTag");
                return false;
            }
            var nodesToWrite = new WriteValueCollection
            {
                new WriteValue {NodeId = identifier, Value = new DataValue {Value = value}, AttributeId = 13}
            };
            if (nodesToWrite.Count == 0)
            {
                logger.Logged("Error", "#" + PollerId + ": тег '" + tag.TagName + "' не обнаружен на сервере",
                    "OpcUaPoller", "WriteTag");
                return false;
            }
            StatusCodeCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            ResponseHeader responseHeader = session.Write(null, nodesToWrite, out results, out diagnosticInfos);
            return ServiceResult.IsGood(results[0]);
        }

        /// <summary>
        /// Добавление тегов к по подписке
        /// </summary>
        public override void AddTags(List<TagId> taglist)
        {
            TagListBackup = taglist;
            if (Activated)
            {
                try
                {
                    var subscription = new Subscription(session.DefaultSubscription) {PublishingInterval = 1};

                    int tag_counter = 0;
                    var list = new List<MonitoredItem>();

                    //foreach (TagId tag in taglist)
                    //пока конфигуратор пуст поиск делаем по всей сфере
                    foreach (var tag in discoveredTags)
                        {
                        string identifier = "";
                        //if (discoveredTags.ContainsKey(tag.TagName))
                        //пока конфигуратор пуст поиск делаем по всей сфере
                         if (tag.Key.Contains("Sfera"))
                        {
                            //logger.Logged("Info", "#" + PollerId + ": добавляем тег '" + tag.TagName + "' в подписку",
                            logger.Logged("Info", "#" + PollerId + ": добавляем тег '" + tag.Key + "' в подписку",
                                "OpcUaPoller", "AddTags");
                            //identifier = discoveredTags[tag.TagName];
                            //пока конфигуратор пуст поиск делаем по всей сфере
                            identifier = discoveredTags[tag.Key];
                            tag_counter++;
                            var item = new MonitoredItem(subscription.DefaultItem)
                            {
                               // DisplayName = tag.TagName,
                                DisplayName = tag.Key,
                                StartNodeId = identifier
                            };
                            list.Add(item);
                        }
                        else
                        {
                            logger.Logged("Error",
                                //"#" + PollerId + ": тег '" + tag.TagName + "' не обнаружен на сервере", "OpcUaPoller",
                                "#" + PollerId + ": тег '" + tag.Key + "' не обнаружен на сервере", "OpcUaPoller",
                                "AddTags");
                        }
                    }

                    if (tag_counter > 0)
                    {
                        list.ForEach(i => i.Notification += OnNotification);
                        subscription.AddItems(list);

                        session.AddSubscription(subscription);
                        subscription.Create();

                        logger.Logged("Info",
                            "Добавлено " + tag_counter + " тегов для контроля с OPC UA сервера #" + PollerId + "",
                            "OpcUaPoller", "AddTags");
                    }
                    else
                    {
                        logger.Logged("Error", "Не найдено ни одного тега для контроля OPC UA сервера #" + PollerId + "",
                            "OpcUaPoller", "AddTags");
                    }
                }
                catch (Exception ex)
                {
                    logger.Logged("Error",
                        "Не удалось добавить теги для контроля OPC UA сервером #" + PollerId + ":" + ex.Message,
                        "OpcUaPoller", "AddTags");
                }
            }
        }

        /// <summary>
        /// Обработчик обновления тега по подписке
        /// </summary>
        private void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            LastPoll = DateTime.Now;
            foreach (var value in item.DequeueValues())
            {
                TagId tag = new TagId();
                tag.TagName = item.DisplayName;
                tag.PollerId = PollerId;

                DateTime timestamp = DateTime.SpecifyKind(value.SourceTimestamp, DateTimeKind.Utc);
                timestamp = timestamp.ToLocalTime();
                if (value.Value != null) {
                    string signal = value.Value.ToString();
                OnUpdate(tag, signal, timestamp, (int) value.StatusCode.Code);
                }
                else
                OnUpdate(tag, null, timestamp, (int) value.StatusCode.Code);
                // проверка на то, что качество у нас Good или его производные
                // if ((value.StatusCode.Code & 0xFF000000) == 0)
                // {
                //     if (LoggerUtils.IsDiscreteType(value.Value))
                //     {
                //         bool signal = value.Value.ToString() == "True";
                //          OnUpdateDiscrete(tag, signal, timestamp, (int) value.StatusCode.Code);
                //     }
                //     else 
                //         if (LoggerUtils.IsAnalogType(value.Value))
                //         {
                //             // я дичайше извиняюсь за этот и вышестоящий костыли, но более приличного способа кастануть ЭТО я не нашел
                //             float signal = float.Parse(value.Value.ToString());
                //             OnUpdateAnalog(tag, signal, timestamp, (int) value.StatusCode.Code);
                //         }
                //         else
                //             if (LoggerUtils.IsStringType(value.Value))
                //             {
                //                 // я дичайше извиняюсь за этот и вышестоящий костыли, но более приличного способа кастануть ЭТО я не нашел
                //                 string signal = value.Value.ToString();
                //                 OnUpdateString(tag, signal, timestamp, (int)value.StatusCode.Code);
                //             } 
                // }
                // else 
                //     {
                //         // я дичайше извиняюсь за этот и вышестоящий костыли, но более приличного способа кастануть ЭТО я не нашел
                //         float signal = float.Parse(value.Value.ToString());
                //         OnUpdateEmpty(tag,   timestamp, (int) value.StatusCode.Code);
                //     }
            }
        }

        public override string ReadOpcTag(TagId tag)
        {
            var identifier = "";
            if (discoveredTags.ContainsKey(tag.TagName))
                  identifier = discoveredTags[tag.TagName];
            else {
                logger.Logged("Error", "#" + PollerId + ": тег '" + tag.TagName + "' не обнаружен на сервере",
                    "OpcUaPoller", "WriteTag");
                return null;
            } 
            return session.ReadValue(identifier).ToString(); 
             
        }

        /*    private List<object> OnReadOpcTags(List<TagId> tags)
        { IList<NodeId> nodelist = null;
            IList<Type> typelList = null;
            foreach (var tag in tags)
            {
                nodelist.Add(tag.TagName);
            }
            List<object> values;
            List<ServiceResult> servResult;
             session.ReadValues(nodelist,  typelList, values: out values, errors: out servResult);
            return values;
        }*/
    }
}

 
