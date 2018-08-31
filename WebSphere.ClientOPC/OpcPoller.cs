using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using System.Web.Script.Serialization;

namespace WebSphere.ClientOPC
{
    abstract class DataPoller
    {
        public abstract bool IsConnected();
        public int PollerId;

        public bool Activated;
        public bool Connect = false;
        public DateTime LastPoll;
        public string ConnString;
        public bool SubAll;

        public List<TagId> TagListBackup;
        public bool Restart = false;
        public bool checkLastPoll = true;

        public UpdateActionAnalog OnUpdateAnalog;
        public UpdateActionDiscrete OnUpdateDiscrete;
        public UpdateActionString OnUpdate;
        public UpdateActionEmpty OnUpdateEmpty;

        public abstract bool Initialize();
        public abstract bool Initialize(string connectionString, int newPollerId);
        public abstract bool Uninitialize();
        public abstract void AddTags(List<TagId> taglist);
        public abstract bool WriteTag(TagId tag, string value);
        public abstract string ReadOpcTag(TagId tag);


    }


    delegate void UpdateActionAnalog(TagId tag, float value, DateTime dt, int quality);

    delegate void UpdateActionDiscrete(TagId tag, bool value, DateTime dt, int quality);

    delegate void UpdateActionString(TagId tag, string value, DateTime dt, int quality);

    delegate void UpdateActionEmpty(TagId tag, DateTime dt, int quality);

    public class OpcPoller : IOpcPoller
    {
        public class MySQLRow
        {
            public List<string> values = new List<string>();
        }
 

        private static readonly object TagLocker = new object();
        private static EFDbContext context = new EFDbContext();
        private static readonly JSON json = new JSON();
        private static readonly Logging logger = new Logging();
        static bool fit_first_load = false;
        static bool must_break = false;
        private static List<DataPoller> _pollers;
        private static List<TagValueContainer> _tagValues;
        private static List<TagValueContainer> _tagBlackList;
        private static List<FitValueContainer> _fitList;
        DateTime SaveeSQLTag_Time;

        static bool Worked;

        static Thread _watchdog;
        static int _timeout;

        private static readonly Logging Logger = new Logging();

