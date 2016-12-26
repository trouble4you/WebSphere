using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.WebUI.App_Start
{
    public class RoleConfig
    {
        // контекст
        private static readonly EFDbContext context = new EFDbContext();

        // json
        private static readonly JSON json = new JSON();

        // роли
        private static List<Role> Roles { get; set; }

        // поиск всех ролей
        public static void RepositoryRoles()
        {
            // список ролей
            List<Role> roles = new List<Role>();

            // все роли
            var q = (from t1 in context.Objects
                     join t2 in context.ObjectTypes on t1.Type equals t2.Id
                     join t3 in context.Properties on t1.Id equals t3.ObjectId
                     where t2.Name == "Group"
                     select new
                     {
                         Id = t1.Id,
                         Name = t1.Name,
                         ParentId = t1.ParentId,
                         Type = t1.Type,
                         Value = t3.Value

                     }).ToList();

            // десерилизуем весь список
            foreach (var i in q)
            {
                Role role = new Role() 
                {
                    Id = i.Id,
                    Name = i.Name,
                    Type = i.Type
                };

                // список шаблонов разрешений ролей
                List<RolePermissionsTemplate> rptLst = new List<RolePermissionsTemplate>();

                // наполняем
                rptLst = (List<RolePermissionsTemplate>)json.Deserialize(i.Value, rptLst.GetType());

                role.Permissions = rptLst;

                // в общий список
                roles.Add(role);
            }

            // и переобределяем 'Roles'
            Roles = roles;
        }

        // возвращаем роли
        public static List<Role> GetAllRoles()
        {
            return Roles;
        }
    }
}