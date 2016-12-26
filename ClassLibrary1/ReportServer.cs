using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NPOI;
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
using System.Data;

namespace WebSphere.Reports
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
        private static readonly JSON Json = new JSON();
        private static List<AlarmThreadManagerConfig> _cfgs= new List<AlarmThreadManagerConfig>();

        private static readonly List<Report> reportList = new List<Report>
            {
                 new Report{Id = 1,Name = "МГББ (Счетчики)"} ,
                 new Report{Id = 2,Name = "МГББ (Тех.параметры)"} ,
                 new Report{Id = 3,Name = "Площадка подогрева газа"} ,
                 //new Report{Id = 4,Name = "Площадка замера газа"} ,
                 new Report{Id = 5,Name = "Площадка конденсатосборников"},  
                 new Report{Id = 6,Name = "Журнал событий и тревог"} 
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
            return string.IsNullOrEmpty(value) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(value.Replace(".", ",")), k));
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

                logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
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

        public Report MgbbReport(string name, Dictionary<string, dynamic> parameters)
        {
            var t = 1;
            var p = 2;
            var e = 3;
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
                    double sum_nrsg_day = 0;
                    double sum_nrpg_day = 0;


                    //string f1_sum = GetTop1ValBeforeDatesById(720,d);
                    //string f1_sumN = GetTop1ValBeforeDatesById(721, d);
                    //string f2_sum = GetTop1ValBeforeDatesById(717, d);
                    //string f2_sumN = GetTop1ValBeforeDatesById(716, d);
                    
                   var f1_sum =Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(720, d)).GetValue(0, 0), 1));
                  var f1_sumN =Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(721, d)).GetValue(0, 0), 1));
                  var f2_sum =Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(717, d)).GetValue(0, 0), 1));
                  var f2_sumN = Convert.ToDouble(ProcFl(MyDB.sql_query_local(GetTop1ValBeforeDatesById(716, d)).GetValue(0, 0), 1));
                    for (DateTime n = d; n <= d.AddDays(1); n = n.AddHours(2))
                    {
                        if (n>DateTime.Now) break;
                        DateTime n2 = n.AddHours(2);
                        var row = new List<string>();

                        //string q3 = "select top 1 Value from SignalsAnalogs where TagId=718  and Datetime between '" +
                        //            n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                        //string q4 = "select top 1 Value from SignalsAnalogs where TagId=719  and Datetime between '" +
                        //            n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                          

                        //string q9 = "select top 1 Value from SignalsAnalogs where TagId=714  and Datetime between '" +
                        //            n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                        //string q10 = "select top 1 Value from SignalsAnalogs where TagId=715  and Datetime between '" +
                        //             n.ToString() + "' and '" + n2.ToString() + "' order by Datetime desc";
                  

                         
                        var pit05 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(642, n, n2)); 
                        var tt02 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(662, n, n2));
                         
                        var pit06 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(643, n, n2)); 
                        var tt03 = MyDB.sql_query_local(GetTop1ValBetweenDatesById(663, n, n2));

                        //var rsg = MyDB.sql_query_local(q3);
                        //var Nrsg = MyDB.sql_query_local(q4);
                        var SUMrsg = MyDB.sql_query_local(GetTop1ValBetweenDatesById(720, n, n2));
                        var SUMNrsg = MyDB.sql_query_local(GetTop1ValBetweenDatesById(721, n, n2)); 
                        var _SUMrsg =((SUMrsg.count_rows > 0) ? Convert.ToDouble(ProcFl(SUMrsg.GetValue(0, 0), e)) :f1_sum);
                        var _SUMNrsg=((SUMNrsg.count_rows > 0) ? Convert.ToDouble(ProcFl(SUMNrsg.GetValue(0, 0), e)) : f1_sumN);
                        var _rsg = (_SUMrsg-f1_sum ); 
                        var _Nrsg = (_SUMNrsg-f1_sumN );
                        f1_sum = _SUMrsg;
                         f1_sumN = _SUMNrsg;
                        //var rpg = MyDB.sql_query_local(q9);
                        //var Nrpg = MyDB.sql_query_local(q10);
                        var SUMrpg = MyDB.sql_query_local(GetTop1ValBetweenDatesById(717, n, n2));
                        var SUMNrpg = MyDB.sql_query_local(GetTop1ValBetweenDatesById(716, n, n2));
                        var _SUMrpg=((SUMrpg.count_rows > 0) ? Convert.ToDouble(ProcFl(SUMrpg.GetValue(0, 0), e)) : f2_sum);
                        var _SUMNrpg=((SUMNrpg.count_rows > 0) ? Convert.ToDouble(ProcFl(SUMNrpg.GetValue(0, 0), e)) : f2_sumN);  
                        var _rpg=(_SUMrpg-f2_sum);
                        var _Nrpg=(_SUMNrpg-f2_sumN );

                        f2_sum = _SUMrpg;
                        f2_sumN = _SUMNrpg;

                        row.Add(n.TimeOfDay.ToString());

                        row.Add((pit05.count_rows > 0) ? ProcFl(pit05.GetValue(0, 0), p) : "");
                        row.Add((tt02.count_rows > 0) ? ProcFl(tt02.GetValue(0, 0), t) : "");
                        row.Add(Convert.ToString(_rsg));
                        row.Add(Convert.ToString(_Nrsg));
                            sum_nrsg_day = sum_nrsg_day + _Nrsg;
                        row.Add(Convert.ToString(sum_nrsg_day));  
                        row.Add(Convert.ToString(_SUMrsg));
                        row.Add(Convert.ToString(_SUMNrsg));

                        row.Add((pit06.count_rows > 0) ? ProcFl(pit06.GetValue(0, 0), p) : "");
                        row.Add((tt03.count_rows > 0) ? ProcFl(tt03.GetValue(0, 0), t) : "");
                        row.Add(Convert.ToString(_rpg));
                        row.Add(Convert.ToString(_Nrpg)); 
                            sum_nrpg_day = sum_nrpg_day + _Nrpg;
                        row.Add(Convert.ToString(sum_nrpg_day));
                        row.Add(Convert.ToString(_SUMrpg));
                        row.Add(Convert.ToString(_SUMNrpg));

                        rez.Add(row);
                    }
                }
                logger.Logged("Info", " Report loaded..." + name, "ReportServer", "Rep2");
                report.Name = name;
                report.Head = new List<string>
                {
                    "Время чч:мм",
                    "Cыр. газ. Давление мПа",
                    "Сыр. газ. Температура С",
                    "Сыр. газ. Расход  2-часовой м3/ч ",
                    "Сыр. газ. Расход  2-часовой нм3/ч ",
                    "Сыр. газ. Нак. расход с начала суток нм3",
                    "Сыр. газ. Суммарный расход  РУ м3/ч ",
                    "Сыр. газ. Суммарный расход  НУ нм3/ч ",

                    "Подг. газ. Давление мПа",
                    "Подг. газ. Температура С",
                    "Подг. газ. Расход 2-часовой м3/ч ",
                    "Подг. газ. Расход 2-часовой нм3/ч ",
                    "Подг. газ. Нак. расход с начала суток нм3",
                    "Подг. газ. Суммарный расход РУ нм3/ч ",
                    "Подг. газ. Суммарный расход НУ нм3/ч "
                };
                report.Rows = rez;
            }
            catch (Exception ex)
            {
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

        public Report EventReport(string name, Dictionary<string, dynamic> parameters)
        {


            var report = new Report();
            var rez = new List<List<string>>();
            var Sdate = DateTime.Now.AddDays(-1);
            var Edate = DateTime.Now;
            foreach (var parameter in parameters)
            {
                if (parameter.Key == "StartDate")
                    Sdate = Convert.ToDateTime(Convert.ToString(parameter.Value));
                if (parameter.Key == "EndDate")
                    Edate = Convert.ToDateTime(Convert.ToString(parameter.Value));
            }       
            _cfgs.Clear();
                    var taglist = (from ti in _context.Objects
                        join to in _context.Properties on ti.Id equals to.ObjectId
                        where ti.Type == 2 && to.PropId == 0
                        select new {Id = ti.Id, Prop = to.Value});
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

                var tagId = Convert.ToInt32(tagjson.Id);
                var enabled = Convert.ToBoolean(alarm.Alarm_IsPermit);
                var active = Convert.ToBoolean(alarm.Alarm_IsPermit);

                var hihiText = Convert.ToString(alarm.hihiText);
                var hiText = Convert.ToString(alarm.hiText);
                var normalText = Convert.ToString(alarm.normalText);
                var loText = Convert.ToString(alarm.loText);
                var loloText = Convert.ToString(alarm.loloText);


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
                    LoloText = loloText
                });
            }
            try
            {

                using (_context)
                {
                    var Salarms =
                        _context.Alarms.Where(x => x.STime > Sdate && x.STime < Edate).OrderByDescending(x => x.STime);
                    var Salarmslist = Salarms.ToList();
             

                        foreach (var salarm in Salarmslist)
                        {
                            var row = new List<string>();
                            string endS = "", startS = "";
                            switch (salarm.SRes)
                            {
                                case -2:
                                    startS = _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().LoloText;
                                    break;
                                case -1:
                                    startS = _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().LoText;
                                    break;
                                case 0:
                                    startS =
                                        _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().NormalText;
                                    break;
                                case 1:
                                    startS = _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().HiText;
                                    break;
                                case 2:
                                    startS = _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().HihiText;
                                    break;
                            }
                            switch (salarm.ERes)
                            {
                                case -2:
                                    endS = _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().LoloText;
                                    break;
                                case -1:
                                    endS = _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().LoText;
                                    break;
                                case 0:
                                    endS = _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().NormalText;
                                    break;
                                case 1:
                                    endS = _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().HiText;
                                    break;
                                case 2:
                                    endS = _cfgs.Where(x => x.TagId == salarm.TagId).FirstOrDefault().HihiText;
                                    break;
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
                    }             }

            catch (Exception ex)
            {
                return report;
            }
            return report;
                    }
                
            
        

        public
            Report GetReport(int id, Dictionary<string, dynamic> parameters)
        {
            var report = new Report();
            switch (id)
            {
                case 1:
                    report = MgbbReport(ReportName(id), parameters);
                    break;
                case 2:
                    report = MgbbReport2(ReportName(id), parameters);
                    break;
                case 3:
                    report = PgReport(ReportName(id), parameters);
                    break;
                case 4:
                   /// report = PuzgReport(ReportName(id), parameters);
                    break;
                case 5:
                    report = PkReport(ReportName(id), parameters);
                    break;
                case 6:
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

            //create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "NPOI Team";
            _workbook.DocumentSummaryInformation = dsi;

            //create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "NPOI SDK Example";
            _workbook.SummaryInformation = si;
        }

        public MemoryStream GetExcelReport(Report report, Dictionary<string, dynamic> parameters)
        {
            InitializeWorkbook();
            var _date = DateTime.Now;
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
                {
                    _date = Convert.ToDateTime(Convert.ToString(parameter.Value));
                }
            }
            ICell cell;
            IRow row;
            string head = "Суточная отчетность ГСУ Метели";
            //end of styles
            var col = 0;
            var rown = 0;
            var icell = 0;
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

            //Create new Excel Sheet 
            IRow headerRow = sheet.CreateRow(rown);
            rown++;

            for (var rc = 0; rc < report.Head.Count; rc++)
                headerRow.CreateCell(rc).SetCellValue(report.Head[rc]);

            for (var j = 0; j < report.Head.Count; j++) headerRow.GetCell(j).CellStyle = header;
            //замопозить область
            //sheet.CreateFreezePane(0, rown); 
            //filters
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

            rown++;
            rown++;

            row = sheet.CreateRow(rown);
            row.CreateCell(0).SetCellValue("Смену сдал:");
            row.CreateCell(Convert.ToInt32(col / 2)).SetCellValue("Смену принял:");
            rown++;
            row = sheet.CreateRow(rown);
            row.CreateCell(0).SetCellValue("Ф.И.О:");
            row.CreateCell(Convert.ToInt32(col / 2)).SetCellValue("Ф.И.О:");
            rown++;


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
    }


}