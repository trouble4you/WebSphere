using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
//using Newtonsoft.Json;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using WebSphere.ClientOPC;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete; 
using System.Data.SqlClient;
using System.Data;

namespace WebSphere.Alarms
{
    public class AlarmServer : IAlarmServer
    {
        private static IOpcPoller _opcPoller = new OpcPoller();
        private static EFDbContext context = new EFDbContext();
        private static IJSON _json = new JSON();
        private static readonly Logging logger = new Logging();
        // Теги за которыми следим.
        private static List<AlarmThreadManagerConfig> _cfgs;
        // Актуальные тревоги.
        private static List<AlarmThreadManagerAlarm> _alarms;
        private static List<int> _soundalarms;
        // Интервал проверки
        private static int _delayPeriod;
        private bool _ready;
        // Поток для тревог. 
        private static Thread _thread;
        private static readonly object ConfigLocker = new object();
        private readonly object AlarmLocker = new object();
        private readonly object StructLocker = new object();
        private static bool _stopAlarmServer;

        private const string DtFormat = "yyyy-dd-MM HH:mm:ss";
  
        void IAlarmServer.Run()
        { 
            Run();
        }
        void IAlarmServer.Restart()
        {
            _stopAlarmServer = false;
            Init();
            Run();
        }
        bool IAlarmServer.Init()
        {
            return Init();
        }

        /// <summary>
        /// Инициализирует модуль тревог.
        /// </summary>
        public  bool SoundAlarm()
        {
           var active_alarms=_alarms.Where(x => x.ERes==-10 && (x.Qted == null|| x.Qted == 0)).ToList();
            try
            {
                foreach (var _soundalarm in _soundalarms)
                {
                    if (active_alarms.FirstOrDefault(x => x.TagId == _soundalarm) != null)
                        return true; 
                            //  var rez = _soundalarms.Any(soundalarm => active_alarms.FirstOrDefault(x => x.TagId == soundalarm) != null);

                }
                return false;
                //var rez= _soundalarms.Any(soundalarm => active_alarms.FirstOrDefault(x => x.TagId == soundalarm) != null);
                // return rez;
            }
            catch (Exception ex)
            {
                logger.Logged("Error", " ...SoundAlarm. Reason:" + ex.Message, "AlarmServer", "SoundAlarm");
                return false;
            }
        }
        public static bool Init()
        {
            try
            {
                _stopAlarmServer = true;
                logger.Logged("Info", " Begin inicialization...", "AlarmServer", "Init");
                _delayPeriod =
                    Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["AlarmThreadDelayBetweenCheck"]);
                _cfgs = new List<AlarmThreadManagerConfig>();
                _soundalarms = new List<int>();
                _alarms = new List<AlarmThreadManagerAlarm>();

                logger.Logged("Info", "Loading: alarms conf...", "AlarmServer", "Init");
                LoadAlarmsConfigurations();
                logger.Logged("Info", "Loaded: alarms conf. Loading: alarms last states...", "AlarmServer", "Init");
                LoadAlarmsLastStates();
                logger.Logged("Info", "Loaded: alarms last states.", "AlarmServer", "Init");
                _thread = new Thread(CheckAlarms) { IsBackground = true };
                logger.Logged("Info", " ...Inicialization done.", "AlarmServer", "Init");
                return true;
            }
            catch (Exception ex)
            {
                logger.Logged("Error", " ...Inicialization failed. Reason:" + ex.Message, "AlarmServer", "Init");
                return false;
            }
        }

        /// <summary>
        /// Запускает модуль тревог.
        /// </summary>
        public static void Run()
        {
            try
            {  
                _thread.Start();
                logger.Logged("Info", " [" + _thread.ManagedThreadId + "] запущен.", "AlarmThreadManager", "Run");

            }
            catch (Exception ex)
            {
                logger.Logged("Error", " Run run failed' [" + ex.Message + "]", "AlarmThreadManager", "Run");
            }

        }

        class AlSrc
        {
            public string Tag;
            public string Opc;
            public string Sql;
        }

