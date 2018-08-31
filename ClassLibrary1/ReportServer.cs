using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using System.Text;
using NPOI.SS.Util;

namespace WebSphere.Reports
{
    public class Well
    {
        public int AgzuId;
        public int WellId;
        public string AgzuName;
        public string WellName;
        public int Psm;
    }

    public class AGZU
    {
        public string Name;
        public List<Well> Wells;
        public int st_id;
        public int end_id;
        public int Psm;
        public int MinSec;
        public int DayHour;
        public int YearMont;
        public int Tism;
        public int QMfld;
        public int QVfld;
        public int Goil;
        public int Qng;
        public int water;
        public int pofld;
        public int Mfld;
        public int Vfld;
        public int Moil;
        public int Vng;
    }



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

    public static class MyDB
    {
        private static readonly string ConnectionStringLocal = System.Configuration.ConfigurationSettings.AppSettings["MyConnection"];
        public static MySQLResult sql_query_local(string query)
        {
            {
                var result = new MySQLResult { count_rows = 0 };
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



    public class ReportServer : IReportServer
    {
        private static HSSFWorkbook _workbook;
        private readonly EFDbContext _context = new EFDbContext();
        private static readonly Logging logger = new Logging();
        // private static readonly JSON Json = new JSON();
        private static IJSON _json = new JSON();
        private static List<AlarmThreadManagerConfig> _cfgs = new List<AlarmThreadManagerConfig>();
        private static List<EventThreadManagerConfig> _cfgsEv = new List<EventThreadManagerConfig>();

        private static readonly List<Report> reportList = new List<Report>
            {
                 new Report{Id = 1,Name = "Журнал замеров АГЗУ"} ,
                 new Report{Id = 2,Name = "Хронология пусков и остановок скважин"} ,
                 new Report{Id = 3,Name = "Текущее состояние скважин"} ,
                 new Report{Id = 4,Name = "Журнал состояния связи"} ,
                 //new Report{Id = 5,Name = "Площадка конденсатосборников"},
                 new Report{Id = 6,Name = "Журнал тревог"},
                 new Report{Id = 7,Name = "Журнал событий "}
            };


        private List<Obj> _objects;
        private List<Obj> _tags;

        public class Obj
        {
            public int Id;
            public int Type;
            public int? Parent;
            public string Name;
            public string Prop;


        }
        public void LoadObjSign()
        {
            _objects = (from ti in _context.Objects
                        where (ti.Type == 1 || ti.Type == 2 || ti.Type == 5 || ti.Type == 21)
                         && !ti.Name.Contains("Setting") && !ti.Name.Contains("cfg") && !ti.Name.Contains("Rez")
                        select new Obj { Id = ti.Id, Name = ti.Name, Parent = ti.ParentId, Type = ti.Type, Prop = null }).ToList();

            _tags = (from ti in _context.Objects
                     join to in _context.Properties on ti.Id equals to.ObjectId
                     where ti.Type == 2 && to.PropId == 0
                     select new Obj { Id = ti.Id, Name = ti.Name, Parent = ti.ParentId, Type = ti.Type, Prop = to.Value }).ToList();


        }
        public List<int> GetChildTags(int? parID)
        {
            var rez = new List<int>();
            if (parID == null) return rez;
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
        private static readonly List<Well> WellList = new List<Well> {
           new Well{AgzuName="АГЗУ 4003",WellName="Скважина 124",Psm=4},
           new Well{AgzuName="АГЗУ 4003",WellName="Скважина 123",Psm=5},
           new Well{AgzuName="АГЗУ 4003",WellName="Скважина 122",Psm=7},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 160",Psm=1},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 165",Psm=2},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 166",Psm=3},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 170",Psm=4},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 150",Psm=5},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 164",Psm=6},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 202",Psm=7},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 168",Psm=8},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 159",Psm=9},
            new Well{AgzuName="АГЗУ 160",WellName="Скважина 151",Psm=10},
            new Well{AgzuName="АГЗУ 200",WellName="Скважина 330",Psm=5},
            new Well{AgzuName="АГЗУ 200",WellName="Скважина 320",Psm=6},
            new Well{AgzuName="АГЗУ 200",WellName="Скважина 310",Psm=7},
            new Well{AgzuName="АГЗУ 200",WellName="Скважина 251",Psm=8},
            new Well{AgzuName="АГЗУ 200",WellName="Скважина 270",Psm=9},
            new Well{AgzuName="АГЗУ 200",WellName="Скважина 200",Psm=10},
            new Well{AgzuName="АГЗУ 201",WellName="Скважина 321",Psm=1},
            new Well{AgzuName="АГЗУ 201",WellName="Скважина 272",Psm=2},
            new Well{AgzuName="АГЗУ 201",WellName="Скважина 271",Psm=3},
            new Well{AgzuName="АГЗУ 201",WellName="Скважина 200",Psm=4},
            new Well{AgzuName="АГЗУ 201",WellName="Скважина 201",Psm=5},
            new Well{AgzuName="АГЗУ 201",WellName="Скважина 318",Psm=8},
            new Well{AgzuName="АГЗУ 201",WellName="Скважина 295",Psm=9},
            new Well{AgzuName="АГЗУ 201",WellName="Скважина 263",Psm=10}
        };



        /*
        public Report Rep1(string name, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            var rez = new List<List<string>>();
            try
            {
                using (context)
                {
                    var parameters1 = new object[parameters.Count];
                    //{  
                    //    new SqlParameter("@Id", "1") ,
                    //    new SqlParameter("@Name", "NULL") 
                    //};
                    var par_string = "";
                    var i = 0;
                    foreach (var parameter in parameters)
                    {
                        if (par_string != "")
                        {
                            par_string = par_string + ',';
                            i = i + 1;
                        }
                        parameters1[i] = new SqlParameter("@" + parameter.Key, "" + parameter.Value + "");

                        par_string = par_string + "@" + parameter.Key;

                    }

                    var sqlQuery = context.Database.SqlQuery<Report2>(name + " " + par_string, parameters1);
                    var sqlQueryList = sqlQuery.ToList();
                    for (i = 0; i < sqlQueryList.Count; i++)
                    {
                        var id = Convert.ToString(sqlQueryList[i].Id);
                        var sd = Convert.ToString(sqlQueryList[i].SignalId);
                        var dt = Convert.ToString(sqlQueryList[i].Datetime);
                        rez.Add(new List<string> {id, sd, dt});

                    }
                    //rez.AddRange(sqlQueryList.Select(rezult => new List<string> { Convert.ToString(rezult.Id), Convert.ToString(rezult.SignalId), Convert.ToString(rezult.Datetime) }));

                }

                logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep1");
                report.Name = name;
                report.Head = new List<string> {"ID", "signal", "dt"};
                report.Rows = rez;
            }
            catch (Exception ex)
            {
                return report;
            }
            return report;
        }
         */

        public string ProcFl(string value, int k)
        {
            double N;
            if (!Double.TryParse(value.Replace(".", ","), out N)) return value;
            else return string.IsNullOrEmpty(value) ? "" : Convert.ToString(Math.Round(N, k));
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

        public Report PkReport(string name, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            var rez = new List<List<string>>();
            var date = DateTime.Now;
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    date = Convert.ToDateTime(Convert.ToString(parameter.Value));
            }
            try
            {
                var d = new DateTime(date.Year, date.Month, date.Day, 2, 0, 0);
                for (var n = d; n <= d.AddDays(1); n = n.AddHours(2))
                {
                    var row = new List<string>();
                    var n2 = n.AddHours(2);

                    var q1 = "select top 1 Value from SignalsAnalogs where TagId=562 and Datetime between '" +
                                n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                    var q2 = "select top 1 Value from SignalsAnalogs where TagId=563  and Datetime between '" +
                                n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                    var q3 = "select top 1 Value from SignalsAnalogs where TagId=564  and Datetime between '" +
                                n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                    var q4 = "select top 1 Value from SignalsAnalogs where TagId=565  and Datetime between '" +
                                n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                    var ss1 = MyDB.sql_query_local(q1);
                    var ss2 = MyDB.sql_query_local(q2);
                    var ss3 = MyDB.sql_query_local(q3);
                    var ss4 = MyDB.sql_query_local(q4);

                    row.Add(n.TimeOfDay.ToString());
                    row.Add((ss1.count_rows > 0) ? ProcFl(ss1.GetValue(0, 0), 2) : "");
                    row.Add((ss2.count_rows > 0) ? ProcFl(ss2.GetValue(0, 0), 2) : "");
                    row.Add((ss3.count_rows > 0) ? ProcFl(ss3.GetValue(0, 0), 2) : "");
                    row.Add((ss4.count_rows > 0) ? ProcFl(ss4.GetValue(0, 0), 2) : "");
                    rez.Add(row);
                }

                logger.Logged("Info", "Report loaded..." + name, "ReportServer", "Rep2");
                report.Name = name;
                report.Head = new List<string> { "Время чч:мм", "Давление Ек1 Мпа", "Давление Ек2 Мпа", "Уровень Ек1 мм", "Уровень Ек2 мм" };
                report.Rows = rez;
            }
            catch (Exception ex)
            {
                return report;
            }
            return report;
        }

