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
        public bool Enabled;
        public bool Active;

        public string HihiText;
        public string HiText;
        public string NormalText;
        public string LoText;
        public string LoloText;

        public double HihiSeverity;
        public double HiSeverity;
        public double LoSeverity;
        public double LoloSeverity;
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
        public TimeSpan Duration;
        public int? Ack;
        public bool Active;
        public string Message;
    }
     
    public interface IAlarmServer
    {
        bool Init();
        void Run();
        void Restart();
        List<AlarmThreadManagerConfig>  GetAlarmStates();
        List<AlarmThreadManagerOut> GetCurrentAlarms();
        List<AlarmThreadManagerOut> GetAlarmsReport(string dt1,string dt2);
        void SetAlarmAck(int alarmId);
        void SetAlarmAckAll();
        bool SoundAlarm();
    }
  
}
