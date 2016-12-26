using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;
using WebSphere.WebUI.App_Start;

namespace WebSphere.WebUI.Helpers
{
    public static class DisplayContentBasedOnRole
    {
        private static readonly Account account = new Account();

        public static bool IsAccess(string username, string controller, string action)
        {
            // список всех ролей (загружен при старте приложения)
            var roles = RoleConfig.GetAllRoles();

            // проверяем, есть ли доступ к контенту
            return account.HasAccess(controller, action, username, roles);
        }
    }
}