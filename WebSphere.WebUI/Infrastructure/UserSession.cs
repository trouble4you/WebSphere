using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebSphere.WebUI.Infrastructure
{
    public class UserSession
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public int IsActive { get; set; }
        public int IsSuperuser { get; set; }
        public string LastLogin { get; set; }
        public string DateJoined { get; set; }

        public static UserSession Current
        {
            get
            {
                UserSession session = (UserSession)HttpContext.Current.Session["User"];
                if (session == null)
                {
                    HttpContext.Current.Session["User"] = session;
                }
                return session;
            }
        }
    }
}
