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
        public DateTime LastPoll;
        public string ConnString;
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

        public class MySQLResult
        {
            public List<string> columns = new List<string>();
            public List<MySQLRow> rows = new List<MySQLRow>();
            public int count_rows;

            public String GetValue(int row, string columnName)
            {
                // Получитьь значение по номеру строки и названию колонки
                if (row >= count_rows)
                    return "*";
                for (var i = 0; i < columns.Count; i++)
                    if (columns[i] == columnName)
                        return rows[row].values[i];
                return "*";
            }

            public bool SetValue(int row, string columnName, string value)
            {
                if (row >= count_rows)
                    return false;
                for (var i = 0; i < columns.Count; i++)
                    if (columns[i] == columnName)
                    {
                        rows[row].values[i] = value;
                        return true;
                    }
                return false;
            }



            public string GetValue(int rowIndex, int columnIndex)
            {
                return rows[rowIndex].values[columnIndex];
            }
        }

        public class MyDB
        {
            private static readonly string ConnectionStringLocal =
                System.Configuration.ConfigurationSettings.AppSettings["MyConnection"];

            public static MySQLResult sql_query_local(string query)
            {
                {
                    var result = new MySQLResult {count_rows = 0};
                    // Выполняем запрос.
                    var dataSet = new DataSet();
                    //string connectionString = System.Configuration.ConfigurationSettings.AppSettings["MyConnection"];
                    string connectionString = ConnectionStringLocal;
                    using (var connection = new SqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();
                            var dbAdapter = new SqlDataAdapter(query, connection);
                            dbAdapter.Fill(dataSet);
                            connection.Close();
                        }
                        catch (Exception ex)
                        {
                            if (connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    // Заполняем данные.
                    if (dataSet.Tables.Count == 0)
                        return result;
                    var table = dataSet.Tables[0];
                    // Собираем имена.
                    for (var columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                        result.columns.Add(table.Columns[columnIndex].ColumnName);
                    // Собираем данные.
                    for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                    {
                        var row = table.Rows[rowIndex];
                        var newRow = new MySQLRow();
                        for (var columnIndex = 0; columnIndex < result.columns.Count; columnIndex++)
                            newRow.values.Add(row[columnIndex].ToString());
                        result.rows.Add(newRow);
                        result.count_rows++;
                    }
                    return result;
                }
            }


        }

        private static readonly object TagLocker = new object();
        private static EFDbContext context = new EFDbContext();
        private static readonly Logging logger = new Logging(); 
        static bool fit_first_load = false;
        static bool must_break = false;
        private static List<DataPoller> _pollers;
        private static List<TagValueContainer> _tagValues;
        private static List<TagValueContainer> _tagBlackList;
        private static List<FitValueContainer> _fitList;

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
                        _tagValues.RemoveAt(index);
                        _tagValues.Add(new TagValueContainer
                        {
                            Tag = tag,
                            LastValue = value,
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
                            LastValue = value,
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
            var firstOrDefault = _pollers.FirstOrDefault(d => d.PollerId == tag.PollerId);
            if (firstOrDefault != null)
                return firstOrDefault.WriteTag(tag, value);
            else return false;
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
                            firstOrDefault.ReadOpcTag(new TagId {PollerId = dict["Opc"], TagName = dict["Connection"]});

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
                    var tags = (from ti in context.Objects.Where(x => x.Type == 2 && x.ParentId == opc_id)
                        join to in context.Properties.Where(x => x.PropId == 0) on ti.Id equals to.ObjectId

                        select new {Id = ti.Id, Prop = to.Value});
                    //var tags = from  to context.Objects

                    foreach (var tag in tags)
                    {
                        try
                        {
                            var dict = jss.Deserialize<Dictionary<string, dynamic>>(tag.Prop);
                            var newTag = new TagId {PollerId = opc_id, TagName = dict["Connection"]};
/*
                            tag_database[new_tag] = value_container;
                            //logger.Logged("Info", "Добавлен тэг {0} ({1})", tag.signal_id, tag.opc);
                    */
                            resultTags.Add(newTag);

                            logger.Logged("Info", "Тэг " + newTag.TagName + " (" + newTag.PollerId + ")  добавлен",
                                "PollerWatchdog", "readTags");
                        }
                        catch (Exception ex)
                        {

                            logger.Logged("Error", "Тэг " + tag.Id + " (" + tag.Prop + ") не добавлен: " + ex.Message,
                                "PollerWatchdog", "readTags");
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
                        select to.Value);
                    var opclist = opcs.ToList();
                    //opclist.Add("{\"Id\":\"1\",\"Type\":\"UA\",\"Connection\":\"opc.tcp://Q3DM6:51212/\"}");


                    foreach (var opc in opclist)
                    {
                        dynamic dict = JsonConvert.DeserializeObject(opc);
                        DataPoller opcPoller = null;
                        string z = Convert.ToString(dict.Type);
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
                                //logger.Error("Неподдерживаемый тип OPC: {0}, строка подключения '{1}'", opc.type_id, opc.connect);
                                break;
                        }

                        if (opcPoller != null && Convert.ToBoolean(dict.Connect))
                        {
                            logger.Logged("Info", "Инициализируем #" + Convert.ToString(dict.Id) + "...",
                                "PollerWatchdog", "GetConfigFromDb");
                            opcPoller.Initialize(dict.Connection.ToString(), Convert.ToInt32(dict.Id));
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
                    //logger.Warn("Повтрим попытку через 5 секунд...");
                    Thread.Sleep(5000);
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
                            if (period.TotalSeconds > _timeout)
                            {
                                logger.Logged("Warn",
                                    "От сервера #" + poller.PollerId + " не было обновлений более " + _timeout +
                                    " секунд, пересоединяемся...", "", "Run");
                                poller.Uninitialize();
                            }

                            if (((poller.Activated == false) || (poller.IsConnected() == false) && poller.checkLastPoll))
                            {
                                logger.Logged("Error",
                                    "Сервер #" + poller.PollerId + " не активен, пробуем соединиться...", "", "Run");
                                poller.Initialize();
                                poller.AddTags(readTags(poller.PollerId));
                            }

                            if ((poller.Restart == true))
                            {
                                logger.Logged("Info",
                                    "Подключение к серверу #" + poller.PollerId + " будет перезапущено...", "", "Run");

                                poller.Uninitialize();
                                poller.Initialize();
                                poller.AddTags(readTags(poller.PollerId));
                                poller.Restart = false;
                            }
                        }
                        updateSQLTags();
                    }

                Thread.Sleep(5000);
            }

            logger.Logged("Info", "Поток контроля работоспособности серверов остановлен.", "", "");
        }

        public string ProcFl(string value, int k)
        {
            return string.IsNullOrEmpty(value)
                ? ""
                : Convert.ToString(Math.Round(Convert.ToDouble(value.Replace(".", ",")), k));
        }

        public string GetTop1ValBetweenDatesById(int id, DateTime dt1, DateTime dt2)
        {
            return "select top 1 Value from SignalsAnalogs where TagId=" + id.ToString() + " and Datetime between '" +
                   dt1.ToString() + "' and '" + dt2.ToString() + "' order by Datetime desc";
        }

        public string GetTop1ValBeforeDatesById(int id, DateTime dt1)
        {
            return "select top 1 Value from SignalsAnalogs where TagId=" + id.ToString() + " and Datetime <= '" +
                   dt1.ToString() + "'  order by Datetime desc";
        }

        public void updateSQLTags()
        {
            if (fit_first_load)
            {

            logger.Logged("Info", "Загрузка данных в  расходомеров.", "", "");
                var list_sum_id = new List<int> {721};
                 
                for (int i=0;i< list_sum_id.Count;i++)
                {
                    logger.Logged("Info", "Расходомер fit0"+i+" с суммарным тегом " + list_sum_id[i] + ".", "", "");
                    var startDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 2, 0, 0); 
                    var startLastDay = startDay.AddDays(-1);

                    var now2Hour = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                    now2Hour = now2Hour.Hour % 2>0 ? now2Hour.AddHours(-1) : now2Hour;

                    var last2Hour = now2Hour.AddHours(-2);

                    logger.Logged("Info", "Загрузка дат: начало предыдущих суток {"+ startLastDay + "},начало текущих суток {" + startDay + "}," +
                                          "начало предыдущих 2 часов {" + last2Hour + "},начало текущих 2 часов {" + now2Hour + "}." +
                                          " Текущее время {" + DateTime.Now + "}.", "", "");
                    var sumBeforeLastDay=
                    Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(list_sum_id[i], startLastDay)).GetValue(0, 0), 1));

                    logger.Logged("Info", "Расход на начало предыдущих суток: " + sumBeforeLastDay + "/{" + startLastDay + "}.", "", "");

                    var sumLastDay = Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(list_sum_id[i], startDay)).GetValue(0, 0), 1));

                    logger.Logged("Info", "Расход на начало текущих суток: " + sumLastDay + "/{" + startDay + "}.", "", "");

                    var sumLast2Hour = Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(list_sum_id[i], last2Hour)).GetValue(0, 0), 1));

                    logger.Logged("Info", "Расход на начало предыдущих 2 часов: " + sumLast2Hour + "/{" + last2Hour + "}.", "", "");

                    var sumNow2Hour = Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(list_sum_id[i], last2Hour)).GetValue(0, 0), 1));

                    logger.Logged("Info", "Расход на начало текущих 2 часов: " + sumNow2Hour + "/{" + startDay + "}.", "", "");

                    var sumNow = Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(list_sum_id[i], DateTime.Now)).GetValue(0, 0), 1));

                    logger.Logged("Info", "Расход на  текущее время: " + sumNow + "/{" + DateTime.Now + "}.", "", "");

                    _fitList.Add(new FitValueContainer { Name ="fit0"+i,
                        Id = list_sum_id[i],
                        sumNow = (float)(sumNow), 
                        summlastDay = (float)  ( sumBeforeLastDay),
                        summtoDay = (float)(sumLastDay),
                        summlast2H = (float)(sumLast2Hour),
                        summto2H = (float)(sumNow2Hour),

                        lastDay_Time = startDay, 
                        last2H_Time = last2Hour,
                        to2H_Time = now2Hour,
                        lastDay = (float) (sumLastDay- sumBeforeLastDay), 
                        toDay = (float) (sumNow-sumLastDay),
                        last2H =(float)(sumNow2Hour-sumLast2Hour),
                        to2H = (float)(sumNow-sumNow2Hour)
                    });
                }
                fit_first_load = false;
            }
            else
            {
                foreach (var fit in _fitList)
                {
                    var sumNow = Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(fit.Id, DateTime.Now)).GetValue(0, 0), 1));
                    fit.sumNow = (float) sumNow;
                   // fit.toDay = fit.sumNow - fit.summtoDay;
                   fit.to2H = fit.sumNow - fit.summto2H;