        public List<AGZU> AGZUList()
        {
            List<AGZU> AGZUList = new List<AGZU> {
    new AGZU{Name="АГЗУ 160" ,st_id= 41540,end_id=41554,Psm=41540,MinSec=41541,DayHour=41542,YearMont=41543,Tism=41544,QMfld=41545,QVfld=41546,Goil=41547,water=41549,pofld=41550,Mfld=41551,Vfld=41552,Moil=41553},
    new AGZU{Name="АГЗУ 200" ,st_id= 31421,end_id=31435,Psm=31421,MinSec=31422,DayHour=31423,YearMont=31424,Tism=31425,QMfld=31426,QVfld=31427,Goil=31428,water=31430,pofld=31431,Mfld=31432,Vfld=31433,Moil=31434},
    new AGZU{Name="АГЗУ 201" ,st_id= 35194,end_id=35208,Psm=35194,MinSec=35195,DayHour=35196,YearMont=35197,Tism=35198,QMfld=35199,QVfld=35200,Goil=35201,water=35203,pofld=35204,Mfld=35205,Vfld=35206,Moil=35207},
    new AGZU{Name="АГЗУ 4003",st_id= 39429,end_id=39469,Psm=39429,MinSec=39429,DayHour=39429,YearMont=39429,Tism=39469,QMfld=39469,QVfld=39459,Goil=39461,water=39458,pofld=39467,Mfld=39463,Vfld=39462,Moil=39464}

     };
            return AGZUList;
        }

        public int GetPSM(string WellName)
        {
            if (WellName == "Скважина 124") return 4;
            if (WellName == "Скважина 123") return 5;
            if (WellName == "Скважина 122") return 7;
            if (WellName == "Скважина 160") return 1;
            if (WellName == "Скважина 165") return 2;
            if (WellName == "Скважина 166") return 3;
            if (WellName == "Скважина 170") return 4;
            if (WellName == "Скважина 150") return 5;
            if (WellName == "Скважина 164") return 6;
            if (WellName == "Скважина 202") return 7;
            if (WellName == "Скважина 168") return 8;
            if (WellName == "Скважина 159") return 9;
            if (WellName == "Скважина 151") return 10;
            if (WellName == "Скважина 330") return 5;
            if (WellName == "Скважина 320") return 6;
            if (WellName == "Скважина 310") return 7;
            if (WellName == "Скважина 251") return 8;
            if (WellName == "Скважина 270") return 9;
            if (WellName == "Скважина 200") return 10;
            if (WellName == "Скважина 321") return 1;
            if (WellName == "Скважина 272") return 2;
            if (WellName == "Скважина 271") return 3;
            if (WellName == "Скважина 200") return 4;
            if (WellName == "Скважина 201") return 5;
            if (WellName == "Скважина 318") return 8;
            if (WellName == "Скважина 295") return 9;
            if (WellName == "Скважина 263") return 10;
            else return 0;
        }
        public List<Well> GetAgzuOrWell(int? GzuId = null, string GzuName = null, int? WellId = null, string WellName = null)
        {
            var wellList = new List<Well>();
            using (_context)
            {
                string q = "";
                q = q + " declare @agzu table(id int, name varchar(max))                                                                                         ";
                q = q + "  insert into @agzu                                                                                                                     ";
                q = q + " SELECT[Id],                                                                                                                            ";
                q = q + " case when Name like 'Южно-золоторевское%' then Replace([Name], 'Южно-золоторевское', 'АГЗУ')                                           ";
                q = q + "  when Name = 'Вербовское ' then  'АГЗУ 4003'                                                                                           ";
                q = q + "  when Name = 'Крепостное ' then  'АГЗУ 160' end as Name                                                                                ";
                q = q + " FROM [Objects] where type = 5                                                                                            ";
                q = q + "                                                                                                                                        ";
                q = q + "                                                                                                                                        ";
                q = q + " SELECT gzu.id as gzuid, gzu.name as gzuname,wells.[Id] as wellid ,Replace(wells.[Name], 'Well', 'Скважина ') as wellName               ";
                q = q + " FROM @agzu as gzu                                                                                                                      ";
                q = q + " left join Objects as tags on(tags.ParentId = gzu.Id and tags.Name = 'Теги' and tags.type = 21)                                         ";
                q = q + " left join Objects as port on(port.ParentId = tags.Id and port.Name like 'Port%' and port.type = 21)                                    ";
                q = q + " inner join Objects as wells on(wells.ParentId = port.Id and(wells.Name  like 'Well___' or wells.Name like 'Купер') and wells.type = 21)";
                if (GzuId != null && WellId != null)
                    q = q + " where gzu.id=" + GzuId + " and wells.id=" + WellId;
                else if (GzuId != null)
                    q = q + " where  gzu.id=" + GzuId;
                else if (WellId != null)
                    q = q + " where  wells.id=" + WellId;


                try
                {
                    var rezult = MyDB.sql_query_local(q);
                    foreach (var _row in rezult.rows)
                    {
                        var well = new Well()
                        {
                            AgzuId = Convert.ToInt32(_row.values[0]),
                            AgzuName = _row.values[1],
                            WellId = Convert.ToInt32(_row.values[2]),
                            WellName = _row.values[3],
                            Psm = GetPSM(_row.values[3])
                        };
                        wellList.Add(well);
                    }
                }
                catch (Exception ex)
                {
                    logger.Logged("Error", " Report GetAgzuOrWell not loaded. Reason: " + ex.Message, "ReportServer", "GetAgzuOrWellstr");
                }
            }
            return wellList;
        }
        public string GetAgzuOrWellstr(int? GzuId = null, string GzuName = null, int? WellId = null, string WellName = null)
        {
            if (GzuId == 0) GzuId = null;
            if (WellId == 0) WellId = null;

            string q = "";
            q = q + " declare @agzu table(id int, name varchar(max))                                                                                         ";
            q = q + "  insert into @agzu                                                                                                                     ";
            q = q + " SELECT[Id],                                                                                                                            ";
            q = q + " case when Name like 'Южно-золоторевское%' then Replace([Name], 'Южно-золоторевское', 'АГЗУ')                                           ";
            q = q + "  when Name = 'Вербовское ' then  'АГЗУ 4003'                                                                                           ";
            q = q + "  when Name = 'Крепостное ' then  'АГЗУ 160' end as Name                                                                                ";
            q = q + " FROM [Objects] where type = 5  ";

            q = q + "  declare @well table(gzuid int, gzuname varchar(max), wellid int, wellName varchar(max))                                                                                                                                      ";
            q = q + "  insert into @well                                                                                                                                      ";
            q = q + " SELECT gzu.id as gzuid, gzu.name as gzuname,wells.[Id] as wellid ,Replace(wells.[Name], 'Well', 'Скважина ') as wellName               ";
            q = q + " FROM @agzu as gzu                                                                                                                      ";
            q = q + " left join Objects as tags on(tags.ParentId = gzu.Id and tags.Name = 'Теги' and tags.type = 21)                                         ";
            q = q + " left join Objects as port on(port.ParentId = tags.Id and port.Name like 'Port%' and port.type = 21)                                    ";
            q = q + " inner join Objects as wells on(wells.ParentId = port.Id and(wells.Name  like 'Well___' or wells.Name like 'Купер') and wells.type = 21)";
            if (GzuId != null && WellId != null)
                q = q + " where gzu.id=" + GzuId + " and wells.id=" + WellId;
            else if (GzuId != null)
                q = q + " where  gzu.id=" + GzuId;
            else if (WellId != null)
                q = q + " where  wells.id=" + WellId;

            logger.Logged("Info", " Report GetAgzuOrWellstr loaded.", "ReportServer", "GetAgzuOrWellstr");
            return q;
        }