        static void OnUpdateAnalog(TagId tag, float value, DateTime dt, int quality)
        {
            try
            {
                lock (TagLocker)
                {
                    var index = _tagValues.FindIndex(a => Equals(a.Tag, tag));
                    if (index != -1)
                    {
                        _tagValues.RemoveAt(index);
                        _tagValues.Add(new TagValueContainer
                        {
                            Tag = tag,
                            LastAnalogValue = value,
                            LastLogged = dt,
                            Quality = quality
                        });
                        //_tagValues.First(d => Equals(d.Tag, tag)).LastAnalogValue = value;
                        //_tagValues.First(d => Equals(d.Tag, tag)).LastLogged = dt;
                    }
                    else
                    {
                        _tagValues.Add(new TagValueContainer
                        {
                            Tag = tag,
                            LastAnalogValue = value,
                            LastLogged = dt,
                            Quality = quality
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Logged("Info", "[ ] [ERR]  [" + tag + "//" + ex.Message + "] Cant update to gtags.", "",
                    "OnUpdateAnalog");
            }

        }

        static void OnUpdateDiscrete(TagId tag, bool value, DateTime dt, int quality)
        {
            try
            {
                lock (TagLocker)
                {
                    var index = _tagValues.FindIndex(a => Equals(a.Tag, tag));
                    if (index != -1)
                    {
                        _tagValues.RemoveAt(index);
                        _tagValues.Add(new TagValueContainer
                        {
                            Tag = tag,
                            LastDiscreteValue = value,
                            LastLogged = dt,
                            Quality = quality
                        });
                        //_tagValues.First(d => Equals(d.Tag, tag)).LastAnalogValue = value;
                        //_tagValues.First(d => Equals(d.Tag, tag)).LastLogged = dt;
                    }
                    else
                    {
                        _tagValues.Add(new TagValueContainer
                        {
                            Tag = tag,
                            LastDiscreteValue = value,
                            LastLogged = dt,
                            Quality = quality
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Logged("Info", "[ ] [ERR]  [" + tag + "//" + ex.Message + "] Cant update to gtags.", "",
                    "OnUpdateDiscrete");
            }
        }

        public static void OnUpdate(TagId tag, string value, DateTime dt, int quality)
        {
            try
            {
                if (value != null) value = value.Replace(",", ".");
                lock (TagLocker)
                {
                    var index = _tagValues.FindIndex(a => Equals(a.Tag, tag));
                    if (index != -1)
                    {
                        _tagValues.ElementAt(index).LastLogged = dt;
                        _tagValues.ElementAt(index).Quality = quality;

                        if (!_tagValues.ElementAt(index).Imitation)
                            _tagValues.ElementAt(index).LastValue = value;
                        else
                            _tagValues.ElementAt(index).RealLastValue = value;

                        //  _tagValues.RemoveAt(index); 
                        //  _tagValues.Add(new TagValueContainer
                        //  {
                        //      Tag = tag,
                        //      LastValue = value,
                        //      LastLogged = dt,
                        //      Quality = quality
                        //  });

                        //_tagValues.First(d => Equals(d.Tag, tag)).LastAnalogValue = value;
                        //_tagValues.First(d => Equals(d.Tag, tag)).LastLogged = dt;
                    }
                    else
                    {
                        _tagValues.Add(new TagValueContainer { Tag = tag, LastValue = value, LastLogged = dt, Quality = quality });
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Logged("Info", "[ ] [ERR]  [" + tag + "//" + ex.Message + "] Cant update to gtags.", "",
                    "OnUpdateDiscrete");
            }
        }

        static void OnUpdateEmpty(TagId tag, DateTime dt, int quality)
        {
            try
            {
                lock (TagLocker)
                {
                    var index = _tagValues.FindIndex(a => Equals(a.Tag, tag));
                    if (index != -1)
                    {
                        _tagValues.RemoveAt(index);
                        _tagValues.Add(new TagValueContainer
                        {
                            Tag = tag,
                            LastDiscreteValue = null,
                            LastValue = null,
                            LastAnalogValue = null,
                            LastLogged = dt,
                            Quality = quality
                        });
                        //_tagValues.First(d => Equals(d.Tag, tag)).LastAnalogValue = value;
                        //_tagValues.First(d => Equals(d.Tag, tag)).LastLogged = dt;
                    }
                    else
                    {
                        _tagValues.Add(new TagValueContainer
                        {
                            Tag = tag,
                            LastDiscreteValue = null,
                            LastAnalogValue = null,
                            LastLogged = dt,
                            Quality = quality
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Logged("Info", "[ ] [ERR]  [" + tag + "//" + ex.Message + "] Cant update to gtags.", "",
                    "OnUpdateEmpty");
                // Logger.Logged("[ ] [ERR]  [" + tag + "//" + ex.Message + "] Cant update to gtags.", this);
            }
        }

        public TagValueContainer ReadTag(TagId tag)
        {
            try
            {
                if (!Worked) return null;

                lock (TagLocker)
                {
                    if (_tagBlackList.Exists(d => Equals(d.Tag, tag)))
                    {
                        return null;
                    }

                    if (_tagValues.Exists(d => Equals(d.Tag, tag)))
                    {
                        return _tagValues.First(d => Equals(d.Tag, tag));
                        // }
                        // else
                        // {
                        //   Logger.Log("[ ] [INF]  [" + tag + "] No in gtags.", Logger.Levels.Error);
                        //  _blackList.Add(new AOPCItem(tag, null));
                    }

                }
                //}
                return null;
            }

            catch (Exception ex)
            {
                Logger.Logged("Error", " [ERR] [" + ex.Message + "] HRES[" + ex.HResult + "]", "", "ReadTag");

            }
            return null;
        }

        public TagValueContainer ReadTag(string tag)
        {
            try
            {
                if (!Worked) return null;

                lock (TagLocker)
                {
                    if (_tagBlackList.Exists(d => Equals(d.Tag.TagName, tag)))
                    {
                        return null;
                    }

                    if (_tagValues.Exists(d => Equals(d.Tag.TagName, tag)))
                    {
                        return _tagValues.First(d => Equals(d.Tag.TagName, tag));
                    }

                }
                return null;
            }

            catch (Exception ex)
            {
                Logger.Logged("Error", " [ERR] [" + ex.Message + "] HRES[" + ex.HResult + "]", "", "ReadTag");

            }
            return null;
        }
        public TagValueContainer ReadTag(int tagId)
        {
            try
            {
                if (!Worked) return null;

                lock (TagLocker)
                {
                    if (_tagBlackList.Exists(d => Equals(d.Tag.Id, tagId)))
                    {
                        return null;
                    }

                    if (_tagValues.Exists(d => Equals(d.Tag.Id, tagId)))
                    {
                        return _tagValues.First(d => Equals(d.Tag.Id, tagId));
                    }

                }
                return null;
            }

            catch (Exception ex)
            {
                Logger.Logged("Error", " [ERR] [" + ex.Message + "] HRES[" + ex.HResult + "]", "", "ReadTag");

            }
            return null;
        }

        public List<TagValueContainer> ReadTags()
        {
            try
            {
                if (!Worked) return null;
                lock (TagLocker)
                {
                    return _tagValues;
                }
            }

            catch (Exception ex)
            {
                Logger.Logged("Error", " [ERR] [" + ex.Message + "] HRES[" + ex.HResult + "]", "", "ReadTags");

                //Logger.Log("[MOpcThM.ReadOpcTag] [ERR] [" + ex.Message + "] HRES[" + ex.HResult + "]", Logger.Levels.Error);

            }
            return null;
        }

        public bool WriteTag(TagId tag, string value)
        {
            lock (TagLocker)
            {
                var z = _tagValues.FirstOrDefault(x => Equals(x.Tag, tag));

                {
                    if (z.Imitation)
                    {
                        try
                        {
                            _tagValues.FirstOrDefault(x => Equals(x.Tag, tag)).LastValue = value;
                            Logger.Logged("Info", "[ ] [ERR]  [" + tag + "//" + value + "] Set imitation to tag.", "", "WriteTag");
                            return true;

                        }
                        catch (Exception ex)
                        {
                            Logger.Logged("Info", "[ ] [ERR]  [" + tag + "//" + value + "] Cant set imitation to tag." + ex.Message + "", "",
                                "WriteTag");
                            return false;
                        }
                    }
                    else
                    {
                        var firstOrDefault = _pollers.FirstOrDefault(d => d.PollerId == tag.PollerId);
                        if (firstOrDefault != null)
                            return firstOrDefault.WriteTag(tag, value);
                        else return false;
                    }
                }
            }
        }

        public string OnReadOpcTag(string tag)
        {
            var jss = new JavaScriptSerializer();
            var custs = from Object in context.Objects.Where(x => x.Name == tag)
                        join to in context.Properties.Where(x => x.PropId == 0) on Object.Id equals to.ObjectId
                        select to.Value;
            if (custs.FirstOrDefault() != null)
            {
                var dict = jss.Deserialize<Dictionary<string, dynamic>>(custs.FirstOrDefault());

                if (dict != null)
                {
                    var firstOrDefault = _pollers.FirstOrDefault(d => d.PollerId == dict["Opc"]);
                    if (firstOrDefault != null)
                        return
                            firstOrDefault.ReadOpcTag(new TagId { PollerId = dict["Opc"], TagName = dict["Connection"] });

                }
            }
            return null;
        }

        public bool Reinicialize(int pollerId)
        {
            logger.Logged("Info", "Перезапуск  OPC-сервера [" + pollerId + "] ... ", "PollerWatchdog", "Reinicialize");
            try
            {
                if (_pollers != null) _pollers.First(f => Equals(f.PollerId, pollerId)).Restart = true;
                return true;
            }
            catch (Exception ex)
            {
                logger.Logged("Error", "Не удалось перезапустить  OPC-сервер [" + pollerId + "] : " + ex.Message,
                    "PollerWatchdog", "Reinicialize");
                return false;
            }
        }

        public bool Connect(int pollerId)
        {
            logger.Logged("Info", "Запуск  OPC-сервера [" + pollerId + "] ... ", "PollerWatchdog", "Reinicialize");
            try
            {
                if (_pollers != null) _pollers.First(f => Equals(f.PollerId, pollerId)).Initialize();
                return true;
            }
            catch (Exception ex)
            {
                logger.Logged("Error", "Не удалось запустить  OPC-сервер [" + pollerId + "] : " + ex.Message,
                    "PollerWatchdog", "Reinicialize");
                return false;
            }
        }

        public bool Stop(int pollerId)
        {
            logger.Logged("Info", "Остановка  OPC-сервера [" + pollerId + "] ... ", "PollerWatchdog", "Reinicialize");
            try
            {
                if (_pollers != null) _pollers.First(f => Equals(f.PollerId, pollerId)).Uninitialize();
                return true;
            }
            catch (Exception ex)
            {
                logger.Logged("Error", "Не удалось остановить  OPC-сервер [" + pollerId + "] : " + ex.Message,
                    "PollerWatchdog", "Reinicialize");
                return false;
            }
        }

        public static List<TagId> readTags(int opc_id)
        {
            var jss = new JavaScriptSerializer();
            var resultTags = new List<TagId>();
            bool result = false;
            while (result == false && must_break == false)
            {
                try
                {
                    var tags = (from ti in context.Objects.Where(x => x.Type == 2)
                                join to in context.Properties.Where(x => x.PropId == 0) on ti.Id equals to.ObjectId

                                select new { Id = ti.Id, Prop = to.Value }).ToList();
                    //var tags = from  to context.Objects

                    foreach (var tag in tags)
                    {
                        try
                        {
                            var dict = jss.Deserialize<Dictionary<string, dynamic>>(tag.Prop);
                            if (dict["Opc"] == opc_id)
                            {
                                var newTag = new TagId { Id = tag.Id, PollerId = opc_id, TagName = dict["Connection"] };

                                resultTags.Add(newTag);

                                logger.Logged("Info", "Тэг " + newTag.TagName + " (" + newTag.PollerId + ")  добавлен", "PollerWatchdog", "readTags");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Logged("Error", "Тэг " + tag.Id + " (" + tag.Prop + ") не добавлен: " + ex.Message, "PollerWatchdog", "readTags");
                            continue;
                        }
                    }
                    result = true;

                }
                catch (Exception ex)
                {
                    logger.Logged("Error", "Не удалось считать список тегов для OPC-сервера из БД:" + ex.Message,
                        "PollerWatchdog", "readTags");
                    logger.Logged("Warn", "Повторим попытку через 5 секунд...", "PollerWatchdog", "readTags");
                    Thread.Sleep(5000);
                }
            }
            logger.Logged("Info", "Добавление тэгов для сервера #" + opc_id + " завершено.", "PollerWatchdog",
                "readTags");
            logger.Logged("Info", "Всего " + resultTags.Count() + " тэг(ов)", "PollerWatchdog", "readTags");
            return resultTags;
        }
        public bool TagImit(TagId tag)
        {
            try
            {
                lock (TagLocker)
                {
                    var index = _tagValues.FindIndex(a => Equals(a.Tag, tag));
                    if (index != -1)
                    {
                        _tagValues.ElementAt(index).Imitation = !_tagValues.ElementAt(index).Imitation;
                        _tagValues.ElementAt(index).RealLastValue = null;
                        Logger.Logged("Info", "[ ] [WARN]  [" + tag + "//] Set imitation to tag.", "", "TagImit");
                        return true;
                    }
                    else
                    {
                        Logger.Logged("Info", "[ ] [WARN]  [" + tag + "//] Cant set imitation to tag. No in _tagValues", "",
                            "TagImit");
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Logged("Info", "[ ] [ERR]  [" + tag + "//] Cant set imitation to tag. " + ex.Message + ".", "", "TagImit");
                return false;
            }
        }

        public static void GetConfigFromDb()
        {
            //logger.Info(" =================================================================== ");
            logger.Logged("Info", "Запуск ", "PollerWatchdog", "GetConfigFromDb");
            //dbSaver = new DbSaver();
            bool result = false;
            while (result == false // && must_break == false
                )
            {
                try
                {

                    var opcs = (from ti in context.Objects
                                join to in context.Properties on ti.Id equals to.ObjectId
                                where ti.Type == 1 && to.PropId == 0
                                select new { Id = ti.Id, Name = ti.Name, Props = to.Value }).ToList();
                  //  var z1 = opcs.Where(x => x.Props.Contains(Environment.MachineName)).FirstOrDefault();
                  //   if (z1==null )
                  //      opcs.Add(new { Id = 0, Name = Environment.MachineName, Props = "{\"Id\":0,\"Name\":\"COPCUA\",\"Type\":\"UA\",\"Connection\":\"opc.tcp:\\\\\\\\"+ Environment.MachineName + ":51212\",\"Connect\":true}" });
                   // opclist.Add("{\"Id\":\"1\",\"Type\":\"UA\",\"Connection\":\"opc.tcp://Q3DM6:51212/\"}");


                    foreach (var opc in opcs)
                    {
                        dynamic dict = json.DeserializeObj(opc.Props);
                        DataPoller opcPoller = null;
                        string z = "";
                        try { z = dict.Type; }
                        catch (Exception) { }  // if (json.IsPropertyExist(dict, "Name"))  
                        //string z = Convert.ToString(dict.Type);
                        switch (z)
                        {
                            case "DA":
                                logger.Logged("Info",
                                    "Создаем поллер для DA сервера:  " + Convert.ToString(dict.Connection),
                                    "PollerWatchdog", "GetConfigFromDb");
                                opcPoller = new OpcDaPoller();
                                break;
                            case "UA":
                                logger.Logged("Info",
                                    "Создаем поллер для UA сервера:   " + Convert.ToString(dict.Connection),
                                    "PollerWatchdog", "GetConfigFromDb");
                                opcPoller = new UaPoller();
                                break;
                            default:
                                logger.Logged("Info", "Неподдерживаемый тип OPC: " + opc.Id + ",  " + opc.Name,
                                    "PollerWatchdog", "GetConfigFromDb");
                                break;
                        }

                        if (opcPoller != null)
                        {
                            if (Convert.ToBoolean(dict.Connect))
                            {
                                opcPoller.Connect = true;
                                logger.Logged("Info", "Инициализируем #" + Convert.ToString(dict.Id) + "...", "PollerWatchdog", "GetConfigFromDb");
                                opcPoller.Initialize(dict.Connection.ToString(), Convert.ToInt32(dict.Id));
                            }
                            opcPoller.SubAll = Convert.ToBoolean(dict.SubAll);
                            opcPoller.LastPoll = DateTime.Now;
                            opcPoller.OnUpdateAnalog += OnUpdateAnalog;
                            opcPoller.OnUpdateDiscrete += OnUpdateDiscrete;
                            opcPoller.OnUpdateEmpty += OnUpdateEmpty;
                            opcPoller.OnUpdate += OnUpdate;
                            opcPoller.AddTags(readTags(Convert.ToInt32(dict.Id)));
                            _pollers.Add(opcPoller);
                        }
                    }

                    result = true;
                }
                catch
                    (Exception ex)
                {
                    logger.Logged("Error", "Не удалось считать список OPC-серверов из БД: " + ex.Message,
                        "PollerWatchdog", "GetConfigFromDb");
                    return;
                }


            }
        }

        public List<Dictionary<string, dynamic>> GetOpcInfo()
        {
            var rezList = new List<Dictionary<string, dynamic>>();
            foreach (var poller in _pollers)
            {
                var server = new Dictionary<string, dynamic>();
                server.Add("Id", poller.PollerId);
                server.Add("ConnString", poller.ConnString);
                server.Add("Connected", poller.IsConnected());
                server.Add("Tags", poller.TagListBackup.Count);
                rezList.Add(server);
            }
            return rezList;

        }

        public void Init()
        {
            SaveeSQLTag_Time = DateTime.Now;
            fit_first_load = true;
            _timeout = 0;
            must_break = false;
            _pollers = new List<DataPoller>();
            _tagValues = new List<TagValueContainer>();
            _tagBlackList = new List<TagValueContainer>();
            _fitList = new List<FitValueContainer>();
            GetConfigFromDb();
            Int32.TryParse("300", out _timeout);
            // Int32.TryParse(ConfigurationManager.AppSettings["OPCDA_server_timeout"], out timeout);
            _watchdog = new Thread(Run);
            _watchdog.Start();
            Worked = true;
        }

        public void Stop()
        {
            must_break = true;
            Worked = false;
            Logger.Logged("Info", "Ожидаем завершения потока контроля работоспособности серверов...", "", "Stop");
        }

        public void Run()
        {
            logger.Logged("Info", "Запущен поток контроля работоспособности серверов...", "", "Run");
            Thread.Sleep(5000);
            while (must_break == false)
            {
                if (_timeout != 0)
                    lock (_pollers)
                    {
                        foreach (DataPoller poller in _pollers)
                        {
                            TimeSpan period = DateTime.Now - poller.LastPoll;



                            if (poller.Connect)
                            {
                                if (period.TotalSeconds > _timeout)
                                {
                                    if (poller.Activated && !poller.IsConnected() && poller.checkLastPoll)
                                    {
                                        logger.Logged("Warn", "Что то пошло не так. От сервера #" + poller.PollerId + " не было обновлений более ", "", "Run");
                                        poller.Uninitialize();
                                    }
                                    if ((!poller.Activated || !poller.IsConnected()) && poller.checkLastPoll)
                                    {
                                        logger.Logged("Error", "Сервер #" + poller.PollerId + " не активен, пробуем соединиться...", "", "Run");
                                        poller.Initialize();
                                        poller.AddTags(readTags(poller.PollerId));
                                        poller.LastPoll = DateTime.Now;
                                    }
                                }
                            }

                            if ((poller.Restart == true))
                            {
                                logger.Logged("Info", "Подключение к серверу #" + poller.PollerId + " будет перезапущено...", "", "Run");

                                poller.Connect = true;
                                poller.Uninitialize();
                                poller.Initialize();
                                poller.AddTags(readTags(poller.PollerId));
                                poller.Restart = false;
                            }
                        } 
                    }

                Thread.Sleep(5000);
            }

            logger.Logged("Info", "Поток контроля работоспособности серверов остановлен.", "", "");
        }
 

        //---------Tags from DB--------// 
        //----------------------//
        //
        /*   //---------For WebDataSpy--------//
        public void GetOPCserverTags(int poollerId)
        {
            var _OPCServer = _pollers.Where(x => x.PollerId == poollerId);
            _OPCServer.
        }
        public void SaveeSQLTag(TagId tag, float value, DateTime dt)
        {

        }
        ----------------------//
      //*/
    }
}
