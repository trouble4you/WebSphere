using System;
using System.Collections.Generic;
using System.Linq;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;

namespace WebSphere.Trends
{
    public class Trend : ITrends
    {

        private EFDbContext context = new EFDbContext();
        private static readonly Logging logger = new Logging();
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToLocalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
        public static List<int> ParseSignalsList(string s)
        {
            var signals = new List<int>();
            int state = 0;
            string tmp = "";
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (state == 0 && c == '{') state = 1;
                else if (state == 1 && c == ',')
                {
                    signals.Add(Convert.ToInt32(tmp));
                    tmp = "";
                }
                else if (state == 1 && c == '}')
                {
                    if (tmp.Length > 0) signals.Add(Convert.ToInt32(tmp));
                    break;
                }
                else if (state == 1)
                {
                    tmp += c;
                }
            }

            return signals;
        }

        public MyTrendData GetTrend(string sd1, string ed1, string signalId)
        {
            var res = new MyTrendData();
            //начало тренда
       /*     string sd1 = data.Get("start_date");
            //конец тренда
            string ed1 = data.Get("end_date");
            //список сигналов тренда
            string signalId = data.Get("signal_id");
            */
            List<int> signals = ParseSignalsList(signalId);

           /* string reset_cach = data.Get("reset_cach");
            string object_id = data.Get("object_id");
            string object_type_id = data.Get("object_id");
            string auto_update = data.Get("auto_update");
            */
            if (sd1 == null || ed1 == null)
            {
                sd1 = "0";
                ed1 = "0";
            }
            sd1 = sd1.Replace(',', '.');
            ed1 = ed1.Replace(',', '.');

            var sD = Convert.ToUInt64(sd1);
            var eD = Convert.ToUInt64(ed1);

            var dt1 = ConvertFromUnixTimestamp(sD);
            var dt2 = ConvertFromUnixTimestamp(eD);
            if (sD > 2000000000) return res;
            if (eD > 2000000000) return res;

       /*     if (auto_update != "0")
            {
                dt2 = DateTime.UtcNow;
                dt1 = dt2.AddSeconds(-1 * Convert.ToInt32(auto_update));

                sD = (ulong)ConvertToUnixTimestamp(dt1);
                eD = (ulong)ConvertToUnixTimestamp(dt2);

            }
            */

            double max = 0;
            double min = 0;

            var myTrends = new List<MyTrend>();
            foreach (int sid in signals)
            {
                var myTrend = new MyTrend();
                myTrend.Points = new List<Point>();
                res.signals.Add(sid);
                myTrend.id = (ulong)sid;
                var analog = context.SignalsAnalog.FirstOrDefault(x => (x.TagId == sid));
                var discrete = context.SignalsDiscrete.FirstOrDefault(x => (x.TagId == sid));
                var points = new List<Point>();
                if (analog != null)
                {
                    //запись до начала тренда 
                    var _analog = context.SignalsAnalog.Where(x => (x.TagId == sid && x.Datetime < dt1))
                        .OrderByDescending(x => x.Datetime)
                        .FirstOrDefault();
                    //запись после конца тренда 
                    var analog_ = context.SignalsAnalog.Where(x => (x.TagId == sid && x.Datetime > dt2))
                        .OrderBy(x => x.Datetime)
                        .FirstOrDefault();
                    //записи в промежутке между началом тренда и его концом
                    var analogs =
                        context.SignalsAnalog.Where(x => (x.TagId == sid && x.Datetime > dt1 && x.Datetime < dt2)).ToList();

                    if (_analog != null) analogs.Add(_analog);
                    if (analog_ != null) analogs.Add(analog_);
                    points.AddRange(analogs.Select(sa => new Point
                    {
                        v = sa.Value,
                        dt = (ulong)ConvertToUnixTimestamp(sa.Datetime)
                    }));
                }
                else if (discrete != null)
                {
                    var _discrete = context.SignalsDiscrete.Where(x => (x.TagId == sid && x.Datetime < dt1))
                        .OrderByDescending(x => x.Datetime)
                        .FirstOrDefault();
                    var discrete_ = context.SignalsDiscrete.Where(x => (x.TagId == sid && x.Datetime > dt2))
                        .OrderBy(x => x.Datetime)
                        .FirstOrDefault();
                    var discretes =
                        context.SignalsDiscrete.Where(x => (x.TagId == sid && x.Datetime > dt1 && x.Datetime < dt2)).ToList();

                    if (_discrete != null) discretes.Add(_discrete);
                    if (discrete_ != null) discretes.Add(discrete_);
                    points.AddRange(discretes.Select(sa => new Point
                    {
                        v = Convert.ToDouble(sa.Value),
                        dt = (ulong)ConvertToUnixTimestamp(sa.Datetime)
                    }));
                }

                var maxsign = points.OrderByDescending(x => x.v).FirstOrDefault();
                var minsign = points.OrderBy(x => x.v).FirstOrDefault();

                if (minsign != null && minsign.v < min) min = minsign.v;
                if (maxsign != null && maxsign.v > max) max = maxsign.v;

                foreach (var point in points.OrderBy(x => x.dt))
                {
                    myTrend.Points.Add(point);
                }

                if (myTrend.Points.Count > 0 && myTrend.Points.Last().dt < eD)
                {
                    var p = myTrend.Points.Last();
                    var p2 = new Point { v = p.v, dt = eD };
                    myTrend.Points.Add(p2);
                }
                myTrends.Add(myTrend);
            }
            res.trends = myTrends;
            res.min = min;
            res.max = max;

            res.date_min_sec = sD;
            res.date_max_sec = eD;

            return res;
        }  
    }



}