/*
                    //if (DateTime.Now > fit.lastDay_Time.AddDays(1))
                        if (DateTime.Now > fit.lastDay_Time.AddMinutes(2))
                        {
                        fit.lastDay = fit.toDay; 
                        fit.summlastDay = fit.summtoDay;
                        fit.summtoDay = fit.sumNow;

                        fit.last2H = fit.to2H;
                        fit.summlast2H = fit.summto2H;
                        fit.summto2H = fit.sumNow;
                        
                        //fit.lastDay_Time = fit.lastDay_Time.AddDays(1);
                            fit.lastDay_Time = DateTime.Now;

                        }
 */                   //if (DateTime.Now > fit.to2H_Time.AddHours(2))
                        if (DateTime.Now > fit.to2H_Time.AddSeconds(60))
                        {

                        fit.last2H = fit.to2H;
                        fit.summlast2H = fit.summto2H;
                        fit.summto2H = fit.sumNow; 
                        //fit.to2H_Time = fit.to2H_Time.AddHours(2);
                        fit.to2H_Time = DateTime.Now;
                    }


                    OnUpdate(new TagId { PollerId = 0, TagName = fit.Name + ".lastDay" }, Convert.ToString(fit.last2H), DateTime.Now, 192);
                    OnUpdate(new TagId { PollerId = 0, TagName = fit.Name + ".last2H"  }, Convert.ToString(fit.last2H), DateTime.Now, 192);
                    OnUpdate(new TagId { PollerId = 0, TagName = fit.Name+".toDay" }, Convert.ToString(fit.toDay), DateTime.Now, 192);
                    OnUpdate(new TagId { PollerId = 0, TagName = fit.Name + ".to2H" }, Convert.ToString(fit.to2H), DateTime.Now, 192);

                }
            }   
             
           
        }

    }
}
 