        /// <summary>
        /// Метод создающий тревоги.
        /// </summary>
        private static void CheckAlarms()
        {
            while (_stopAlarmServer)
            {
                lock (_alarms)
                {
                    lock (ConfigLocker)
                    {
                        //collect alarms sources;


                        // string result = _cfgs.Where(cfg => cfg.Tag != "").Aggregate("", (current, cfg) => current + ("'" + cfg.Tag + "',"));
                        // var s = result.Trim(',');
                        // var qr = MyDB.sql_query_local("SELECT tag, opc, sql from c_tags where tag in(" + s + ");");
                        // var t = new List<AlSrc>();
                        // for (var i = 0; i < qr.count_rows; i++)
                        // {
                        //     t.Add(new AlSrc
                        //     {
                        //         Tag = qr.GetValue(i, 0),
                        //         Opc = qr.GetValue(i, 1),
                        //         Sql = qr.GetValue(i, 2)
                        //     });
                        // }

                        foreach (AlarmThreadManagerConfig config in _cfgs)
                        {
                            if (!config.Enabled) continue;
                            //var src = t.FirstOrDefault(x => x.Tag == config.TagId);
                            //if (src == null) continue;
                            TagValueContainer rawVal = null;
                            if (!String.IsNullOrEmpty(config.Tag.TagName))
                            {
                                rawVal = _opcPoller.ReadTag(config.Tag);
                                //rawVal = OpcThreadsManager.ReadOpcTag("alarm", src.Opc, true);
                                goto calc;
                            }
                        //if (!String.IsNullOrEmpty(src.Sql))
                        //{
                        //    rawVal = _opcclient == 1 ? MOpcThreadManager.ReadOpcTag(src.Opc) : OpcThreadsManager.ReadOpcTag("alarm", src.Opc, true);
                        //    //rawVal = OpcThreadsManager.ReadOpcTag("alarm", src.Opc, true);
                        //    goto calc;
                        //}
                        // continue;
                        calc:
                            // if (rawVal == null || rawVal.LastAnalogValue == null && rawVal.LastDiscreteValue == null) continue;
                            // double val;
                            // if (rawVal.LastValue == null)
                            //     val = rawVal.LastDiscreteValue == true ? 1 : 0;
                            // else 
                            //     val = (double) rawVal.LastAnalogValue;
                            //
                            if (rawVal == null || rawVal.LastValue == null) continue;
                            double val;
                            if (!Double.TryParse(rawVal.LastValue.Replace('.', ','), out val))
                                val = rawVal.LastValue.ToLower() == "true" ? 1 : 0;
                            if (val >= config.HihiSeverity) { ProcessAlarm(config, val, 2); continue; }
                            if (val >= config.HiSeverity) { ProcessAlarm(config, val, 1); continue; }
                            if (val <= config.LoloSeverity) { ProcessAlarm(config, val, -2); continue; }
                            if (val <= config.LoSeverity) { ProcessAlarm(config, val, -1); continue; }
                            ProcessAlarm(config, val, 0);
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private static void ProcessAlarm(AlarmThreadManagerConfig cfg, double value, int reason)
        {
            // Найдем тревогу у которой время окончания - пустое.
            var al = GetAlarm(cfg.TagId);
            // Если среди тревог нет активных тревог.
            if (al == null)
            {
                // И если причина не нормальное состояние.
                if (reason != 0)
                    OpenAlarm(cfg, value, reason);
                return;
            }
            // Если её начальное состояние совпадает с текущим, то похер эта тревога уже и так есть.
            if (al.SRes == reason)
                return;
            // Если её начальное состояние не совпадает с текущим, то зафиксируем окончание этой тревоги.
            CloseAlarm(al, value, reason);

            // И если тревога квитирована. Уберем из списка.
            if (al.Qted != null)
                _alarms.Remove(al);

            // Если состояние не стало нормальным. Добавим новую тревогу.
            if (reason != 0)
                OpenAlarm(cfg, value, reason);
        }

        private static AlarmThreadManagerAlarm GetAlarm(int tagId)
        {
            return _alarms.LastOrDefault(t => t.TagId == tagId && t.ETime == DateTime.MinValue);
        }

        private static void OpenAlarm(AlarmThreadManagerConfig alarmConfig, double value, int reason)
        {
            var dateTime = DateTime.Now;
            var dateTimeString = dateTime.ToString(DtFormat);
            // Добавляем тревогу в базу.
            var rawVal = value.ToString().Replace(',', '.');

            var sysUser = context.Alarms.Create();

            sysUser.TagId = alarmConfig.TagId;
            sysUser.SRes = reason;
            sysUser.STime = dateTime;
            sysUser.SVal = (float)value;
            sysUser.ERes = -10;

            context.Alarms.Add(sysUser);
            // сохраняем
            context.SaveChanges();


            //MyDB.sql_query_local("insert into alarms (Tag, SRes, STime, SVal, ERes) values('" + alarmConfig.Tag + "'," + reason + ",'" + dateTimeString + "', " + rawVal + ", -10);");
            //var qres2 = MyDB.sql_query_local("select id from alarms where tag='" + alarmConfig.Tag + "' and STime='" + dateTimeString + "'");
            //int tid = -1;
            //if (qres2.count_rows > 0)
            //    tid = Convert.ToInt32(qres2.GetValue(0, 0));
            var z = (from ti in context.Alarms
                     where ti.TagId == alarmConfig.TagId //&& ti.STime == dateTime
                     orderby ti.STime descending
                     select ti.Id);

            // from al context.Alarms .Where(d => d.TagId == alarmConfig.TagId && d.STime == dateTime).ToList();
            var id = z.ToList();// Создадим тревогу чтобы не дергать каждый раз базу.
            var result = new AlarmThreadManagerAlarm
            {
                Id = id.First(),
                TagId = alarmConfig.TagId,
                SRes = reason,
                STime = dateTime,
                SVal = value,
                Qted = null,
                ERes = -10,
                ETime = DateTime.MinValue
            };
            _alarms.Add(result);
            logger.Logged("Info", "add alarm [" + alarmConfig.TagId + "][" + reason + "] to alarmserver...", "AlarmThreadManager", "OpenAlarm");

        }

        private static void CloseAlarm(AlarmThreadManagerAlarm alarm, double value, int reason)
        {
            try
            {
                var dt2 = DateTime.Now;
                var dts2 = dt2.ToString(DtFormat);

                var rawVal = value.ToString().Replace(',', '.');
                var alarmtab = context.Alarms.FirstOrDefault(u => u.Id == alarm.Id);


                //MyDB.sql_query_local("update alarms set ERes=" + reason + ", ETime='" + dts2 + "', EVal=" + rawVal + " WHERE Id=" + alarm.Id + ";");
                if (alarmtab != null)
                {
                    alarmtab.ERes = reason;
                    alarmtab.ETime = dt2;
                    alarmtab.EVal = (float)value;
                    context.SaveChanges();
                    alarm.ERes = reason;
                    alarm.ETime = dt2;
                    alarm.EVal = value;
                } logger.Logged("Info", "close alarm [" + alarm.TagId + "][" + reason + "] to alarmserver...", "AlarmThreadManager", "CloseAlarm");


            }
            catch (Exception ex)
            {
                logger.Logged("Error", "close alarm [" + alarm.TagId + "][" + reason + "] failed...", "AlarmThreadManager", "CloseAlarm");

            }

        }

        // Загружает конфигурацию тревог при старте системы.
        public static void LoadAlarmsConfigurations()
        {
            lock (ConfigLocker)
            {
                _cfgs.Clear();
                _soundalarms.Clear();
                var taglist = (from ti in context.Objects
                            join to in context.Properties on ti.Id equals to.ObjectId
                            where ti.Type == 2 && to.PropId == 0
                            select new { Id = ti.Id, Prop = to.Value });
                ;
                //var taglist = tags.ToList();

                //foreach (var tag in tags)
                foreach (var tagjson in taglist)
                { 
                     
                    dynamic alarm = JsonConvert.DeserializeObject(tagjson.Prop); 

                   var tag = new TagId
                    {
                        TagName = Convert.ToString(alarm.Connection),
                        PollerId = Convert.ToInt32(alarm.Opc)
                    };
                    
                    try
                    {
                        var tagId = Convert.ToInt32(tagjson.Id);
                        var enabled = Convert.ToBoolean(alarm.Alarm_IsPermit);
                        var active = Convert.ToBoolean(alarm.Alarm_IsPermit);

                        var hihiText = Convert.ToString(alarm.hihiText);
                        var hiText = Convert.ToString(alarm.hiText);
                        var normalText = Convert.ToString(alarm.normalText);
                        var loText = Convert.ToString(alarm.loText);
                        var loloText = Convert.ToString(alarm.loloText);

                        var audio = Convert.ToBoolean(alarm.Audio);

                        double hihiSeverity;
                        if (!Double.TryParse(Convert.ToString(alarm.hihiSeverity), out hihiSeverity))
                            hihiSeverity = Double.MaxValue;
                        double hiSeverity;
                        if (!Double.TryParse(Convert.ToString(alarm.hiSeverity), out hiSeverity))
                            hiSeverity = Double.MaxValue;
                        double loSeverity;
                        if (!Double.TryParse(Convert.ToString(alarm.loSeverity), out loSeverity))
                            loSeverity = Double.MinValue;
                        double loloSeverity;
                        if (!Double.TryParse(Convert.ToString(alarm.loloSeverity), out loloSeverity))
                            loloSeverity = Double.MinValue;
                         
                        _cfgs.Add(new AlarmThreadManagerConfig
                        {
                            Tag = tag,
                            TagId = tagId,
                            Enabled = enabled,
                            Active = active,

                            HihiText = hihiText,
                            HiText = hiText,
                            NormalText = normalText,
                            LoText = loText,
                            LoloText = loloText,

                            HihiSeverity = hihiSeverity,
                            HiSeverity = hiSeverity,
                            LoSeverity = loSeverity,
                            LoloSeverity = loloSeverity
                        });
                        if (audio)
                            _soundalarms.Add(tagId);
                        logger.Logged("Info",
                            "add tag [" + tag.PollerId + "][" + tag.TagName + "] to alarmserver...",
                            "AlarmThreadManager", "LoadAlarmsLastStates");
                    }
                    catch (Exception ex)
                    {
                        logger.Logged("Err",
                            "add tag [" + tag.PollerId + "][" + tag.TagName + "] to alarmserver failed: " + ex.Message,
                            "AlarmThreadManager", "LoadAlarmsLastStates");
                    }
                }
            }

        }


        // Перезагружает конфигурацию одной тревоги. Если к примеру изменили её через конфигуратор тревог.
        public void LoadOneAlarm(int tagid)
        {
            lock (ConfigLocker)
            {
                if (tagid == null)
                {
                    logger.Logged("Error", " TagName is null or empty.", "AlarmThreadManager", "LoadOneAlarm");
                    return;
                }
                AlarmThreadManagerConfig curTag;
                // var cRes = MyDB.sql_query_local("SELECT * FROM Signal_Alarm_Messages WHERE input_source='" + tagName + "';");
                // if (cRes.count_rows > 0)
                // {

                var tags = (from ti in context.Objects
                            join to in context.Properties on ti.Id equals to.ObjectId
                            where ti.Type == 2 && to.PropId == 0 && ti.Id == tagid
                            select to.Value);

                var taglist = tags.ToList();
                // var cRes = MyDB.sql_query_local("SELECT * FROM Signal_Alarm_Messages;");
                foreach (var tagjson in taglist)
                {

                    dynamic alarm = _json.Deserialize(tagjson);

                    var tag = Convert.ToString(alarm.Connection);
                    var tagId = Convert.ToInt32(alarm.Id);
                    var enabled = Convert.ToBoolean(alarm.enabled);
                    var active = Convert.ToBoolean(alarm.active);

                    var hihiText = Convert.ToString(alarm.hihiText);
                    var hiText = Convert.ToString(alarm.hiText);
                    var normalText = Convert.ToString(alarm.normalText);
                    var loText = Convert.ToString(alarm.loText);
                    var loloText = Convert.ToString(alarm.loloText);


                    double hihiSeverity;
                    if (!Double.TryParse(alarm.hihiSeverity, out hihiSeverity)) hihiSeverity = Double.MaxValue;
                    double hiSeverity;
                    if (!Double.TryParse(alarm.hiSeverity, out hiSeverity)) hiSeverity = Double.MaxValue;
                    double loSeverity;
                    if (!Double.TryParse(alarm.loSeverity, out loSeverity)) loSeverity = Double.MinValue;
                    double loloSeverity;
                    if (!Double.TryParse(alarm.loloSeverity, out loloSeverity)) loloSeverity = Double.MinValue;

                    curTag = new AlarmThreadManagerConfig
                    {
                        Tag = tag,
                        TagId = tagId,
                        Enabled = enabled,
                        Active = active,

                        HihiText = hihiText,
                        HiText = hiText,
                        NormalText = normalText,
                        LoText = loText,
                        LoloText = loloText,

                        HihiSeverity = hihiSeverity,
                        HiSeverity = hiSeverity,
                        LoSeverity = loSeverity,
                        LoloSeverity = loloSeverity
                    };


                    var index = -1;
                    for (var cfgi = 0; cfgi < _cfgs.Count; cfgi++)
                    {
                        if (_cfgs[cfgi].TagId == tagid)
                        {
                            index = cfgi;
                            break;
                        }
                    }

                    if (index != -1)
                        _cfgs[index] = curTag;
                    else
                        _cfgs.Add(curTag);
                }
            }
        } 
        /// <summary>
        /// Загружает состояния тревог при запуске системы.
        /// </summary>         
        public static void LoadAlarmsLastStates()
        {
            logger.Logged("Info", "Begin load last alarms...", "AlarmThreadManager", "LoadAlarmsLastStates");
            var day3Before = DateTime.Now.AddDays(-3);
            foreach (AlarmThreadManagerConfig t in _cfgs)
            {
                var alarm =
                    context.Alarms.Where(x => (x.ERes == -10 && x.TagId == t.TagId && x.STime > day3Before))
                        .OrderByDescending(x => x.STime)
                        .FirstOrDefault();

                if (alarm != null)
                {
                    int id = Convert.ToInt32(alarm.Id);
                    int tag = Convert.ToInt32(alarm.TagId);
                    int sres = Convert.ToInt32(alarm.SRes);
                    DateTime stime = Convert.ToDateTime(alarm.STime);
                    double sval = Convert.ToDouble(alarm.SVal);
                    int eres = Convert.ToInt32(alarm.ERes);
                    DateTime etime = Convert.ToDateTime(alarm.ETime);
                    double eval = Convert.ToDouble(alarm.EVal);
                    DateTime acktime = Convert.ToDateTime(alarm.AckTime);
                    int ack = Convert.ToInt32(alarm.Ack);
                    _alarms.Add(new AlarmThreadManagerAlarm
                    {
                        Id = id,
                        TagId = tag,
                        SRes = sres,
                        STime = stime,
                        SVal = sval,
                        ERes = eres,
                        ETime = etime,
                        EVal = eval,
                        QTime = acktime,
                        Qted = ack
                    });
                    logger.Logged("Info", "Load last alarm [tag:" + tag + ";sval:" + sval + ";  STime:" + stime + " ]...", "AlarmThreadManager", "LoadAlarmsLastStates");

                }
            }
            logger.Logged("Info", "...End", "AlarmThreadManager", "LoadAlarmsLastStates");
        }

        public List<AlarmThreadManagerOut> GetCurrentAlarms()
        {
            var result = new List<AlarmThreadManagerOut>();
            for (var ai = 0; ai < _alarms.Count; ai++)
            {
                var tm = _cfgs.FirstOrDefault(x => x.TagId == _alarms[ai].TagId) ?? new AlarmThreadManagerConfig();
                var outAlarm = new AlarmThreadManagerOut();
                outAlarm.Id = _alarms[ai].Id;
                outAlarm.TagId = _alarms[ai].TagId;
                _alarms[ai].SVal = Math.Round(_alarms[ai].SVal, 2);
                _alarms[ai].EVal = Math.Round(_alarms[ai].EVal, 2);
                var sr = "";
                switch (_alarms[ai].SRes)
                {
                    case -2:sr = tm.LoloText; break;
                    case -1:sr = tm.LoText; break;
                    case 0: sr= tm.NormalText; break;
                    case 1: sr= tm.HiText; break;
                    case 2: sr= tm.HihiText; break;
                }
                outAlarm.StartReason = sr + " (" + Math.Round(_alarms[ai].SVal, 2) + ")";
                //outAlarm.StartReason = _alarms[ai].SRes;
                outAlarm.StartValue = _alarms[ai].SVal;
                outAlarm.StartTime = _alarms[ai].STime;
                //outAlarm.StartValue = _alarms[ai].SVal;
                sr = "";
                tm.Active = false;
                switch (_alarms[ai].ERes)
                {
                    case -2:sr = tm.LoloText; break;
                    case -1:sr = tm.LoText; break;
                    case 0: sr= tm.NormalText; break;
                    case 1: sr= tm.HiText; break;
                    case 2: sr= tm.HihiText; break;
                    case -10:sr = "Тревога активна"; 
                        outAlarm.Active = true; break;
                }
                outAlarm.EndReason = sr + " (" + Math.Round(_alarms[ai].EVal, 2) + ")";
                //outAlarm.EndReason = _alarms[ai].ERes;
                outAlarm.EndValue = _alarms[ai].EVal;
                outAlarm.EndTime = _alarms[ai].ETime;
                //outAlarm.EndValue = _alarms[ai].EVal;

                outAlarm.AckTime = _alarms[ai].QTime;
                outAlarm.Ack = _alarms[ai].Qted;
                var Duration =new TimeSpan();
                if (_alarms[ai].ETime==DateTime.MinValue)
                      Duration = DateTime.Now - _alarms[ai].STime;
                else Duration = _alarms[ai].ETime-_alarms[ai].STime ;
                outAlarm.Duration=new TimeSpan(Duration.Days, Duration.Hours, Duration.Minutes, Duration.Seconds);
                result.Add(outAlarm);
            }
            return result.OrderBy(x => x.StartTime).ToList();
        }

        public List<AlarmThreadManagerOut> GetAlarmsReport (string dt1, string dt2)
        {  var Sdate=  new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            var Edate=  new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,24, 0, 0);
                if (dt1!=null)
                     Sdate = Convert.ToDateTime(dt1);
                if (dt2!=null)
                     Edate = Convert.ToDateTime(dt2);
            var result = new List<AlarmThreadManagerOut>();
            var alarmslist = context.Alarms.Where(x => x.STime >= Sdate && x.STime <= Edate).OrderByDescending(x => x.STime);
            var alarms = alarmslist.ToList();
            for (var ai = 0; ai < _alarms.Count; ai++)
            {
                var tm = _cfgs.FirstOrDefault(x => x.TagId == _alarms[ai].TagId) ?? new AlarmThreadManagerConfig();
                var outAlarm = new AlarmThreadManagerOut();
                outAlarm.Id = _alarms[ai].Id;
                outAlarm.TagId = _alarms[ai].TagId;
                _alarms[ai].SVal = Math.Round(_alarms[ai].SVal, 2);
                _alarms[ai].EVal = Math.Round(_alarms[ai].EVal, 2);
                var sr = "";
                switch (_alarms[ai].SRes)
                {
                    case -2: sr = tm.LoloText; break;
                    case -1: sr = tm.LoText; break;
                    case 0:  sr = tm.NormalText; break;
                    case 1:  sr = tm.HiText; break;
                    case 2:  sr = tm.HihiText; break;
                }
                //outAlarm.StartReason = _alarms[ai].SRes; 
                outAlarm.StartReason = sr + " (" + Math.Round(_alarms[ai].SVal, 2) + ")";
                    
                outAlarm.StartValue = _alarms[ai].SVal;
                outAlarm.StartTime = _alarms[ai].STime;
                //outAlarm.StartValue = _alarms[ai].SVal;
                sr = "";
                tm.Active = false;
                switch (_alarms[ai].ERes)
                {
                    case -2:sr  = tm.LoloText; break;
                    case -1:sr  = tm.LoText; break;
                    case 0: sr = tm.NormalText; break;
                    case 1: sr = tm.HiText; break;
                    case 2: sr = tm.HihiText; break;
                    case -10:// outAlarm.Message = "Тревога активна"; 
                        outAlarm.Active = true; break;
                }
                outAlarm.StartReason = sr + " (" + Math.Round(_alarms[ai].EVal, 2) + ")";
                //outAlarm.EndReason = _alarms[ai].ERes;
                outAlarm.EndValue = _alarms[ai].EVal;
                outAlarm.EndTime = _alarms[ai].ETime;
                //outAlarm.EndValue = _alarms[ai].EVal;

                outAlarm.AckTime = _alarms[ai].QTime;
                outAlarm.Ack = _alarms[ai].Qted;
                result.Add(outAlarm);
            }
            return result.OrderBy(x => x.StartTime).ToList();
        }

        public void ChangeAlarmState(int tagID, bool state)
        {
            lock (ConfigLocker)
            {
                var index = -1;
                for (var cfgi = 0; cfgi < _cfgs.Count; cfgi++)
                {
                    if (_cfgs[cfgi].TagId == tagID)
                    {
                        index = cfgi;
                        break;
                    }
                }
                if (index != -1)
                    _cfgs[index].Enabled = state;
            }
        }


        public void SetAlarmAck(int alarmId)
        {
            lock (AlarmLocker)
            {
                DateTime myDateTime = DateTime.Now;
                // Квитируем тревогу в базе.
                // MyDB.sql_query_local("UPDATE Alarms SET AckTime='" + myDateTime.ToString(DtFormat) + "', Ack=1 WHERE Id=" + alarmId + ";");

                var alarm =
                        context.Alarms.FirstOrDefault(x => (x.Id == alarmId));// Посмотрим есть ли в списке тревог эта тревога. 
                if (alarm != null)
                {
                    alarm.Ack = 1;
                    alarm.AckTime = DateTime.Now; ;
                    context.SaveChanges();
                } // Может быть ситуация когда слежение за тревогой убрали и осталась она только в базе с состояние "не квитировано"
                var alarmIdI = Convert.ToInt32(alarmId);
                var index = -1;
                for (var ai = 0; ai < _alarms.Count; ai++)
                {
                    if (_alarms[ai].Id == alarmIdI)
                    {
                        index = ai;
                        break;
                    }
                }
                // Если нашли тревогу.
                if (index != -1)
                {
                    // Не убираем из списка только если текущая тревога - активная. 
                    if (_alarms[index].ERes == -10)
                    {
                        _alarms[index].Qted = 1;
                        _alarms[index].QTime = DateTime.Now;
                        return;
                    }
                    _alarms.RemoveAt(index);
                }

            }
        }

        private class Indian001
        {
            public int Id;
        }

        public void SetAlarmAckAll()
        {
            var ind001 = _alarms.Select(alarm => new Indian001 { Id = alarm.Id }).ToList();
            foreach (var item in ind001)
                SetAlarmAck(item.Id);
        }

        public void DeleteAlarm(int tagId)
        {
            lock (ConfigLocker)
            {
                var index = -1;
                for (var cfgi = 0; cfgi < _cfgs.Count; cfgi++)
                {
                    if (_cfgs[cfgi].TagId == tagId)
                    {
                        index = cfgi;
                        break;
                    }
                }
                if (index != -1)
                    _cfgs.RemoveAt(index);
            }
        }

        public List<AlarmThreadManagerConfig> GetAlarmStates()
        {
            return _cfgs;
        }

    }

}
