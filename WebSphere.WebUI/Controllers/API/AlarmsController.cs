using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Script.Serialization;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.WebUI.Controllers.API
{
    public class AlarmsController : ApiController
    {
         
  /*      [HttpPost]
        public void Permissions(FormDataCollection data)
        {
            var groupID = data.Get("groupID");
            var action = data.Get("action");
            var Checked = data.Get("checked");
            Int32 Perm;

            if (Checked == "true") Perm = 1;
            else Perm = 0;

            MyDB.sql_query_local("UPDATE Permissions SET permission = " + Perm + " WHERE group_id = " + groupID + " AND action = '" + action + "'");
        }
*/
   /*     [HttpPost]
        public bool UpdateConfig(FormDataCollection data)
        {
            if (data == null)
            {
                Logger.Log("[AlarmsController.UpdateConfig] [ERR] Data is null", Logger.Levels.Error);
                return false;
            }
            var tag = data.Get("tag");
            var propName = data.Get("configName");
            var propValue = data.Get("propValue");
            MyDB.sql_query_local("UPDATE Signal_alarm_messages SET " + propName + " = '" + propValue + "'  WHERE input_source = '" + tag + "';");
            _alarmServer.LoadOneAlarm(tag);
            return true;
        }
        */
  /*      [HttpPost]
        public bool DeleteConfig(FormDataCollection data)
        {
            var tag = data.Get("tag");
            MyDB.sql_query_local("delete from Signal_alarm_messages   WHERE input_source = '" + tag + "';");
            AlarmThreadManager.DeleteAlarm(tag);
            return true;
        }
*/
  /*      [HttpPost]
        public bool ChangeConfig(FormDataCollection data)
        {
            Int32 check_int;
            var tag = data.Get("tag");
            bool check = Convert.ToBoolean(data.Get("check"));
            if (check) check_int = 1;
            else check_int = 0;
            MyDB.sql_query_local("UPDATE Signal_Alarm_Messages SET enabled = " + check_int + " WHERE input_source = '" + tag + "';");
            AlarmThreadManager.ChangeAlarmState(tag, check);
            return true;
        }

        */
        [HttpPost]
        public List<AlarmThreadManagerOut> GetCurrentAlarms(FormDataCollection data)
        {
            return MvcApplication.AlarmServer.GetCurrentAlarms();
        }
        [AcceptVerbs("Get", "Post")]
        public List<AlarmThreadManagerOut> Get_journal(FormDataCollection data)
        {
            DateTime thisDay = DateTime.Today;
            string dt1;
            string dt2;

            if (data.Get("s_d") == "")
            {
                dt1 = thisDay.AddDays(-2).ToString("d");
            }
            else
            {
                dt1 = Convert.ToString(data.Get("s_d"));
            }
            if (data.Get("e_d") == "")
            {
                dt2 = thisDay.AddDays(+1).ToString("d");
            }
            else
            {
                dt2 = Convert.ToString(data.Get("e_d"));
            }

            return MvcApplication.AlarmServer.GetAlarmsReport(dt1, dt2);

        }

        [HttpPost]
        public void KvitAlarm(FormDataCollection data)
        {
            if (data == null)
                return;
            var id = Convert.ToInt32(data.Get("Id"));
            MvcApplication.AlarmServer.SetAlarmAck(id);
        }
        public bool SoundAlarm(FormDataCollection data)
        {  
            return MvcApplication.AlarmServer.SoundAlarm();
        }

        [HttpPost]
        public void KvitAll(FormDataCollection data)
        {
            MvcApplication.AlarmServer.SetAlarmAckAll();
        }
        [HttpPost]
        public void RestartAlarms(FormDataCollection data)
        {
            MvcApplication.AlarmServer.Restart();
        }
        [HttpPost]
        public List<AlarmThreadManagerConfig> GetAlarmsCfgStates(FormDataCollection data)
        {
            //var result = TagThreadsManager.GetThreadStates();
            var result = MvcApplication.AlarmServer.GetAlarmStates();
            return result;
        }
   /*     [HttpPost]
        public bool AddConfig(FormDataCollection data)
        {
            var tag = data.Get("tag");

            Int32 max_id_int;
            var qres = MyDB.sql_query_local("select max(al_id) as max_id from Signal_Alarm_Messages");
            var max_id_str = qres.GetValue(0, "max_id");
            if (max_id_str == "") max_id_int = 1;
            else max_id_int = Convert.ToInt32(max_id_str) + 1;

            MyDB.sql_query_local("insert into  Signal_alarm_messages (al_id, input_source, enabled, requires_ack) values(" + max_id_int + ", '" + tag + "',1,0);");
            AlarmThreadManager.LoadOneAlarm(tag);
            return true;
        }
        */

 /*
        [HttpPost]
        public ObjectAlarms GetObjectAlarms(FormDataCollection data)
        {
            var result = new ObjectAlarms();
            if (!MyUser.CheckPermission(PermissionRight.ConfAlarm))
                return result;
     //      
     //                 string strObjectId = data.Get("objectId");
     //                 // Получаем свойства объекта.
     //                 var qres = MyDB.sql_query_local("SELECT * FROM Objects WHERE id = " + strObjectId + ";");
     //                 if (qres.count_rows == 0)
     //                     return result;
     //                 result.Id = qres.GetValue(0, "id");
     //                 result.Name = qres.GetValue(0, "name");
     //                  
            var list_alarms = new List<ConfigAlarm>();
            string q = "SELECT * FROM  c_tags  ";
            q += " LEFT JOIN Signal_Alarm_Messages AS sam ON (tag = input_source) ";
            var qres = MyDB.sql_query_local(q);
            for (var j = 0; j < qres.count_rows; j++)
            {
                var objectalarm = new ConfigAlarm();
                objectalarm.lolo_text = qres.GetValue(j, "lolo_text");
                objectalarm.lo_text = qres.GetValue(j, "lo_text");
                objectalarm.hi_text = qres.GetValue(j, "hi_text");
                objectalarm.hihi_text = qres.GetValue(j, "hihi_text");
                objectalarm.normal_text = qres.GetValue(j, "normal_text");
                objectalarm.lolo_severity = qres.GetValue(j, "lolo_severity");
                objectalarm.lo_severity = qres.GetValue(j, "lo_severity");
                objectalarm.hi_severity = qres.GetValue(j, "hi_severity");
                objectalarm.hihi_severity = qres.GetValue(j, "hihi_severity");
                objectalarm.tag = qres.GetValue(j, "tag");
                objectalarm.descript = qres.GetValue(j, "descript");
                objectalarm.AlarmID = qres.GetValue(j, "al_id");
                var enabled = qres.GetValue(j, "enabled");
                if (enabled == "") objectalarm.enabled = false;
                else
                    objectalarm.enabled = Convert.ToBoolean(enabled);

                var requires_ack = qres.GetValue(j, "requires_ack");
                if (requires_ack == "") objectalarm.requires_ack = false;
                else
                    objectalarm.requires_ack = Convert.ToBoolean(requires_ack);

                result.configalarms.Add(objectalarm);
            }
            return result;
        }
*/

     /*  
        [HttpPost]
        public ObjectAlarms GetObjectAlarms1(FormDataCollection data)
        { 

            Models.ObjectAlarms result = new Models.ObjectAlarms();
            string objectId = data.Get("objectId");
            // Получаем свойства объекта.
            var qres = MyDB.sql_query_local("SELECT * FROM Objects WHERE id = " + objectId + ";");
            if (qres.count_rows == 0)
                return result;

            result.Id = qres.GetValue(0, "id");
            result.Name = qres.GetValue(0, "name");

            return result;
        }
        */


    

    }
}