        public Report MgbbReport2(string name, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            var rez = new List<List<string>>();
            var date = DateTime.Now;
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    date = Convert.ToDateTime(Convert.ToString(parameter.Value));
            }
            try
            {
                using (_context)
                {
                    var d = new DateTime(date.Year, date.Month, date.Day, 2, 0, 0);
                    for (DateTime n = d; n <= d.AddDays(1); n = n.AddHours(2))
                    {
                        DateTime n2 = n.AddHours(2);
                        var row = new List<string>();


                        var q1 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(632, n, n2)); //"AT01. З
                        var q2 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(633, n, n2)); //"AT02. З
                        var q3 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(661, n, n2)); //"TT01H. 
                        var q4 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(636, n, n2)); //"PDIT01.
                        var q5 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(637, n, n2)); //"PDIT101
                        var q6 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(638, n, n2)); //"PIT01. 
                        var q7 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(660, n, n2)); //"TT01. С
                        var q8 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(648, n, n2)); //"PIT11. 
                        var q9 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(667, n, n2)); //"TT07. П
                        var q10 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(639, n, n2)); //"PIT02.С
                        var q11 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(640, n, n2)); //"PIT03.С
                        var q12 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(641, n, n2)); //"PIT04.С
                        var q13 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(668, n, n2)); //"TT08. С
                        var q14 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(642, n, n2)); //"PIT05. 
                        var q15 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(662, n, n2)); //"TT02. С
                        var q16 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(643, n, n2)); //"PIT06. 
                        var q17 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(663, n, n2)); //"TT03. П
                        var q18 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(644, n, n2)); //"PIT07. 
                        var q19 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(664, n, n2)); //"TT04. С
                        var q20 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(649, n, n2)); //"PIT12. 
                        var q21 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(665, n, n2)); //"TT05.  
                        var q22 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(645, n, n2)); //"PIT08. 
                        var q23 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(646, n, n2)); //"PIT09. 
                        var q24 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(647, n, n2)); //"PIT10. 
                        var q25 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(666, n, n2)); //"TT06. Т 

                        row.Add(n.TimeOfDay.ToString());
                        var t = 1;
                        var p = 2;
                        var e = 3;


                        row.Add((q1.count_rows > 0) ? ProcFl(q1.GetValue(0, 0), 2) : "");
                        row.Add((q2.count_rows > 0) ? ProcFl(q2.GetValue(0, 0), 2) : "");
                        row.Add((q3.count_rows > 0) ? ProcFl(q3.GetValue(0, 0), 2) : "");
                        row.Add((q4.count_rows > 0) ? ProcFl(q4.GetValue(0, 0), 2) : "");
                        row.Add((q5.count_rows > 0) ? ProcFl(q5.GetValue(0, 0), 2) : "");
                        row.Add((q6.count_rows > 0) ? ProcFl(q6.GetValue(0, 0), 2) : "");
                        row.Add((q7.count_rows > 0) ? ProcFl(q7.GetValue(0, 0), 2) : "");
                        row.Add((q8.count_rows > 0) ? ProcFl(q8.GetValue(0, 0), 2) : "");
                        row.Add((q9.count_rows > 0) ? ProcFl(q9.GetValue(0, 0), 2) : "");
                        row.Add((q10.count_rows > 0) ? ProcFl(q10.GetValue(0, 0), 2) : "");
                        row.Add((q11.count_rows > 0) ? ProcFl(q11.GetValue(0, 0), 2) : "");
                        row.Add((q12.count_rows > 0) ? ProcFl(q12.GetValue(0, 0), 2) : "");
                        row.Add((q13.count_rows > 0) ? ProcFl(q13.GetValue(0, 0), 2) : "");
                        row.Add((q14.count_rows > 0) ? ProcFl(q14.GetValue(0, 0), 2) : "");
                        row.Add((q15.count_rows > 0) ? ProcFl(q15.GetValue(0, 0), 2) : "");
                        row.Add((q16.count_rows > 0) ? ProcFl(q16.GetValue(0, 0), 2) : "");
                        row.Add((q17.count_rows > 0) ? ProcFl(q17.GetValue(0, 0), 2) : "");
                        row.Add((q18.count_rows > 0) ? ProcFl(q18.GetValue(0, 0), 2) : "");
                        row.Add((q19.count_rows > 0) ? ProcFl(q19.GetValue(0, 0), 2) : "");
                        row.Add((q20.count_rows > 0) ? ProcFl(q20.GetValue(0, 0), 2) : "");
                        row.Add((q21.count_rows > 0) ? ProcFl(q21.GetValue(0, 0), 2) : "");
                        row.Add((q22.count_rows > 0) ? ProcFl(q22.GetValue(0, 0), 2) : "");
                        row.Add((q23.count_rows > 0) ? ProcFl(q23.GetValue(0, 0), 2) : "");
                        row.Add((q24.count_rows > 0) ? ProcFl(q24.GetValue(0, 0), 2) : "");
                        row.Add((q25.count_rows > 0) ? ProcFl(q25.GetValue(0, 0), 2) : "");
                        rez.Add(row);
                    }
                }
                logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                report.Name = name;
                report.Head = new List<string>
                {
                    "Время",
                    "AT01. Загазованность в блок-боксе %",
                    "AT02. Загазованность в блок-боксе %",
                    "TT01H. Температура воздуха в блок-боксе °С",
                    "PDIT01. Перепад давления на БФ-101 МПа",
                    "PDIT101. Перепад давления на МГБ-101 МПа ",
                    "PIT01. Сыр. газ. Давление  на входе в установку МПа",
                    "TT01. Сыр. газ. Температура с на входе в установку °С",
                    "PIT11. Подг. газ.Давление на выходе установки МПа",
                    "TT07. Подг. газ.Температура на выходе установки °С",
                    "PIT02.Сыр. газ. Давление в верхней секции БФ-101 МПа",
                    "PIT03.Сыр. газ. Давление на выходе УРГ №1 МПа",
                    "PIT04.Сыр. газ. Давление на выходе УРГ №1 МПа",
                    "TT08. Сыр. газ.Температура после УРГ №1  ",
                    "PIT05. Сыр. газ. Давление МПа",
                    "TT02. Сыр. газ. Температура °С",
                    "PIT06. Подг. газ. Давление  МПа",
                    "TT03. Подг. газ.Температура  °С",
                    "PIT07. Сыр. газ. Давление на входе МГБ-101 МПа ",
                    "TT04. Сыр. газ. Температура на входе МГБ-101 °С",
                    "PIT12. Подг. газ.Давление на выходе МГБ-101 МПа",
                    "TT05.  Подг. газ.Температура на выходе МГБ-101 °С",
                    "PIT08. Подг. газ.Давление  после УРГ №2 МПа",
                    "PIT09. Подг. газ.Давление после УРГ №2 МПа ",
                    "PIT10. Давление газа в пермеатном коллекторе МПа ",
                    "TT06. Температура газа в пермеатном коллекторе °С"
                };
                report.Rows = rez;
            }
            catch (Exception ex)
            {
                return report;
            }
            return report;
        }

        public Report PgReport(string name, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            var rez = new List<List<string>>();
            var date = DateTime.Now;
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    date = Convert.ToDateTime(Convert.ToString(parameter.Value));
            }
            try
            {
                using (_context)
                {
                    var d = new DateTime(date.Year, date.Month, date.Day, 2, 0, 0);
                    for (DateTime n = d; n <= d.AddDays(1); n = n.AddHours(2))
                    {
                        DateTime n2 = n.AddHours(2);
                        var row = new List<string>();



                        string q1 = "select top 1 Value from SignalsAnalogs where TagId= 624  and Datetime between '" +
                      n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                        /*_tempOut = */
                        string q2 = "select top 1 Value from SignalsAnalogs where TagId=625  and Datetime between '" +
                          n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                        /*_level = ""*/
                        string q3 = "select top 1 Value from SignalsAnalogs where TagId=626  and Datetime between '" +
                                    n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                        /*_gasPress =*/
                        string q4 = "select top 1 Value from SignalsAnalogs where TagId=627  and Datetime between '" +
                                    n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                        /*_razr = "";*/
                        string q5 = "select top 1 Value from SignalsAnalogs where TagId=628  and Datetime between '" +
                                    n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                        /*_Pperepad =*/
                        string q6 = "select top 1 Value from SignalsAnalogs where TagId=629  and Datetime between '" +
                                    n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                        /*_flame = ""*/
                        string q7 = "select top 1 Value from SignalsAnalogs where TagId=630  and Datetime between '" +
                                    n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";


                        var tempTN = MyDB.sql_query_local(q1);
                        var tempOut = MyDB.sql_query_local(q2);
                        var level = MyDB.sql_query_local(q3);
                        var gasPress = MyDB.sql_query_local(q4);
                        var razr = MyDB.sql_query_local(q5);
                        var Pperepad = MyDB.sql_query_local(q6);
                        var flame = MyDB.sql_query_local(q7);

                        var t = 1;
                        var p = 2;
                        var e = 3;

                        row.Add(n.TimeOfDay.ToString());
                        row.Add((tempTN.count_rows > 0) ? ProcFl(tempTN.GetValue(0, 0), t) : "");
                        row.Add((tempOut.count_rows > 0) ? ProcFl(tempOut.GetValue(0, 0), t) : "");
                        row.Add((level.count_rows > 0) ? ProcFl(level.GetValue(0, 0), t) : "");
                        row.Add((gasPress.count_rows > 0) ? ProcFl(gasPress.GetValue(0, 0), p) : "");
                        row.Add((razr.count_rows > 0) ? ProcFl(razr.GetValue(0, 0), p) : "");
                        row.Add((Pperepad.count_rows > 0) ? ProcFl(Pperepad.GetValue(0, 0), p) : "");
                        row.Add((flame.count_rows > 0) ? ProcFl(flame.GetValue(0, 0), t) : "");

                        rez.Add(row);
                    }
                }
                logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                report.Name = name;
                report.Head = new List<string>
                {
                    "Время чч:мм",
                    "Температура теплоносителя С",
                    "Температура подогреваемой среды С ",
                    "Уровень теплоносителя  ",
                    "Давление топливного газа мПа",
                    "Разряжение ",
                    "Перепад давления на фильтре мПа",
                    "Датчик пламени %"
                };
                report.Rows = rez;
            }
            catch (Exception ex)
            {
                return report;
            }
            return report;
        }
        public Report StartStopReport(string name, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            var rez = new List<List<string>>();
            var StartDate = DateTime.Now;
            var EndDate = DateTime.Now.AddDays(-1);
            int? AgzuId = null;
            int? WellId = null;
            var listobj = GetAgzuOrWell();
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    StartDate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "EndDate")
                    EndDate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "AgzuId")
                    AgzuId = Convert.ToInt32(parameter.Value);
                if (parameter.Key == "WellId")
                    WellId = Convert.ToInt32(parameter.Value);
            }
            try
            {
                using (_context)
                {
                    string q = "";
                    q = q + "declare                      ";
                    q = q + " @gzu as int,                    ";
                    q = q + " @wid as integer,                ";
                    q = q + " @sum as datetime,               ";
                    q = q + " @endtime as datetime,           ";
                    q = q + " @starttime as datetime,    ";
                    q = q + " @n as integer,     @tempstart as bigint, @tempend as bigint, @stopflag as bit ,@cur as float,     @w as int,@cur_ as float,     @w_ as int ";
                    q = q + "set @starttime = CONVERT(smalldatetime, '" + StartDate + "')                                              ";
                    q = q + "set @endtime = CONVERT(smalldatetime, '" + EndDate + "')                                                ";

                    q = q + GetAgzuOrWellstr(GzuId: AgzuId, WellId: WellId);

                    q = q + "declare @well2 table(id int, name varchar(max), stop int, start int)";
                    q = q + " insert into @well2";

                    //q =q+ " select well.wellid,well.wellName,stop.Id as stop,start.id as start from @well as well";
                    //q =q+ " left join Objects as reg on(reg.ParentId = well.wellid and reg.Name = 'Regs')";
                    //q =q+ " left join Objects as reg2 on(reg2.ParentId = well.wellid and reg2.Name = 'Regs2')";
                    //q =q+ " left join Objects as DI on(DI.ParentId = well.wellid and DI.Name = 'DI')";
                    //q =q+ " inner join Objects as stop on((stop.ParentId = reg2.Id or stop.ParentId = reg.Id or stop.ParentId = DI.Id) and(stop.Name = 'TimeLastStop' or stop.Name = '0'))  ";
                    //q =q+ " inner join Objects as start on((start.ParentId = reg2.Id or start.ParentId = reg.Id or start.ParentId = DI.Id) and(start.Name = 'TimeLastStart' or start.Name = '0')) ";

                    q = q + " select well.wellid,well.wellName,stop.Id as stop,start.id as start";
                    q = q + " from @well as well left join Objects as reg on(reg.ParentId = well.wellid and reg.Name = 'Regs')";
                    q = q + " left join Objects as st on(st.ParentId = reg.Id and st.Name = 'Status')";
                    q = q + " left join Objects as stst on(stst.ParentId = reg.Id and stst.Name = 'StoredStatus')";
                    q = q + " left join Objects as DI on(DI.ParentId = well.wellid and DI.Name = 'DI')";
                    q = q + " inner join Objects as stop on((stop.ParentId = st.Id or stop.ParentId = stst.Id or stop.ParentId = DI.Id) and(stop.Name = 'TimeLastStop' or stop.Name = '0'))  ";
                    q = q + " inner join Objects as start on((start.ParentId = st.Id or start.ParentId = stst.Id or start.ParentId = DI.Id) and(start.Name = 'TimeLastStart' or start.Name = '0'))";



                    q = q + " declare @journal table(id int, work int, time bigint)";
                    q = q + " insert into @journal";
                    q = q + " select  w2.id, 0,convert(bigint, Value) from @well2 as w2";
                    q = q + " inner join SignalsAnalogs as sa on(sa.TagId in (w2.stop)and Value > 1)";
                    q = q + " where sa.Datetime <= @endtime and sa.Datetime >= @starttime";
                    q = q + " insert into @journal";
                    q = q + " select  w2.id, 1 ,convert(bigint, Value) from @well2 as w2";
                    q = q + " inner join SignalsAnalogs as sa on(sa.TagId in (w2.start)and Value > 1)";
                    q = q + " where sa.Datetime <= @endtime and sa.Datetime >= @starttime";
                    q = q + " insert into @journal";
                    q = q + " select  w2.id,sa.Value,DATEDIFF(second,{ d '1970-01-01'},Datetime) from @well2 as w2";
                    q = q + " inner join SignalsDiscretes as sa on(sa.TagId in (w2.start))";
                    q = q + " where sa.Datetime <= @endtime and sa.Datetime >= @starttime";

                    q = q + " set @n = 0";
                    q = q + " declare @nt table(id_num int IDENTITY(1, 1), wellid integer, time bigint, Value float)";
                    q = q + " insert into @nt select  w.wellid,j.time,   j.work from @journal j inner join @well as w on(w.wellid = j.id) order by w.wellid,  j.time";
                    q = q + " ";
                    q = q + " create table #forcursor(id_num int IDENTITY(1,1),wellid integer, time bigint, value float)  ";
                    q = q + " create table #aftercursor(id_num int IDENTITY(1,1),wellid integer, time bigint, value float)      ";
                    q = q + " ";
                    q = q + " set @stopflag = 0";
                    q = q + " while (@stopflag = 0)";
                    q = q + " begin";
                    q = q + " set @tempstart = DATEDIFF(second,{ d '1970-01-01'},@starttime)+@n * 86400";
                    q = q + "  set @tempend = @tempstart + 86400";
                    q = q + "  if @tempend >= DATEDIFF(second,{ d '1970-01-01'},@endtime )   ";
                    q = q + "  begin";
                    q = q + "   set @tempend = DATEDIFF(second,{ d '1970-01-01'},@endtime )  ";
                    q = q + "   set @stopflag = 1";
                    q = q + "  end";
                    q = q + " ";
                    q = q + "  delete from #forcursor    ";
                    q = q + "  insert into #forcursor    ";
                    q = q + "  select wellid, time, Value from @nt where time >= @tempstart and time <= @tempend";
                    q = q + "  declare c CURSOR";
                    q = q + "  for select wellid, value from #forcursor ";
                    q = q + "  Open c";
                    q = q + "  fetch";
                    q = q + " from c into @w, @cur";
                    q = q + "  set @cur_ = @cur set @w_ = @w";
                    q = q + "  while @@fetch_status = 0";
                    q = q + "  begin";
                    q = q + "   fetch next from c into @w, @cur";
                    q = q + "   if (@@fetch_status = 0)  ";
                    q = q + "    if (@cur = @cur_ and @w = @w_) ";
                    q = q + "     delete #forcursor where current of c     ";
                    q = q + "   set @cur_ = @cur set @w_ = @w";
                    q = q + "  end";
                    q = q + "  close c";
                    q = q + "  DEALLOCATE c";
                    q = q + "  insert into #aftercursor ";
                    q = q + "  select wellid, time, value from #forcursor order by time    ";

                    q = q + "  if (@stopflag = 1)";
                    q = q + "                         break";
                    q = q + "  set @n = @n + 1";
                    q = q + " end";
                    q = q + " ";
                    q = q + " delete from @nt";
                    q = q + " insert into @nt";
                    q = q + " select wellid, time, value from #aftercursor order by wellid, time    ";
                    q = q + "                                                   ";
                    q = q + " drop table #aftercursor ";
                    q = q + " drop table #forcursor ";
                    q = q + "                                                   ";
                    q = q + " declare c CURSOR ";
                    q = q + " for select wellid, value from @nt ";
                    q = q + " ";
                    q = q + " Open c ";
                    q = q + " fetch from c into @w, @cur ";
                    q = q + "  set @cur_ = @cur set @w_ = @w ";
                    q = q + " ";
                    q = q + " while @@fetch_status = 0 ";
                    q = q + " begin ";
                    q = q + " ";
                    q = q + "  fetch next from c into @w, @cur                  ";
                    q = q + "  if (@@fetch_status = 0)                          ";
                    q = q + "   if (@cur = @cur_ and @w = @w_)                  ";
                    q = q + "    delete @nt where current of c                  ";
                    q = q + "  set @cur_ = @cur set @w_ = @w ";
                    q = q + " end ";
                    q = q + " close c ";
                    q = q + " DEALLOCATE c ";
                    q = q + " ";
                    q = q + " select Distinct gzuname, wellName,case when value = 1 then  'Пуск' else  'Останов' end as";
                    q = q + "  work , time";
                    q = q + "  from @nt j inner join @well as w on(w.wellid = j.wellid) order by time desc";
                    //q =q+ " select Distinct gzuname,wellName,case when work = 1 then  'Пуск' else  'Останов' end as work , time from @journal j";
                    //q =q+ " inner join @well as w on(w.wellid = j.id) order by time desc"; 

                    var rezult = MyDB.sql_query_local(q);
                    foreach (var _row in rezult.rows)
                    {
                        var row = new List<string>();
                        //row.Add((_row.values[0]));
                        //row.Add(WellList.FirstOrDefault(x => x.AGZU == AGZUName && x.Psm == Convert.ToInt32(_row.values[2])).Name);
                        //for (var i = 0; i < _row.values.Count; i++)
                        //    row.Add(ProcFl(_row.values[i], 3));
                        row.Add(ProcFl(_row.values[0], 3));
                        row.Add(ProcFl(_row.values[1], 3));
                        row.Add(ProcFl(_row.values[2], 3));
                        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                        dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(_row.values[3])).ToLocalTime();
                        row.Add(dtDateTime.ToString());
                        rez.Add(row);
                    }
                    logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                    report.Name = name;
                    report.Head = new List<string>
                {
                    "АГЗУ",
                    "Скважина",
                    "Событие",
                    "Дата "
                };
                    report.Rows = rez;
                }
            }
            catch (Exception ex)
            {
                return report;
            }
            return report;
        }
        public Report LastStateReport(string name, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            var rez = new List<List<string>>();
            var StartDate = DateTime.Now;
            var EndDate = DateTime.Now.AddDays(-1);
            int? AgzuId = null;
            int? WellId = null;
            var listobj = GetAgzuOrWell();
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    StartDate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "EndDate")
                    EndDate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "AgzuId")
                    AgzuId = Convert.ToInt32(parameter.Value);
                if (parameter.Key == "WellId")
                    WellId = Convert.ToInt32(parameter.Value);
            }
            try
            {
                using (_context)
                {
                    string q = "";
                    q = q + "declare                      ";
                    q = q + " @gzu as int,                    ";
                    q = q + " @wid as integer,                ";
                    q = q + " @sum as datetime,               ";
                    q = q + " @endtime as datetime,           ";
                    q = q + " @starttime as datetime          ";
                    q = q + "set @starttime = CONVERT(smalldatetime, '" + StartDate + "')                                              ";
                    q = q + "set @endtime = CONVERT(smalldatetime, '" + EndDate + "')                                                ";

                    q = q + GetAgzuOrWellstr(GzuId: AgzuId, WellId: WellId);

                    q = q + "declare @well2 table(id int, name varchar(max), stop int, start int)";
                    q = q + " insert into @well2";
                    q = q + " select well.wellid,well.wellName,stop.Id as stop,start.id as start from @well as well";
                    q = q + " left join Objects as reg on(reg.ParentId = well.wellid and reg.Name = 'Regs')";
                    q = q + " left join Objects as reg2 on(reg2.ParentId = well.wellid and reg2.Name = 'Regs2')";
                    q = q + " left join Objects as DI on(DI.ParentId = well.wellid and DI.Name = 'DI')";
                    q = q + " inner join Objects as stop on((stop.ParentId = reg2.Id or stop.ParentId = reg.Id or stop.ParentId = DI.Id) and(stop.Name = 'TimeLastStop' or stop.Name = '0'))  ";
                    q = q + " inner join Objects as start on((start.ParentId = reg2.Id or start.ParentId = reg.Id or start.ParentId = DI.Id) and(start.Name = 'TimeLastStart' or start.Name = '0')) ";
                    q = q + " declare @journal table(id int, work int, time bigint)";

                    q = q + " insert into @journal";
                    q = q + " select distinct w2.id, case when w2.start = TagId then  0 else  1 end as work ,convert(bigint, Value) from @well2 as w2 ";
                    q = q + " CROSS APPLY (select top 1 *  from SignalsAnalogs where TagId in (w2.stop,w2.start)and Value > 1 order by convert(bigint, Value) desc) as Min";



                    q = q + " insert into @journal";
                    q = q + " select  w2.id,Min.Value,DATEDIFF(second,{ d '1970-01-01'},Min.Datetime) from @well2 as w2 ";
                    q = q + " CROSS APPLY (select top 1 *  from SignalsDiscretes where TagId in (w2.start)and Value <= 1 order by Datetime desc) as Min";


                    q = q + " select Distinct gzuname,wellName,case when work = 1 then  'Пуск' else  'Останов' end as work , time from @journal j";
                    q = q + " inner join @well as w on(w.wellid = j.id)";



                    var rezult = MyDB.sql_query_local(q);

                    logger.Logged("Info", " SQL executed...rez contains " + rezult.rows, "ReportServer", "Rep2");
                    foreach (var _row in rezult.rows)
                    {
                        var row = new List<string>();
                        //row.Add((_row.values[0]));
                        //row.Add(WellList.FirstOrDefault(x => x.AGZU == AGZUName && x.Psm == Convert.ToInt32(_row.values[2])).Name);
                        //for (var i = 0; i < _row.values.Count; i++)
                        //    row.Add(ProcFl(_row.values[i], 3));
                        row.Add(ProcFl(_row.values[0], 3));
                        row.Add(ProcFl(_row.values[1], 3));
                        row.Add(ProcFl(_row.values[2], 3));
                        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                        dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(_row.values[3])).ToLocalTime();
                        row.Add(dtDateTime.ToString());
                        rez.Add(row);
                    }
                    logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                    report.Name = name;
                    report.Head = new List<string>
                {
                    "АГЗУ",
                    "Скважина",
                    "Событие",
                    "Дата "
                };
                    report.Rows = rez;
                }
            }
            catch (Exception ex)
            {
                return report;
            }
            return report;
        }

        public Report ZamReport(string name, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            var rez = new List<List<string>>();
            var StartDate = DateTime.Now;
            var EndDate = DateTime.Now.AddDays(-1);
            int? AgzuId = null;
            int? WellId = null;
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    StartDate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "EndDate")
                    EndDate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "AgzuId")
                    AgzuId = Convert.ToInt32(parameter.Value);
                if (parameter.Key == "WellId")
                    WellId = Convert.ToInt32(parameter.Value);
            }
            var agzu = GetAgzuOrWell(GzuId: AgzuId);
            logger.Logged("Info", " Report GetAgzuOrWell..." + agzu.Count(), "ReportServer", "Rep2");
            var agzulist = AGZUList();
            var agz = agzulist.FirstOrDefault(x => x.Name == agzu.FirstOrDefault().AgzuName);
            logger.Logged("Info", " Report agzulist..." + agz.Name, "ReportServer", "Rep2");
            try
            {
                using (_context)
                {

                    string q = "";
                    q = q + "declare @signals table(TagId varchar(10), Value  float, dt  datetime) "
                       + "insert into @signals select TagId,Value,Datetime from SignalsAnalogs " +
                        "where TagId between'" +
                       agz.st_id + "'  and '" + agz.end_id + "'  and Datetime between '" + StartDate.AddHours(-12).ToString() + "' and '" + EndDate.AddHours(12).ToString() + "'";

                    //if ("" != AGZUName) q = q + "and ids.agzu ='" + AGZUName + "'";
                    q = q + " select distinct '" + agz.Name + "', '" + agz.Name + "',psm.Value as psm,";
                    if (agz.Name != "АГЗУ 4003")
                    {
                        q = q + "    Convert(datetime,dbo.OznaTime(MinSec.Value,DayHour.Value,YearMont.Value)) as time,"
                    + "Tism.Value as Tism, QMfld.Value as QMfld, QVfld.Value as QVfld, Goil.Value  as Goil,";
                    }
                    else
                    {
                        q = q + "Convert(datetime,dbo.OznaEATime(Min.Value,Hour.Value,Day.Value,Mont.Value,Year.Value)) as time,"
                    + "Tism.Value as Tism, '---' as QMfld, QVfld.Value as QVfld, Goil.Value  as Goil,";
                    }

                    q = q + "  water.Value as water, pofld.Value as pofld, Mfld.Value  as Mfld,                                      "
                    + "Vfld.Value as Vfld, Moil.Value as Moil                                                         "
                    + " from    @signals as QVfld                                                                                                        "
                     + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.Psm + "  and dt<=QVfld.dt order by dt desc) as psm        ";
                    if (agz.Name != "АГЗУ 4003")
                    {
                        q = q + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.MinSec + " and dt<=QVfld.dt order by dt desc) as MinSec   "
                             + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.DayHour + " and dt<=QVfld.dt order by dt desc) as DayHour   "
                            + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.YearMont + " and dt<=QVfld.dt order by dt desc) as YearMont ";
                    }
                    else
                    {
                        q = q + "CROSS APPLY (select top 1 *  from @signals where TagId=39440 and dt<=QVfld.dt order by dt desc) as Min   "
                              + "CROSS APPLY (select top 1 *  from @signals where TagId=39439 and dt<=QVfld.dt order by dt desc) as Hour   "
                              + "CROSS APPLY (select top 1 *  from @signals where TagId=39436 and dt<=QVfld.dt order by dt desc) as Day   "
                              + "CROSS APPLY (select top 1 *  from @signals where TagId=39437 and dt<=QVfld.dt order by dt desc) as Mont   "
                              + "CROSS APPLY (select top 1 *  from @signals where TagId=39438 and dt<=QVfld.dt order by dt desc) as Year ";
                    }
                    q = q + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.Tism + " and dt<=QVfld.dt order by dt desc) as Tism         "
                          + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.QMfld + " and dt<=QVfld.dt order by dt desc) as QMfld       "
                          + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.Goil + " and dt<=QVfld.dt order by dt desc) as Goil         "
                          + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.water + " and dt<=QVfld.dt order by dt desc) as water       "
                          + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.pofld + " and dt<=QVfld.dt order by dt desc) as pofld       "
                          + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.Mfld + " and dt<=QVfld.dt order by dt desc) as Mfld         "
                          + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.Vfld + " and dt<=QVfld.dt order by dt desc) as Vfld         "
                          + "CROSS APPLY (select top 1 *  from @signals where TagId=" + agz.Moil + " and dt<=QVfld.dt order by dt desc) as Moil         "
                          + " where QVfld.TagId=" + agz.QVfld;
                    if (agz.Name != "АГЗУ 4003")
                        q = q + " and MinSec.Value>0 and Convert(datetime,dbo.OznaTime(MinSec.Value,DayHour.Value,YearMont.Value)) between '" + StartDate.ToString() + "' and '" + EndDate.ToString() + "'   order by time desc";
                    else
                        q = q + " and Convert(datetime,dbo.OznaEATime(Min.Value,Hour.Value,Day.Value,Mont.Value,Year.Value)) between '" + StartDate.ToString() + "' and '" + EndDate.ToString() + "'   order by time desc";



                    var rezult = MyDB.sql_query_local(q);
                    foreach (var _row in rezult.rows)
                    {
                        var row = new List<string>();
                        row.Add((_row.values[0]));
                        row.Add(agzu.FirstOrDefault(x => x.Psm == Convert.ToInt32(_row.values[2])).WellName);
                        for (var i = 2; i < _row.values.Count; i++)
                            row.Add(ProcFl(_row.values[i], 3));
                        rez.Add(row);
                    }
                    logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                    report.Name = name;
                    report.Head = new List<string>
                {
                    "АГЗУ",
                    "Скважина",
                    "Отвод",
                    "Дата замера",
                    "Длительность",
                    "Q жид тонн/сут",
                    "Q жид м3/сут",
                    "Q нефт тонн/сут",
                    //"Q газ нм3/сут",
                    "Обводненность %",
                    "Плотность кг/м3",
                    "Масса жидкости тонн",
                    "Объем жид м3",
                    "Масса нефти тонн ",
                    //"Объем газа м3"
                };
                    report.Rows = rez;
                }
            }
            catch (Exception ex)
            {
                logger.Logged("Error", " Report " + name + " not loaded. Reason" + ex.Message, "ReportServer", "Rep2");
                return report;
            }
            return report;
        }
        public List<Report> ReportList()
        {
            return reportList;
        }
        public string ReportName(int id)
        {
            var firstOrDefault = reportList.FirstOrDefault(x => x.Id == id);
            if (firstOrDefault != null)
                return firstOrDefault.Name;
            else return "Журнал не найден";
        }
        public Report AlarmReport(string name, Dictionary<string, dynamic> parameters)
        {

            var report = new Report();
            var rez = new List<List<string>>();
            var Sdate = DateTime.Now.AddDays(-1);
            var Edate = DateTime.Now;
            int? AgzuId = null;
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    Sdate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "EndDate")
                    Edate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "AgzuId")
                    AgzuId = Convert.ToInt32(parameter.Value);
            }

            LoadObjSign();
            List<Obj> tags = new List<Obj>();
            if (AgzuId != null)
            {
                var tagsId = GetChildTags(AgzuId);
                tags = _tags.Where(x => tagsId.Contains(x.Id)).ToList();
            }
            else
                tags = _tags;

            _cfgsEv.Clear();

            var tagsIDList = new List<int>();
            foreach (var tagjson in tags)
            {
                tagsIDList.Add(tagjson.Id);
                dynamic alarm = JsonConvert.DeserializeObject(tagjson.Prop);
                var tag = new TagId
                {
                    TagName = Convert.ToString(alarm.Connection),
                    PollerId = Convert.ToInt32(alarm.Opc)
                };
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


            try
            {

                using (_context)
                {
                    var Salarms =
                        _context.Alarms.Where(x => x.STime > Sdate && x.STime < Edate && tagsIDList.Contains(x.TagId)).OrderByDescending(x => x.STime);
                    var Salarmslist = Salarms.ToList();

                    foreach (var salarm in Salarmslist)
                    {
                        var row = new List<string>();
                        string endS = "", startS = "";

                        var index = _cfgs.FindIndex(x => x.TagId == salarm.TagId);
                        if (index != -1)
                        {
                            switch (salarm.SRes)
                            {
                                case -2:
                                    startS = _cfgs.ElementAt(index).LoloText;
                                    break;
                                case -1:
                                    startS = _cfgs.ElementAt(index).LoText;
                                    break;
                                case 0:
                                    startS =
                                        _cfgs.ElementAt(index).NormalText;
                                    break;
                                case 1:
                                    startS = _cfgs.ElementAt(index).HiText;
                                    break;
                                case 2:
                                    startS = _cfgs.ElementAt(index).HihiText;
                                    break;
                            }
                            switch (salarm.ERes)
                            {
                                case -2:
                                    endS = _cfgs.ElementAt(index).LoloText;
                                    break;
                                case -1:
                                    endS = _cfgs.ElementAt(index).LoText;
                                    break;
                                case 0:
                                    endS = _cfgs.ElementAt(index).NormalText;
                                    break;
                                case 1:
                                    endS = _cfgs.ElementAt(index).HiText;
                                    break;
                                case 2:
                                    endS = _cfgs.ElementAt(index).HihiText;
                                    break;
                            }

                        }
                        else
                        {
                            endS = "Отсутствует  конфигурация"; startS = "Отсутствует  конфигурация";
                        }
                        row.Add(startS);
                        row.Add(salarm.STime.ToString());
                        row.Add(endS);
                        row.Add(salarm.ETime.ToString());
                        row.Add(salarm.AckTime.ToString());
                        rez.Add(row);

                    }
                    logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                    report.Name = name;
                    report.Head = new List<string> { "Начало события", "Время начала", "Конец события", "Время конца", "Время квитирования" };
                    report.Rows = rez;
                }
            }

            catch (Exception ex)
            {
                return report;
            }
            return report;
        }
        public Report EventReport(string name, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            var rez = new List<List<string>>();
            var Sdate = DateTime.Now.AddDays(-1);
            var Edate = DateTime.Now;
            int? AgzuId = null;
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    Sdate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "EndDate")
                    Edate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "AgzuId")
                    AgzuId = Convert.ToInt32(parameter.Value);
            }

            LoadObjSign();
            List<Obj> tags = new List<Obj>();
            if (AgzuId != null)
            {
                var tagsId = GetChildTags(AgzuId);
                tags = _tags.Where(x => tagsId.Contains(x.Id)).ToList();
            }
            else
                tags = _tags;

            _cfgsEv.Clear();

            var tagsIDList = new List<int>();
            foreach (var tagjson in tags)
            {
                tagsIDList.Add(tagjson.Id);
                dynamic alarm = JsonConvert.DeserializeObject(tagjson.Prop);
                var tag = new TagId
                {
                    TagName = Convert.ToString(alarm.Connection),
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
            }

            try
            {
                using (_context)
                {
                    var Sevents =
                        _context.Events.Where(x => x.Time > Sdate && x.Time < Edate && tagsIDList.Contains(x.TagId)).OrderByDescending(x => x.Time);
                    var Seventslist = Sevents.ToList();


                    foreach (var salarm in Seventslist)
                    {
                        var row = new List<string>();
                        string endS = "", startS = "";


                        var outEvent = new EventThreadManagerOut();

                        outEvent.Id = salarm.Id;
                        outEvent.TagId = salarm.TagId;
                        outEvent.Time = salarm.Time;

                        var tm = _cfgsEv.FirstOrDefault(x => x.TagId == salarm.TagId) ?? new EventThreadManagerConfig();

                        if (tm.EventMessages != null)
                        {
                            foreach (var a in tm.EventMessages)
                            {
                                if (a.Value == salarm.Value)

                                    outEvent.Message = a.Message;

                            }
                        }
                        if (outEvent.Message == "" || outEvent.Message == null)
                            outEvent.Message = "Сообщение отсутствует( Tag:" + salarm.TagId + ", Value:" + salarm.Value + ")";
                        row.Add(outEvent.Time.ToString());
                        row.Add(outEvent.Message);

                        rez.Add(row);
                    }
                    logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                    report.Name = name;
                    report.Head = new List<string> { "Время", "Cобытие" };
                    report.Rows = rez;
                }
            }

            catch (Exception ex)
            {
                return report;
            }
            return report;
        }
        public Report LinkReport(string name, Dictionary<string, dynamic> parameters)
        {

            var report = new Report();
            var rez = new List<List<string>>();
            var Sdate = DateTime.Now.AddDays(-1);
            var Edate = DateTime.Now;
            int? AgzuId = null;
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    Sdate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "EndDate")
                    Edate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "AgzuId")
                    AgzuId = Convert.ToInt32(parameter.Value);
            }

            LoadObjSign();
            List<Obj> tags = new List<Obj>();
            if (AgzuId != null)
            {
                var tagsId = GetChildTags(AgzuId);
                tags = _tags.Where(x => tagsId.Contains(x.Id) && x.Name == "ObjectLink").ToList();
            }
            else
                tags = _tags.Where(x => x.Name == "ObjectLink").ToList();

            _cfgsEv.Clear();

            var tagsIDList = new List<int>();
            foreach (var tagjson in tags)
            {
                tagsIDList.Add(tagjson.Id);
                dynamic alarm = JsonConvert.DeserializeObject(tagjson.Prop);
                var tag = new TagId
                {
                    TagName = Convert.ToString(alarm.Connection),
                    PollerId = Convert.ToInt32(alarm.Opc)
                };
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


                            if (alarm_.HihiSeverity == null) alarm_.HihiSeverity = Double.MaxValue;
                            if (alarm_.HiSeverity == null) alarm_.HiSeverity = Double.MaxValue;
                            if (alarm_.LoSeverity == null) alarm_.LoSeverity = Double.MinValue;
                            if (alarm_.LoloSeverity == null) alarm_.LoloSeverity = Double.MinValue;
                            _cfgs.Add(alarm_);
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.Logged("Err", "Неверная конфигурация тревоги [" + tag.PollerId + "][" + tag.TagName + "] : " +
                        ex.Message, "AlarmThreadManager", "LoadAlarmsLastStates");
                }
            }


            try
            {

                using (_context)
                {
                    var Salarms =
                        _context.Alarms.Where(x => x.STime > Sdate && x.STime < Edate && tagsIDList.Contains(x.TagId)).OrderByDescending(x => x.STime);
                    var Salarmslist = Salarms.ToList();

                    foreach (var salarm in Salarmslist)
                    {
                        var row = new List<string>();
                        string endS = "", startS = "";

                        var index = _cfgs.FindIndex(x => x.TagId == salarm.TagId);
                        if (index != -1)
                        {
                            switch (salarm.SRes)
                            {
                                case -2:
                                    startS = _cfgs.ElementAt(index).LoloText;
                                    break;
                                case -1:
                                    startS = _cfgs.ElementAt(index).LoText;
                                    break;
                                case 0:
                                    startS =
                                        _cfgs.ElementAt(index).NormalText;
                                    break;
                                case 1:
                                    startS = _cfgs.ElementAt(index).HiText;
                                    break;
                                case 2:
                                    startS = _cfgs.ElementAt(index).HihiText;
                                    break;
                            }
                            switch (salarm.ERes)
                            {
                                case -2:
                                    endS = _cfgs.ElementAt(index).LoloText;
                                    break;
                                case -1:
                                    endS = _cfgs.ElementAt(index).LoText;
                                    break;
                                case 0:
                                    endS = _cfgs.ElementAt(index).NormalText;
                                    break;
                                case 1:
                                    endS = _cfgs.ElementAt(index).HiText;
                                    break;
                                case 2:
                                    endS = _cfgs.ElementAt(index).HihiText;
                                    break;
                            }

                        }
                        else
                        {
                            endS = "Отсутствует  конфигурация"; startS = "Отсутствует  конфигурация";
                        }
                        row.Add(startS);
                        row.Add(salarm.STime.ToString());
                        row.Add(endS);
                        row.Add(salarm.ETime.ToString());
                        row.Add(salarm.AckTime.ToString());
                        rez.Add(row);

                    }
                    logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                    report.Name = name;
                    report.Head = new List<string> { "Начало события", "Время начала", "Конец события", "Время конца", "Время квитирования" };
                    report.Rows = rez;
                }
            }

            catch (Exception ex)
            {
                return report;
            }
            return report;
        }
        public Report EventReportECN(string name, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            var rez = new List<List<string>>();
            var StartDate = DateTime.Now;
            var EndDate = DateTime.Now.AddDays(-1);
            int? AgzuId = null;
            int? WellId = null;
            var listobj = GetAgzuOrWell();
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    StartDate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "EndDate")
                    EndDate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "AgzuId")
                    AgzuId = Convert.ToInt32(parameter.Value);
                if (parameter.Key == "WellId")
                    WellId = Convert.ToInt32(parameter.Value);
            }
            try
            {
                using (_context)
                {
                    string q = "";
                    q = q + "declare                      ";
                    q = q + " @gzu as int,                    ";
                    q = q + " @wid as integer,                ";
                    q = q + " @sum as datetime,               ";
                    q = q + " @endtime as datetime,           ";
                    q = q + " @starttime as datetime,    ";
                    q = q + " @n as integer,     @tempstart as bigint, @tempend as bigint, @stopflag as bit ,@cur as float,     @w as int,@cur_ as float,     @w_ as int ";
                    q = q + "set @starttime = CONVERT(smalldatetime, '" + StartDate + "')                                              ";
                    q = q + "set @endtime = CONVERT(smalldatetime, '" + EndDate + "')                                                ";

                    q = q + GetAgzuOrWellstr(GzuId: AgzuId, WellId: WellId);

                    q = q + "declare @well2 table(id int, name varchar(max), stop int, start int)";
                    q = q + " insert into @well2";

                    //q =q+ " select well.wellid,well.wellName,stop.Id as stop,start.id as start from @well as well";
                    //q =q+ " left join Objects as reg on(reg.ParentId = well.wellid and reg.Name = 'Regs')";
                    //q =q+ " left join Objects as reg2 on(reg2.ParentId = well.wellid and reg2.Name = 'Regs2')";
                    //q =q+ " left join Objects as DI on(DI.ParentId = well.wellid and DI.Name = 'DI')";
                    //q =q+ " inner join Objects as stop on((stop.ParentId = reg2.Id or stop.ParentId = reg.Id or stop.ParentId = DI.Id) and(stop.Name = 'TimeLastStop' or stop.Name = '0'))  ";
                    //q =q+ " inner join Objects as start on((start.ParentId = reg2.Id or start.ParentId = reg.Id or start.ParentId = DI.Id) and(start.Name = 'TimeLastStart' or start.Name = '0')) ";

                    q = q + " select well.wellid,well.wellName,stop.Id as stop,start.id as start";
                    q = q + " from @well as well left join Objects as reg on(reg.ParentId = well.wellid and reg.Name = 'Regs')";
                    q = q + " left join Objects as st on(st.ParentId = reg.Id and st.Name = 'Status')";
                    q = q + " left join Objects as stst on(stst.ParentId = reg.Id and stst.Name = 'StoredStatus')";
                    q = q + " left join Objects as DI on(DI.ParentId = well.wellid and DI.Name = 'DI')";
                    q = q + " inner join Objects as stop on((stop.ParentId = st.Id or stop.ParentId = stst.Id or stop.ParentId = DI.Id) and(stop.Name = 'TimeLastStop' or stop.Name = '0'))  ";
                    q = q + " inner join Objects as start on((start.ParentId = st.Id or start.ParentId = stst.Id or start.ParentId = DI.Id) and(start.Name = 'TimeLastStart' or start.Name = '0'))";



                    q = q + " declare @journal table(id int, work int, time bigint)";
                    q = q + " insert into @journal";
                    q = q + " select  w2.id, 0,convert(bigint, Value) from @well2 as w2";
                    q = q + " inner join SignalsAnalogs as sa on(sa.TagId in (w2.stop)and Value > 1)";
                    q = q + " where sa.Datetime <= @endtime and sa.Datetime >= @starttime";
                    q = q + " insert into @journal";
                    q = q + " select  w2.id, 1 ,convert(bigint, Value) from @well2 as w2";
                    q = q + " inner join SignalsAnalogs as sa on(sa.TagId in (w2.start)and Value > 1)";
                    q = q + " where sa.Datetime <= @endtime and sa.Datetime >= @starttime";
                    q = q + " insert into @journal";
                    q = q + " select  w2.id,sa.Value,DATEDIFF(second,{ d '1970-01-01'},Datetime) from @well2 as w2";
                    q = q + " inner join SignalsDiscretes as sa on(sa.TagId in (w2.start))";
                    q = q + " where sa.Datetime <= @endtime and sa.Datetime >= @starttime";

                    q = q + " set @n = 0";
                    q = q + " declare @nt table(id_num int IDENTITY(1, 1), wellid integer, time bigint, Value float)";
                    q = q + " insert into @nt select  w.wellid,j.time,   j.work from @journal j inner join @well as w on(w.wellid = j.id) order by w.wellid,  j.time";
                    q = q + " ";
                    q = q + " create table #forcursor(id_num int IDENTITY(1,1),wellid integer, time bigint, value float)  ";
                    q = q + " create table #aftercursor(id_num int IDENTITY(1,1),wellid integer, time bigint, value float)      ";
                    q = q + " ";
                    q = q + " set @stopflag = 0";
                    q = q + " while (@stopflag = 0)";
                    q = q + " begin";
                    q = q + " set @tempstart = DATEDIFF(second,{ d '1970-01-01'},@starttime)+@n * 86400";
                    q = q + "  set @tempend = @tempstart + 86400";
                    q = q + "  if @tempend >= DATEDIFF(second,{ d '1970-01-01'},@endtime )   ";
                    q = q + "  begin";
                    q = q + "   set @tempend = DATEDIFF(second,{ d '1970-01-01'},@endtime )  ";
                    q = q + "   set @stopflag = 1";
                    q = q + "  end";
                    q = q + " ";
                    q = q + "  delete from #forcursor    ";
                    q = q + "  insert into #forcursor    ";
                    q = q + "  select wellid, time, Value from @nt where time >= @tempstart and time <= @tempend";
                    q = q + "  declare c CURSOR";
                    q = q + "  for select wellid, value from #forcursor ";
                    q = q + "  Open c";
                    q = q + "  fetch";
                    q = q + " from c into @w, @cur";
                    q = q + "  set @cur_ = @cur set @w_ = @w";
                    q = q + "  while @@fetch_status = 0";
                    q = q + "  begin";
                    q = q + "   fetch next from c into @w, @cur";
                    q = q + "   if (@@fetch_status = 0)  ";
                    q = q + "    if (@cur = @cur_ and @w = @w_) ";
                    q = q + "     delete #forcursor where current of c     ";
                    q = q + "   set @cur_ = @cur set @w_ = @w";
                    q = q + "  end";
                    q = q + "  close c";
                    q = q + "  DEALLOCATE c";
                    q = q + "  insert into #aftercursor ";
                    q = q + "  select wellid, time, value from #forcursor order by time    ";

                    q = q + "  if (@stopflag = 1)";
                    q = q + "                         break";
                    q = q + "  set @n = @n + 1";
                    q = q + " end";
                    q = q + " ";
                    q = q + " delete from @nt";
                    q = q + " insert into @nt";
                    q = q + " select wellid, time, value from #aftercursor order by wellid, time    ";
                    q = q + "                                                   ";
                    q = q + " drop table #aftercursor ";
                    q = q + " drop table #forcursor ";
                    q = q + "                                                   ";
                    q = q + " declare c CURSOR ";
                    q = q + " for select wellid, value from @nt ";
                    q = q + " ";
                    q = q + " Open c ";
                    q = q + " fetch from c into @w, @cur ";
                    q = q + "  set @cur_ = @cur set @w_ = @w ";
                    q = q + " ";
                    q = q + " while @@fetch_status = 0 ";
                    q = q + " begin ";
                    q = q + " ";
                    q = q + "  fetch next from c into @w, @cur                  ";
                    q = q + "  if (@@fetch_status = 0)                          ";
                    q = q + "   if (@cur = @cur_ and @w = @w_)                  ";
                    q = q + "    delete @nt where current of c                  ";
                    q = q + "  set @cur_ = @cur set @w_ = @w ";
                    q = q + " end ";
                    q = q + " close c ";
                    q = q + " DEALLOCATE c ";
                    q = q + " ";
                    q = q + " select Distinct gzuname, wellName,case when value = 1 then  'Пуск' else  'Останов' end as";
                    q = q + "  work , time";
                    q = q + "  from @nt j inner join @well as w on(w.wellid = j.wellid) order by time desc";
                    //q =q+ " select Distinct gzuname,wellName,case when work = 1 then  'Пуск' else  'Останов' end as work , time from @journal j";
                    //q =q+ " inner join @well as w on(w.wellid = j.id) order by time desc"; 

                    var rezult = MyDB.sql_query_local(q);
                    foreach (var _row in rezult.rows)
                    {
                        var row = new List<string>();
                        //row.Add((_row.values[0]));
                        //row.Add(WellList.FirstOrDefault(x => x.AGZU == AGZUName && x.Psm == Convert.ToInt32(_row.values[2])).Name);
                        //for (var i = 0; i < _row.values.Count; i++)
                        //    row.Add(ProcFl(_row.values[i], 3));
                        row.Add(ProcFl(_row.values[0], 3));
                        row.Add(ProcFl(_row.values[1], 3));
                        row.Add(ProcFl(_row.values[2], 3));
                        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                        dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(_row.values[3])).ToLocalTime();
                        row.Add(dtDateTime.ToString());
                        rez.Add(row);
                    }
                    logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                    report.Name = name;
                    report.Head = new List<string>
                {
                    "АГЗУ",
                    "Скважина",
                    "Событие",
                    "Дата "
                };
                    report.Rows = rez;
                }
            }
            catch (Exception ex)
            {
                return report;
            }
            return report;
        }

        public Report GetReport(int id, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            switch (id)
            {
                case 1:
                    report = ZamReport(ReportName(id), parameters);
                    break;
                case 2:
                    report = StartStopReport(ReportName(id), parameters);
                    break;
                case 3:
                    report = LastStateReport(ReportName(id), parameters);
                    break;
                case 4:
                    report = LinkReport(ReportName(id), parameters);
                    break;
                case 5:
                    report = PkReport(ReportName(id), parameters);
                    break;
                case 6:
                    report = AlarmReport(ReportName(id), parameters);
                    break;
                case 7:
                    report = EventReport(ReportName(id), parameters);
                    break;
            }
            return report;
        }

        public MemoryStream GetExcel(int id, Dictionary<string, dynamic> parameters, out string journal)
        {
            MemoryStream reportstStream = null;
            var report = GetReport(id, parameters);
            reportstStream = GetExcelReport(report, parameters);
            journal = report.Name;
            return reportstStream;
        }

        private static void InitializeWorkbook()
        {
            _workbook = new HSSFWorkbook();
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "NPOI Team";
            _workbook.DocumentSummaryInformation = dsi;
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "NPOI SDK Example";
            _workbook.SummaryInformation = si;
        }

        public MemoryStream GetExcelReport(Report report, Dictionary<string, dynamic> parameters)
        {
            InitializeWorkbook();
            var _date = DateTime.Now;
            string op1 = "";
            string op2 = "";
            #region
            ICellStyle journal_name_style = _workbook.CreateCellStyle();
            journal_name_style.Alignment = HorizontalAlignment.Center;
            //create a font style
            IFont font = _workbook.CreateFont();
            font.FontHeight = 20 * 20;
            journal_name_style.SetFont(font);

            journal_name_style.BorderRight = BorderStyle.Thin;
            journal_name_style.BorderBottom = BorderStyle.Thin;

            ICellStyle journal_period_style = _workbook.CreateCellStyle();
            journal_period_style.Alignment = HorizontalAlignment.Center;
            //create a font style
            IFont font1 = _workbook.CreateFont();
            font1.FontHeight = 14 * 14;
            journal_period_style.SetFont(font1);


            ICellStyle header = _workbook.CreateCellStyle();
            header.VerticalAlignment = VerticalAlignment.Top;
            header.WrapText = true;
            header.BorderRight = BorderStyle.Thin;
            header.BorderBottom = BorderStyle.Thin;

            ICellStyle redrow = _workbook.CreateCellStyle();
            redrow.FillForegroundColor = HSSFColor.Red.Index;
            redrow.FillPattern = FillPattern.SolidForeground;
            redrow.Alignment = HorizontalAlignment.Center;
            redrow.BorderRight = BorderStyle.Thin;
            redrow.BorderBottom = BorderStyle.Thin;

            ICellStyle greenrow = _workbook.CreateCellStyle();
            greenrow.FillForegroundColor = HSSFColor.LightGreen.Index;
            greenrow.FillPattern = FillPattern.SolidForeground;
            greenrow.Alignment = HorizontalAlignment.Center;
            greenrow.BorderRight = BorderStyle.Thin;
            greenrow.BorderBottom = BorderStyle.Thin;

            ICellStyle grey40row = _workbook.CreateCellStyle();
            grey40row.FillForegroundColor = HSSFColor.Grey80Percent.Index;
            grey40row.FillPattern = FillPattern.SolidForeground;
            grey40row.Alignment = HorizontalAlignment.Center;
            grey40row.BorderRight = BorderStyle.Thin;
            grey40row.BorderBottom = BorderStyle.Thin;

            ICellStyle grey20row = _workbook.CreateCellStyle();
            grey20row.FillForegroundColor = HSSFColor.Grey25Percent.Index;
            grey20row.FillPattern = FillPattern.SolidForeground;
            grey20row.Alignment = HorizontalAlignment.Center;
            grey20row.BorderRight = BorderStyle.Thin;
            grey20row.BorderBottom = BorderStyle.Thin;


            ICellStyle bluerow = _workbook.CreateCellStyle();
            bluerow.FillForegroundColor = HSSFColor.LightBlue.Index;
            bluerow.FillPattern = FillPattern.SolidForeground;
            bluerow.Alignment = HorizontalAlignment.Center;
            bluerow.BorderRight = BorderStyle.Thin;
            bluerow.BorderBottom = BorderStyle.Thin;

            ICellStyle zerorow = _workbook.CreateCellStyle();
            zerorow.Alignment = HorizontalAlignment.Center;
            zerorow.BorderRight = BorderStyle.Thin;
            zerorow.BorderBottom = BorderStyle.Thin;

            #endregion

            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    _date = Convert.ToDateTime(Convert.ToString(parameter.Value));
            }

            ICell cell;
            IRow row;
            string head = "Суточная отчетность";
            var col = 0; var rown = 0; var icell = 0;
            col = report.Head.Count - 1;
            rown = 0;
            HSSFSheet sheet = (HSSFSheet)_workbook.CreateSheet(head);

            IRow _name = sheet.CreateRow(rown);
            _name.HeightInPoints = 30;
            cell = _name.CreateCell(0);
            //set the title of the sheet
            cell.SetCellValue(head);
            cell.CellStyle = journal_name_style;
            var headerr = new CellRangeAddress(rown, rown, 0, col);
            sheet.AddMergedRegion(headerr);
            ((HSSFSheet)sheet).SetEnclosedBorderOfRegion(headerr, NPOI.SS.UserModel.BorderStyle.Thin, 8);
            rown++;

            IRow jor_name = sheet.CreateRow(rown);
            jor_name.HeightInPoints = 30;
            cell = jor_name.CreateCell(0);
            //set the title of the sheet
            cell.SetCellValue(report.Name);
            cell.CellStyle = journal_name_style;
            var region_name = new CellRangeAddress(rown, rown, 0, col);
            sheet.AddMergedRegion(region_name);
            ((HSSFSheet)sheet).SetEnclosedBorderOfRegion(region_name, NPOI.SS.UserModel.BorderStyle.Thin, 8);
            rown++;

            IRow row_period = sheet.CreateRow(rown);
            row_period.HeightInPoints = 30;
            ICell cell_period = row_period.CreateCell(0);
            cell_period.SetCellValue("Отчетные сутки: " + _date.Day + "." + _date.Month + "." + _date.Year + "");
            cell_period.CellStyle = journal_period_style;
            var region_date = new CellRangeAddress(rown, rown, 0, col);
            sheet.AddMergedRegion(region_date);
            ((HSSFSheet)sheet).SetEnclosedBorderOfRegion(region_date, NPOI.SS.UserModel.BorderStyle.Thin, 8);
            rown++;


            IRow date_form = sheet.CreateRow(rown);
            ICell date = date_form.CreateCell(0);
            var dateTime = DateTime.Now;
            date.SetCellValue("Журнал сформирован: " + dateTime + "");
            CellRangeAddress region = new CellRangeAddress(rown, rown, 0, col);
            sheet.AddMergedRegion(region);
            ((HSSFSheet)sheet).SetEnclosedBorderOfRegion(region, NPOI.SS.UserModel.BorderStyle.Thin, 8);
            rown++;

            row = sheet.CreateRow(rown);
            rown++;
            IRow headerRow = sheet.CreateRow(rown);
            rown++;

            for (var rc = 0; rc < report.Head.Count; rc++)
                headerRow.CreateCell(rc).SetCellValue(report.Head[rc]);

            for (var j = 0; j < report.Head.Count; j++) headerRow.GetCell(j).CellStyle = header;
            //замопозить область
            //sheet.CreateFreezePane(0, rown);  
            //sheet.SetAutoFilter(new CellRangeAddress(rown - 1, report.Head.Count + rown, 0, col));
            foreach (List<string> t in report.Rows)
            {
                row = sheet.CreateRow(rown);
                for (var j = 0; j < t.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(t[j]);
                    row.GetCell(j).CellStyle = zerorow;
                }
                rown++;
            }

            for (int i = 0; i < col + 1; i++)
            {
                sheet.AutoSizeColumn(i);
                sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 3 * 512);
            }

            if (_workbook != null)
            {
                var output = new MemoryStream();
                _workbook.Write(output);

                return output;
            }
            else
                return null;


        }


        public List<Domain.Abstract.AGZUObject> ObjectList()
        {

            var objList = new List<Domain.Abstract.AGZUObject>();

            objList = (from ti in _context.Objects
                       where (ti.Type == 5)
                        && !ti.Name.Contains("Setting") && !ti.Name.Contains("cfg") && !ti.Name.Contains("Rez")
                       select new Domain.Abstract.AGZUObject { Id = ti.Id, Name = ti.Name, ParentId = ti.ParentId }).ToList();

            return objList;
        }
        public List<Domain.Abstract.AGZUObject> ChildList(int parentId)
        {

            var q = "             DECLARE @ID INT = " + parentId + "                                                          ";
            q = q + "                                                                                                        ";
            q = q + " ; WITH ParentChildCTE                                                                                  ";
            q = q + " AS(                                                                                                    ";
            q = q + "     SELECT ID, ParentId, Type, Name                                                                    ";
            q = q + "     FROM Objects                                                                                       ";
            q = q + "     WHERE Id = @ID                                                                                     ";
            q = q + "                                                                                                        ";
            q = q + "     UNION ALL                                                                                          ";
            q = q + "                                                                                                        ";
            q = q + "     SELECT T1.ID, T1.ParentId, T1.Type, T1.Name                                                        ";
            q = q + "     FROM Objects T1                                                                                    ";
            q = q + "     INNER JOIN ParentChildCTE T ON T.ID = T1.ParentID                                                  ";
            q = q + "     WHERE T1.ParentID IS NOT NULL                                                                      ";
            q = q + "     )                                                                                                  ";
            q = q + " SELECT distinct Id,ParentId,REPLACE(Name,'Well','Скважина ')                                                                                      ";
            q = q + " FROM ParentChildCTE where   name not like 'Well%]' and name like 'Well%' and name not like 'WellsFlags'";


            var objList = new List<Domain.Abstract.AGZUObject>(); var rezult = MyDB.sql_query_local(q);
            foreach (var _row in rezult.rows)
                objList.Add(new AGZUObject { Id = Convert.ToInt32(_row.values[0]), Name = _row.values[2], ParentId = Convert.ToInt32(_row.values[1]) });

            return objList;
        }
    }
}




