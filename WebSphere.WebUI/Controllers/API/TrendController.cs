using System;
using System.Collections.Generic;
using System.Web.Http;

using System.Net.Http.Formatting;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;

namespace WebSphere.WebUI.Controllers.API
{
    public class TrendController : ApiController
    {
        [HttpPost]
        public MyTrendData GetData(FormDataCollection data)
        {
            //начало тренда
            string sd1 = data.Get("start_date");
            //конец тренда
            string ed1 = data.Get("end_date");
            //список сигналов тренда
            string signalId = data.Get("signal_id");
            return MvcApplication.Trends.GetTrend(sd1, ed1, signalId);
        }
        [HttpPost]
        public List<MyTrend> GetDataOpc(FormDataCollection data)
        {
            //список сигналов тренда
            string signalId = data.Get("signal_id");
            return MvcApplication.Trends.GetTrendOpc(signalId);
        }
    }
}
