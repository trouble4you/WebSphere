using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Abstract
{
    public class AlarmThreadManagerConfig
    {
        public TagId Tag;
        public int TagId;
        public bool Permit;
        public bool Enabled;
        public bool Sound;

        public string HihiText;
        public string HiText;
        public string NormalText;
        public string LoText;
        public string LoloText;

        public double? HihiSeverity;
        public double? HiSeverity;
        public double? LoSeverity;
        public double? LoloSeverity;
    }
    public class EventThreadManagerConfig
    {
        public TagId Tag;
        public int TagId;
        public bool Enabled;
        public bool Active;
        public double LastValue;

        public List<EventValMessage> EventMessages;
    }

    public class EventValMessage
    {
        public float Value;
        public string Message;

    }

    public class AlarmThreadManagerAlarm
    {
        public int Id;
        public int TagId;
        public int SRes;
        public DateTime STime;
        public double SVal;
        public int ERes;
        public DateTime ETime;
        public double EVal;
        public DateTime QTime;
        public int? Qted;
    }
    public class EventThreadManagerEvent
    {
        public int Id;
        public int TagId;
        public DateTime Time;
        public float Value;
    }

    public class EventAlertManager
    {
        public string Message;
        public int Id;
    }
    public class EventThreadManagerOut
    {
        public int Id;
        public int TagId;
        public DateTime Time;
        public string Message;
    }

    public class AlarmThreadManagerOut
    {
        public int Id;
        public int TagId;
        public string StartReason;
        public DateTime StartTime;
        public double StartValue;
        public string EndReason;
        public DateTime EndTime;
        public double EndValue;
        public DateTime AckTime;
        public string Duration;
        public int? Ack;
        public bool Active;
        public string Message;
    }

    public interface IAlarmServer
    {
        bool Init();
        void Run();
        bool Restart();
        List<AlarmThreadManagerConfig> GetAlarmConfig();
        List<AlarmThreadManagerOut> GetCurrentAlarms(Int32? id);

        List<EventThreadManagerConfig> GetEventConfig();
        List<EventThreadManagerOut> GetCurrentEvents(Int32? id);
        List<AlarmThreadManagerOut> GetAlarmsReport(string dt1, string dt2);
        bool SetAlarmAck(int alarmId);
        bool SetAlarmAckAll();
        List<EventAlertManager> SoundAlarm();
    }

}
