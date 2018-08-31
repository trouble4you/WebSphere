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
        private static List<EventThreadManagerConfig> _cfgsEv;
        // Актуальные тревоги.
        private static List<AlarmThreadManagerAlarm> _alarms;
        private static List<EventThreadManagerEvent> _events;
        private static List<int> _soundalarms;


        private static List<Obj> _objects;
        private static List<Obj> _tags;

        public class Obj
        {
            public int Id;
            public int Type;
            public int? Parent;
            public string Name;
            public string Prop;


        }
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
        bool IAlarmServer.Restart()
        {
            try
            {
                _stopAlarmServer = false;
                Init();
                Run();
                return true;
            }
            catch (Exception ex)
            {
                logger.Logged("ERR", "Restart alarms failed..." + ex.Message, "AlarmServer", "Restart");
                return false;
            }

        }
        bool IAlarmServer.Init()
        {
            return Init();
        }

        public List<int> GetChildTags(int? parID)
        {
            var rez = new List<int>();
            var root = _objects.Where(c => c.Id == parID).Select(c => c).FirstOrDefault();
            var childs = _objects.Where(c => c.Parent == parID && (c.Type == 21 || c.Type == 2 || c.Type == 5)).ToList();
            foreach (var child in childs)
            {
                if (child.Id == 0) continue;
                rez.AddRange(GetChildTags(child.Id));
            }
            if (root.Type == 2)
            {
                rez.Add(root.Id);
            }
            return rez;
        }
        /// <summary>
        /// Инициализирует модуль тревог.
        /// </summary>
        public List<EventAlertManager> SoundAlarm()
        {
            try
            {
                var _alerts = new List<EventAlertManager>();
                var active_alarms = _alarms.Where(x => x.ERes == -10 && (x.Qted == null || x.Qted == 0)).ToList();
                foreach (var _alarm in active_alarms)
                {


                    if (_soundalarms.FirstOrDefault(x => x == _alarm.TagId) != null)
                    {
                        var tm = _cfgs.FirstOrDefault(x => x.TagId == _alarm.TagId) ?? new AlarmThreadManagerConfig();
                        var sr = "";
                        switch (_alarm.SRes)
                        {
                            case -2: sr = tm.LoloText; break;
                            case -1: sr = tm.LoText; break;
                            case 0: sr = tm.NormalText; break;
                            case 1: sr = tm.HiText; break;
                            case 2: sr = tm.HihiText; break;
                        }
                        _alerts.Add(new EventAlertManager { Id = _alarm.Id, Message = _alarm.STime + ":" + sr });
                    }
                }
                return _alerts;
            }
            // }  
            //
            //     foreach (var _soundalarm in _soundalarms)
            //     {
            //         if (active_alarms.FirstOrDefault(x => x.TagId == _soundalarm) != null)
            //             return true; 
            //                 //  var rez = _soundalarms.Any(soundalarm => active_alarms.FirstOrDefault(x => x.TagId == soundalarm) != null);
            // }
            //     return false;
            //var rez= _soundalarms.Any(soundalarm => active_alarms.FirstOrDefault(x => x.TagId == soundalarm) != null);
            // return rez;

            catch (Exception ex)
            {
                logger.Logged("Error", " ...SoundAlarm. Reason:" + ex.Message, "AlarmServer", "SoundAlarm");
                return null;
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
                _cfgsEv = new List<EventThreadManagerConfig>();
                _soundalarms = new List<int>();
                _alarms = new List<AlarmThreadManagerAlarm>();
                _events = new List<EventThreadManagerEvent>();

                LoadObjSign();
                logger.Logged("Info", "Loading: LoadObjSign ..." + _objects.Count(), "AlarmServer", "Init");
                logger.Logged("Info", "Loading: alarms conf...", "AlarmServer", "Init");
                LoadAlarmsConfigurations();
                logger.Logged("Info", "Loaded: alarms conf.Alarm count:" + _cfgs.Count() + ". Event count:" + _cfgsEv.Count() + ". Loading: alarms last states...", "AlarmServer", "Init");
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



        /// <summary>
        /// Метод создающий тревоги.
        /// </summary>
        private static void CheckAlarms()
        {
            while (_stopAlarmServer)
            {
                lock (_events)
                {
                    lock (ConfigLocker)
                    {
                        foreach (EventThreadManagerConfig config in _cfgsEv)
                        {
                            if (!config.Active) continue;
                            TagValueContainer rawVal = null;
                            if (!String.IsNullOrEmpty(config.Tag.TagName))
                            {
                                rawVal = _opcPoller.ReadTag(config.Tag);
                                //rawVal = OpcThreadsManager.ReadOpcTag("alarm", src.Opc, true);
                                goto calc;
                            }
                            calc:
                            if (rawVal == null || rawVal.LastValue == null) continue;
                            double val;
                            if (!Double.TryParse(rawVal.LastValue.Replace('.', ','), out val))
                                val = rawVal.LastValue.ToLower() == "true" ? 1 : 0;
                            if (val != config.LastValue)
                            {
                                foreach (var _event in config.EventMessages)
                                {
                                    if (val == _event.Value)
                                    {
                                        config.LastValue = val;
                                        logger.Logged("Info", "Add event [tag:" + config.Tag + ";sval:" + config.LastValue + ";  STime:" + DateTime.Now + " ]...", "AlarmThreadManager", "LoadAlarmsLastStates");
                                        AddEvent(config, _event.Value); continue;
                                    }
                                }
                            }

                        }
                    }
                }

                lock (_alarms)
                {
                    lock (ConfigLocker)
                    {
                        foreach (AlarmThreadManagerConfig config in _cfgs)
                        {
                            if (!config.Permit) continue;
                            //var src = t.FirstOrDefault(x => x.Tag == config.TagId);
                            //if (src == null) continue;
                            TagValueContainer rawVal = null;
                            if (!String.IsNullOrEmpty(config.Tag.TagName))
                            {
                                rawVal = _opcPoller.ReadTag(config.Tag.TagName);
                                //rawVal = OpcThreadsManager.ReadOpcTag("alarm", src.Opc, true);
                                goto calc;
                            }
                            calc:
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
            logger.Logged("Info", "Add Alarm [tag:" + alarmConfig.Tag + ";sval:" + sysUser.SVal + ";  STime:" + dateTime + " ]...", "AlarmThreadManager", "LoadAlarmsLastStates");

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
        private static void AddEvent(EventThreadManagerConfig eventConfig, float value)
        {
            var dateTime = DateTime.Now;
            var dateTimeString = dateTime.ToString(DtFormat);
            // Добавляем event в базу.
            //var rawVal = value.Replace(',', '.');

            var event_ = context.Events.Create();

            event_.TagId = eventConfig.TagId;
            event_.Value = value;
            event_.Time = dateTime;

            context.Events.Add(event_);
            // сохраняем
            context.SaveChanges();

            var z = (from ti in context.Events
                     where ti.TagId == eventConfig.TagId //&& ti.STime == dateTime
                     orderby ti.Time descending
                     select ti.Id);

            var id = z.ToList();// Создадим тревогу чтобы не дергать каждый раз базу.
            var result = new EventThreadManagerEvent
            {
                Id = id.First(),
                TagId = eventConfig.TagId,
                Time = dateTime,
                Value = value,
            };

            _events.Add(result);
            logger.Logged("Info", "add event [" + eventConfig.TagId + "][" + value + "] to alarmserver...", "AlarmThreadManager", "OpenAlarm");

        }

        private static void CloseAlarm(AlarmThreadManagerAlarm alarm, double value, int reason)
        {
            try
            {
                var dt2 = DateTime.Now;
                var dts2 = dt2.ToString(DtFormat);

                var rawVal = value.ToString().Replace(',', '.');
                var alarmtab = context.Alarms.FirstOrDefault(u => u.Id == alarm.Id);
                if (alarmtab != null)
                {
                    alarmtab.ERes = reason;
                    alarmtab.ETime = dt2;
                    alarmtab.EVal = (float)value;
                    context.SaveChanges();
                    alarm.ERes = reason;
                    alarm.ETime = dt2;
                    alarm.EVal = value;
                }
                logger.Logged("Info", "close alarm [" + alarm.TagId + "][" + reason + "] to alarmserver...", "AlarmThreadManager", "CloseAlarm");
            }
            catch (Exception ex)
            {
                logger.Logged("Error", "close alarm [" + alarm.TagId + "][" + reason + "] failed...", "AlarmThreadManager", "CloseAlarm");

            }

        }
        // Загружает конфигурацию объектов и сигналов при старте системы.
        public static void LoadObjSign()
        {
            lock (ConfigLocker)
            {
                _objects = (from ti in context.Objects
                            where (ti.Type == 1 || ti.Type == 2 || ti.Type == 5 || ti.Type == 21)
                             && !ti.Name.Contains("Setting") && !ti.Name.Contains("cfg") && !ti.Name.Contains("Rez")
                            select new Obj { Id = ti.Id, Name = ti.Name, Parent = ti.ParentId, Type = ti.Type, Prop = null }).ToList();

                _tags = (from ti in context.Objects
                         join to in context.Properties on ti.Id equals to.ObjectId
                         where ti.Type == 2 && to.PropId == 0
                         select new Obj { Id = ti.Id, Name = ti.Name, Parent = ti.ParentId, Type = ti.Type, Prop = to.Value }).ToList();
            }

        }

        // Загружает конфигурацию тревог при старте системы.
        public static void LoadAlarmsConfigurations()
        {
            lock (ConfigLocker)
            {
                _cfgs.Clear();
                _cfgsEv.Clear();
                _soundalarms.Clear();
                var taglist = _tags.Where(x => x.Type == 2 && !String.IsNullOrEmpty(x.Prop));

                foreach (var tagjson in taglist)
                {
                    dynamic alarm = JsonConvert.DeserializeObject(tagjson.Prop);
                    var tag = new TagId
                    {
                        TagName = ("Sfera." + Convert.ToString(alarm.Connection) + "." + tagjson.Name.Replace("_unreal", "")).Replace('/', '.'),
                        PollerId = Convert.ToInt32(alarm.Opc)
                    };
                    try
                    {
                        var events = Convert.ToString(alarm.Events);
                        if (events != null && events != "")
                        {
                            var event_ = new EventThreadManagerConfig();
                            var _props = _json.Deserialize(events, event_.GetType());
                            event_ = (EventThreadManagerConfig)_props;
                            if (event_.Enabled)
                            {
                                event_.Tag = tag;
                                event_.TagId = Convert.ToInt32(tagjson.Id);
                                _cfgsEv.Add(event_);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Logged("Err", "add Event tag [" + tag.PollerId + "][" + tag.TagName + "] to alarmserver failed: " + ex.Message,
                            "AlarmThreadManager", "LoadAlarmsLastStates");
                    }
                    try
                    {
                        var tagId = Convert.ToInt32(tagjson.Id);
                        var alarms = Convert.ToString(alarm.Alarms);
                        if (alarms != null && alarms != "")
                        {
                            var alarm_ = new AlarmThreadManagerConfig();
                            var _props = _json.Deserialize(alarms, alarm_.GetType());
                            alarm_ = (AlarmThreadManagerConfig)_props;
                            if (alarm_.Enabled)
                            {
                                alarm_.TagId = Convert.ToInt32(tagjson.Id);
                                alarm_.Tag = tag;

                                if (alarm_.Sound)
                                    _soundalarms.Add(tagId);

                                if (alarm_.HihiSeverity == null) alarm_.HihiSeverity = Double.MaxValue;
                                if (alarm_.HiSeverity == null) alarm_.HiSeverity = Double.MaxValue;
                                if (alarm_.LoSeverity == null) alarm_.LoSeverity = Double.MinValue;
                                if (alarm_.LoloSeverity == null) alarm_.LoloSeverity = Double.MinValue;
                                _cfgs.Add(alarm_);
                            }

                            logger.Logged("Info", "Конфигурация тревоги [" + tag.PollerId + "][" + tag.TagName + "] добавлена...", "AlarmThreadManager", "LoadAlarmsLastStates");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Logged("Err", "Неверная конфигурация тревоги [" + tag.PollerId + "][" + tag.TagName + "] : " +
                            ex.Message, "AlarmThreadManager", "LoadAlarmsLastStates");
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
                AlarmThreadManagerConfig alarm_ = null;

                var tags = (from ti in context.Objects
                            join to in context.Properties on ti.Id equals to.ObjectId
                            where ti.Type == 2 && to.PropId == 0 && ti.Id == tagid
                            select new { Id = ti.Id, Prop = to.Value });

                var taglist = tags.ToList();
                foreach (var tagjson in taglist)
                {

                    dynamic alarm = _json.Deserialize(tagjson.Prop);
                    var tag = new TagId
                    {
                        TagName = Convert.ToString(alarm.Connection),
                        PollerId = Convert.ToInt32(alarm.Opc)
                    };
                    var alarms = Convert.ToString(alarm.Alarms);
                    if (alarms != null)
                    {
                        alarm_ = new AlarmThreadManagerConfig();
                        var _props = _json.Deserialize(alarms, alarm_.GetType());
                        alarm_ = (AlarmThreadManagerConfig)_props;
                        if (alarm_.Enabled)
                        {
                            alarm_.TagId = Convert.ToInt32(tagjson.Id);
                            alarm_.Tag = tag;

                            if (alarm_.Sound) _soundalarms.Add(tagjson.Id);

                            if (alarm_.HihiSeverity == null) alarm_.HihiSeverity = Double.MaxValue;
                            if (alarm_.HiSeverity == null) alarm_.HiSeverity = Double.MaxValue;
                            if (alarm_.LoSeverity == null) alarm_.LoSeverity = Double.MinValue;
                            if (alarm_.LoloSeverity == null) alarm_.LoloSeverity = Double.MinValue;
                            _cfgs.Add(alarm_);
                        }

                        var tagId = Convert.ToInt32(alarm.Id);


                        if (alarm_.HihiSeverity == null) alarm_.HihiSeverity = Double.MaxValue;
                        if (alarm_.HiSeverity == null) alarm_.HiSeverity = Double.MaxValue;
                        if (alarm_.LoSeverity == null) alarm_.LoSeverity = Double.MinValue;
                        if (alarm_.LoloSeverity == null) alarm_.LoloSeverity = Double.MinValue;
                    }


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
                        _cfgs[index] = alarm_;
                    else
                        _cfgs.Add(alarm_);
                }
            }
        }
        /// <summary>
        /// Загружает состояния тревог при запуске системы.
        /// </summary>         
        public static void LoadAlarmsLastStates()
        {
            logger.Logged("Info", "Begin load last events...", "AlarmThreadManager", "LoadAlarmsLastStates");
            var dayBefore = DateTime.Now.AddDays(-1);
            foreach (EventThreadManagerConfig t in _cfgsEv)
            {
                var _event =
                    context.Events.Where(x => (x.TagId == t.TagId && x.Time > dayBefore))
                        .OrderByDescending(x => x.Time)
                        .FirstOrDefault();
                if (_event != null)
                {

                    int id = Convert.ToInt32(_event.Id);
                    int tag = Convert.ToInt32(_event.TagId);
                    DateTime time = Convert.ToDateTime(_event.Time);
                    var value = _event.Value;
                    if (t.EventMessages.Where(x => x.Value == value).FirstOrDefault() != null)
                        t.LastValue = value;
                    _events.Add(new EventThreadManagerEvent
                    {
                        Id = id,
                        TagId = tag,
                        Value = value,
                        Time = time,

                    });
                    logger.Logged("Info", "Load last event [tag:" + tag + ";sval:" + value + ";  STime:" + time + " ]...", "AlarmThreadManager", "LoadAlarmsLastStates");

                }
            }

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

        public List<AlarmThreadManagerOut> GetCurrentAlarms(Int32? id = null)
        {
            var _alarms_ = new List<AlarmThreadManagerAlarm>();
            if (id != null)
            {
                var list = GetChildTags(id);

                _alarms_ = _alarms.Where(x => list.Contains(x.TagId)).ToList();
            }
            else
                _alarms_ = _alarms;

            logger.Logged("Info", id + ". Get alarm list . length" + _alarms_.Count, "AlarmThreadManager", "GetCurrentAlarms");
            var result = new List<AlarmThreadManagerOut>();
            for (var ai = 0; ai < _alarms_.Count; ai++)
            {
                var tm = _cfgs.FirstOrDefault(x => x.TagId == _alarms_[ai].TagId) ?? new AlarmThreadManagerConfig();
                var outAlarm = new AlarmThreadManagerOut();
                outAlarm.Id = _alarms_[ai].Id;
                outAlarm.TagId = _alarms_[ai].TagId;
                _alarms_[ai].SVal = Math.Round(_alarms_[ai].SVal, 2);
                _alarms_[ai].EVal = Math.Round(_alarms_[ai].EVal, 2);
                var sr = "";
                switch (_alarms_[ai].SRes)
                {
                    case -2: sr = tm.LoloText; break;
                    case -1: sr = tm.LoText; break;
                    case 0: sr = tm.NormalText; break;
                    case 1: sr = tm.HiText; break;
                    case 2: sr = tm.HihiText; break;
                }
                outAlarm.StartReason = sr + " (" + Math.Round(_alarms_[ai].SVal, 2) + ")";
                //outAlarm.StartReason = _alarms[ai].SRes;
                outAlarm.StartValue = _alarms_[ai].SVal;
                outAlarm.StartTime = _alarms_[ai].STime;
                //outAlarm.StartValue = _alarms[ai].SVal;
                sr = "";
                switch (_alarms_[ai].ERes)
                {
                    case -2: sr = tm.LoloText; break;
                    case -1: sr = tm.LoText; break;
                    case 0: sr = tm.NormalText; break;
                    case 1: sr = tm.HiText; break;
                    case 2: sr = tm.HihiText; break;
                    case -10:
                        sr = "Тревога активна";
                        outAlarm.Active = true; break;
                }
                outAlarm.EndReason = sr + " (" + Math.Round(_alarms_[ai].EVal, 2) + ")";
                //outAlarm.EndReason = _alarms[ai].ERes;
                outAlarm.EndValue = _alarms_[ai].EVal;
                outAlarm.EndTime = _alarms_[ai].ETime;
                //outAlarm.EndValue = _alarms[ai].EVal;

                outAlarm.AckTime = _alarms_[ai].QTime;
                outAlarm.Ack = _alarms_[ai].Qted;
                var Duration = new TimeSpan();
                if (_alarms_[ai].ETime == DateTime.MinValue)
                    Duration = DateTime.Now - _alarms_[ai].STime;
                else Duration = _alarms_[ai].ETime - _alarms_[ai].STime;

                outAlarm.Duration = Duration.Hours + ":" + Duration.Minutes + ":" + Duration.Seconds;
                if (Duration.Days > 0)
                    outAlarm.Duration = Duration.Days + "д. " + outAlarm.Duration;
                result.Add(outAlarm);
            }
            return result.OrderBy(x => x.StartTime).ToList();
        }

        public List<EventThreadManagerOut> GetCurrentEvents(Int32? id = null)
        {
            var _events_ = new List<EventThreadManagerEvent>();
            if (id != null)
            {
                var list = GetChildTags(id);

                _events_ = _events.Where(x => list.Contains(x.TagId)).ToList();
            }
            else
                _events_ = _events;
            logger.Logged("Info", id + ". Get alarm list . length" + _events_.Count, "AlarmThreadManager", "GetCurrentEvents");
            var result = new List<EventThreadManagerOut>();
            for (var ai = 0; ai < _events_.Count; ai++)
            {
                var tm = _cfgsEv.FirstOrDefault(x => x.TagId == _events_[ai].TagId) ?? new EventThreadManagerConfig();
                var outEvent = new EventThreadManagerOut();
                outEvent.Id = _events_[ai].Id;
                outEvent.TagId = _events_[ai].TagId;
                outEvent.Time = _events_[ai].Time;
                foreach (var a in tm.EventMessages)
                {
                    if (a.Value == _events_[ai].Value)
                        outEvent.Message = a.Message;
                }
                result.Add(outEvent);
            }
            //result=_events.OrderBy(x => x.Time).ToList();
            return result.OrderBy(x => x.Time).ToList();
        }
        public List<AlarmThreadManagerOut> GetAlarmsReport(string dt1, string dt2)
        {
            var Sdate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            var Edate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 24, 0, 0);
            if (dt1 != null)
                Sdate = Convert.ToDateTime(dt1);
            if (dt2 != null)
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
                    case 0: sr = tm.NormalText; break;
                    case 1: sr = tm.HiText; break;
                    case 2: sr = tm.HihiText; break;
                }
                //outAlarm.StartReason = _alarms[ai].SRes; 
                outAlarm.StartReason = sr + " (" + Math.Round(_alarms[ai].SVal, 2) + ")";

                outAlarm.StartValue = _alarms[ai].SVal;
                outAlarm.StartTime = _alarms[ai].STime;
                //outAlarm.StartValue = _alarms[ai].SVal;
                sr = "";
                switch (_alarms[ai].ERes)
                {
                    case -2: sr = tm.LoloText; break;
                    case -1: sr = tm.LoText; break;
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
                    _cfgs[index].Permit = state;
            }
        }


        public bool SetAlarmAck(int alarmId)
        {
            try
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
                            return true;
                        }
                        _alarms.RemoveAt(index);
                    }
                    return true;

                }
            }
            catch (Exception ex)
            {
                logger.Logged("ERR", "Ack alarm with id=" + alarmId + " failed..." + ex.Message, "AlarmThreadManager", "SetAlarmAck");
                return false;
            }
        }

        private class Indian001
        {
            public int Id;
        }

        public bool SetAlarmAckAll()
        {
            try
            {
                var ind001 = _alarms.Where(x => x.Qted == null).Select(alarm => new Indian001 { Id = alarm.Id }).ToList();
                foreach (var item in ind001)
                    SetAlarmAck(item.Id);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
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

        public List<AlarmThreadManagerConfig> GetAlarmConfig()
        {
            return _cfgs;
        }
        public List<EventThreadManagerConfig> GetEventConfig()
        {
            return _cfgsEv;
        }
    }

}
