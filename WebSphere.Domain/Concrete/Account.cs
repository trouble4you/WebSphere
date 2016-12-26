using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Concrete
{
    public class Account : IAccount
    {
        #region настройки

        // коннект к репозиторию данных
        private readonly EFDbContext context = new EFDbContext();

        // json
        private static readonly JSON json = new JSON();

        #endregion

        // аутентификация
        public bool Authenticate(string username, string password)
        {
            var crypto = new SimpleCrypto.PBKDF2();

            bool isValid = false;

            // тип объекта
            var objType = context.ObjectTypes.FirstOrDefault(t => t.Name == "User").Id;

            // ищем пользователя по логину
            var user = context.Objects.FirstOrDefault(u => u.Name == username && u.Type == objType);

            if (user != null)
            {
                // экземпляр объекта User для десереализации JSON
                User u = new User();

                // находим серелизованную строку свойтсв пользователя
                var q = context.Properties.FirstOrDefault(i => i.PropId == (context.PropTypes.FirstOrDefault(j => j.Name == "JSON obj").Id) && i.ObjectId == user.Id);

                // десерелизуем свойства пользователя
                var props = json.Deserialize(q.Value, u.GetType());

                // наполняем объект свойствами
                u = (User)props;

                // проверяем пароль
                if (u.Password == crypto.Compute(password, u.PasswordSalt))
                {
                    // если пользователь активен, то доступ в систему разрешен
                    if (u.IsActive == 1)
                    {
                        // записываем время последнего входа
                        u.LastLogin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                        // серелизация User для обновления времени послед. входа
                        q.Value = json.Serialize(u);

                        // сохраняем изменения
                        context.SaveChanges();
                    }

                    isValid = true;
                }
                else
                {
                    isValid = false;
                }
            }
            else
            {
                isValid = false;
            }

            return isValid;
        }

        // авторизация
        public bool HasAccess(string controller, string action, string username, List<Role> roles)
        {
            // вход в ContentType controller (необходимо создать пользователя 'developer' либо другого, чтобы туда войти)
            if (controller == "ContentType" && username != "developer")
            {
                return false;
            }

            // список пользователей
            List<User> users = new List<User>();

            // получаем данные пользователя по 'username'
            users = UsersList(username);

            // экзепляр 'User'
            User u = users[0];

            // если пользователь неактивен, запрещаем доступ
            if (u.IsActive == 0)
            {
                return false;
            }

            // если суперпользователь, то сразу даем разрешение на доступ
            if (u.IsSuperuser == 1)
            {
                return true;
            }

            // роли пользователя
            var q = GetUserRoles(username);

            // роли пользователя
            foreach (var i in q)
            {
                // все роли
                foreach (var j in roles)
                {
                    // поиск роли
                    if (i.Name == j.Name)
                    {
                        // свойства роли
                        foreach (var f in j.Permissions)
                        {
                            // поиск контроллера
                            if (f.Name == controller)
                            {
                                // разрешения роли
                                foreach (var g in f.Permission)
                                {
                                    // поиск экшена
                                    if (g.Name == action && g.Selected)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        // help - список всех пользователей
        public List<User> UsersList(string username = "")
        {
            // список
            List<User> users = new List<User>();

            // выборка
            var q = (from t in context.Objects
                    join t1 in context.ObjectTypes on t.Type equals t1.Id
                    join t2 in context.Properties on t.Id equals t2.ObjectId
                    where t1.Name == "User" && (string.IsNullOrEmpty(username) || t.Name == username)
                    select new 
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Type = t.Type,
                        Props = t2.Value
                    }).ToList();

            // список пользователей
            foreach (var i in q)
            {
                var user = new User(); // пользователь
                user = (User)json.Deserialize(i.Props, user.GetType()); // наполняем свойствами
                user.Id = i.Id; // id
                user.UserName = i.Name; // логин
                user.Type = i.Type; // тип объекта
                users.Add(user); // в список
            }

            // роли пользователей
            foreach (var i in users)
            {
                // если не Суперпользователь, то берем роли
                if (i.IsSuperuser == 0)
                {
                    i.Roles = GetUserRoles(i.UserName);
                }
            }

            return users;
        }

        // регистрация пользователя
        public bool CreateUser(User user)
        {
            // тип объекта
            var objType = context.ObjectTypes.FirstOrDefault(t => t.Name == "User").Id;

            // ищем пользователя по логину
            var userExists = context.Objects.FirstOrDefault(u => u.Name == user.UserName && u.Type == objType);

            // проверка на существование пользователя
            if (userExists == null)
            {
                // сначала добавляем роли пользователя, если не 'Superadmin' и если роли вообще есть
                if (user.IsSuperuser != 1 && user.Roles != null)
                {
                    AddUserToRole(user.UserName, user.Roles);
                }

                // создание нового пользователя
                var userName = context.Objects.Create();
                // логин
                userName.Name = user.UserName;
                // тип объекта
                userName.Type = objType;
                // добавляем username в Objects
                context.Objects.Add(userName);
                // сохраняем
                context.SaveChanges();

                // создание свойств пользователя
                var userProps = context.Properties.Create();
                // id нового пользователя (узнаем с помощью userName.Id - это как Last Inserted Id)
                userProps.ObjectId = userName.Id;
                // тип свойтсва
                userProps.PropId = context.PropTypes.FirstOrDefault(i => i.Name == "JSON obj").Id;
                // crypto объект для пароля
                var crypto = new SimpleCrypto.PBKDF2();
                // генерация шифрованного пароля
                var encrPass = crypto.Compute(user.Password);

                // наполняем объект User остальными свойствами
                user.Id = userName.Id;
                user.Password = encrPass;
                user.PasswordSalt = crypto.Salt;
                user.IsActive = user.IsActive;
                user.LastLogin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                user.DateJoined = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                // серелизация объекта User
                var props = json.Serialize(user);
                // значения свойств
                userProps.Value = props;
                // добавляем свойства пользователя
                context.Properties.Add(userProps);
                // сохраняем
                context.SaveChanges();

                return true;
            }

            return false;
        }

        // изменение пользователя
        public bool ChangeUser(User user)
        {
            // тип объекта
            var objType = context.ObjectTypes.FirstOrDefault(t => t.Name == "User").Id;

            // ищем пользователя по id 
            var currentUser = context.Objects.FirstOrDefault(u => u.Id == user.Id);

            // добавляем роли только не для 'admin' и если роли вообще есть
            if (currentUser.Name != "admin" && user.Roles != null)
            {
                // сначала добавляем роли пользователя
                AddUserToRole(currentUser.Name, user.Roles);
            }

            ///
            /// Логин НЕ обновляем
            /// 

            /*
            // если логин не совпадает с текущим, значит пользователю поменяли логин
            if (currentUser.Name != user.UserName)
            {
                // ищем пользователя по новому логину
                var userExists = context.Objects.FirstOrDefault(u => u.Name == user.UserName && u.Type == objType);

                // проверка на существование пользователя
                if (userExists == null)
                {
                    // обновление логина по Id
                    context.Objects.FirstOrDefault(u => u.Id == user.Id).Name = user.UserName;
                    // сохраняем
                    context.SaveChanges();
                }
                else
                {
                    return false;
                }
            }
            */

            // обновляем остальные данные

            // экземпляр объекта User для десереализации JSON
            User uProps = new User();

            // находим серелизованную строку свойтсв пользователя
            var q = context.Properties.FirstOrDefault(i => i.PropId == (context.PropTypes.FirstOrDefault(j => j.Name == "JSON obj").Id) && i.ObjectId == user.Id);

            // десерелизуем свойства пользователя
            var props = json.Deserialize(q.Value, uProps.GetType());

            // наполняем объект свойствами
            uProps = (User)props;

            uProps.Name = user.Name;
            uProps.LastName = user.LastName;
            uProps.MiddleName = user.MiddleName;
            uProps.Email = user.Email;

            // не для 'admin'
            if (currentUser.Name != "admin")
            {
                uProps.IsActive = user.IsActive;
                uProps.IsSuperuser = user.IsSuperuser;

                // если пользователю установлен статус Суперпользователя, удаляем все его роли
                if (user.IsSuperuser == 1)
                {
                    RemoveRolesFromUser(user.UserName);
                }
            }
            
            // если пароль null или empty, то значит пользователь сменил пароль
            if (!string.IsNullOrEmpty(user.Password))
            {
                // crypto объект для пароля
                var crypto = new SimpleCrypto.PBKDF2();
                // генерация шифрованного пароля
                var encrPass = crypto.Compute(user.Password);

                // переопределяем пароль в User
                uProps.Password = encrPass;
                uProps.PasswordSalt = crypto.Salt;
            }

            // серелизация свойств
            q.Value = json.Serialize(uProps);

            // сохраняем изменения
            context.SaveChanges();

            return true;
        }

        // удаление пользователя
        public bool DeleteUser(List<User> users)
        {
            // сначала удаляем все роли у пользователя
            foreach (var i in users)
            {
                RemoveRolesFromUser(i.UserName);
            }

            // удаляем свойства пользователя
            foreach (var i in users)
            {
                var q = from t1 in context.Properties
                        where t1.ObjectId == i.Id
                        select t1;

                context.Properties.RemoveRange(q);
                context.SaveChanges();
            }

            // удаляем пользователя
            foreach (var i in users)
            {
                var q = from t1 in context.Objects
                        where t1.Id == i.Id
                        select t1;

                context.Objects.RemoveRange(q);
                context.SaveChanges();

            }

            return true;
        }

        // список всех ролей
        //public List<Role> GetAllRoles()
        //{
        //    // список
        //    List<Role> Roles = new List<Role>();

        //    // выборка
        //    var q = from t1 in context.Objects
        //            join t2 in context.ObjectTypes on t1.Type equals t2.Id
        //            where t2.Name == "Group"
        //            select new
        //            {
        //                Id = t1.Id,
        //                Name = t1.Name,
        //                Type = t1.Type
        //            };

        //    // наполняем список
        //    foreach (var i in q)
        //    {
        //        // экземпляр объекта
        //        Role role = new Role();

        //        // добавляем роль
        //        role.Id = i.Id;
        //        role.Name = i.Name;
        //        role.Type = i.Type;

        //        // добавляем в список
        //        Roles.Add(role);
        //    }

        //    return Roles;
        //}

        // роли пользователя

        public List<Role> GetUserRoles(string username)
        {
            // список
            List<Role> Roles = new List<Role>();

            // выборка
            var q = (from t1 in context.Objects
                     join t2 in context.Objects on t1.Id equals t2.ParentId
                     join t3 in context.ObjectTypes on t2.Type equals t3.Id
                     where t2.Name == username && t3.Name == "UserGroups"
                     select new
                     {
                         Id = t1.Id,
                         Name = t1.Name,
                         Type = t1.Type,
                         Selected = true

                     }).ToList();

            // наполняем список
            foreach (var i in q)
            {
                // роль
                Role role = new Role()
                {
                    Id = i.Id,
                    Name = i.Name,
                    Type = i.Type,
                    Selected = i.Selected
                };

                // в список
                Roles.Add(role);
            }

            return Roles;
        }

        // добавление роли
        public bool CreateRole(string name, List<ContentType> ct)
        {
            // ищем занята ли роль
            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t1.Name == name && t2.Name == "Group"
                    select t1;

            // если роль не занята, то результат запроса - '0' строк
            if (q.Count() == 0)
            {
                // создание новой роли
                var new_role = context.Objects.Create();
                new_role.Name = name; // имя роли
                new_role.Type = context.ObjectTypes.FirstOrDefault(c => c.Name == "Group").Id; // тип объекта

                // добавляем в Objects
                context.Objects.Add(new_role);

                // сохраняем
                context.SaveChanges();

                // добавляем свойства роли
                var props = context.Properties.Create();
                props.ObjectId = new_role.Id; // Id
                props.PropId = context.PropTypes.FirstOrDefault(c => c.Name == "JSON obj").Id; // тип свойтсва
                props.Value = GetJsonRoleProperties(ct); // получаем серелизованную json - строку свойств роли

                // добавляем в Properties
                context.Properties.Add(props);

                // сохраняем
                context.SaveChanges();

                return true;
            }
            
            return false;
        }

        // изменение роли
        public bool ChangeRole(int id, string name, List<ContentType> ct)
        {
            // ищем занята ли роль, исключая саму себя
            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t1.Name == name && t1.Id != id && t2.Name == "Group"
                    select t1;

            // если роль не занята, то результат запроса - '0' строк
            if (q.Count() == 0)
            {
                // свойства роли
                var p = context.Properties.FirstOrDefault(c => c.ObjectId == id);

                // получаем серелизованную json - строку свойств роли и присваиваем значение к 'p.value'
                p.Value = GetJsonRoleProperties(ct);

                // сохраняем
                context.SaveChanges();

                return true;
            }

            return false;
        }

        // удаление роли
        public bool DeleteRole(List<Role> roles)
        {
            // удаление разрешений роли
            foreach (var i in roles)
            {
                RemoveRolePermissions(i.Id);
            }

            // удаление роли у пользователей, удаление самой роли
            foreach (var i in roles)
            {
                var q = from t1 in context.Objects
                        join t2 in context.ObjectTypes on t1.Type equals t2.Id
                        where (t1.ParentId == i.Id && t2.Name == "UserGroups") || (t1.Id == i.Id && t2.Name == "Group")
                        select t1;

                context.Objects.RemoveRange(q);
                context.SaveChanges();
            }

            return true;
        }

        // help - свойства роли в виде JSON строки
        public string GetJsonRoleProperties(List<ContentType> ct)
        {
            // разрешения
            List<RolePermissionsTemplate> rpt_list = new List<RolePermissionsTemplate>();

            // заполняем разрешения
            foreach (var i in ct)
            {
                // если тип контента 'Selected'
                if (i.Selected)
                {
                    // экземпляр 'RolePermissionsTemplate'
                    RolePermissionsTemplate rpt = new RolePermissionsTemplate();

                    // контроллер
                    rpt.Name = i.Actions[0].Name;

                    // разрешения
                    List<Permission> perms = new List<Permission>();

                    // экшены
                    foreach (var j in i.Actions)
                    {
                        // если разрешение 'Selected'
                        if (j.Selected)
                        {
                            // разрешение
                            Permission perm = new Permission()
                            {
                                Name = j.Name,
                                Title = j.Title,
                                Selected = j.Selected
                            };

                            // добавляем в список
                            perms.Add(perm);
                        }
                    }

                    // экшены
                    rpt.Permission = perms;

                    // удаляем контроллер
                    rpt.Permission.RemoveAt(0);

                    // в общий список
                    rpt_list.Add(rpt);
                }
            }

            // возвращаем серелизованную строку разрешений роли
            return json.Serialize(rpt_list);
        }

        // help - данные роли
        public Role GetRoleData(string name, List<Role> roles)
        {
            // роль
            Role role = new Role();

            // поиск роли 'name'
            var q = roles.FindAll(x => x.Name.StartsWith(name));

            if (q.Count() > 0)
            {
                var res = q.FirstOrDefault();

                role.Id = res.Id;
                role.Name = res.Name;
                role.Type = res.Type;
                role.Selected = res.Selected;
                role.Permissions = res.Permissions;
            }

            return role;
        }

        // добавление пользователя к ролям
        public bool AddUserToRole(string username, List<Role> roles)
        {
            // сначала удаляем все роли у пользователя
            if (RemoveRolesFromUser(username))
            {
                // новые роли
                List<Objects> objs = new List<Objects>();

                // тип объекта
                var typeObj = context.ObjectTypes.FirstOrDefault(t => t.Name == "UserGroups").Id;

                // поиск отмеченных ролей
                foreach (var i in roles)
                {
                    // если роль отмечена
                    if (i.Selected)
                    {
                        // объект
                        Objects obj = new Objects()
                        {
                            Name = username, // username
                            Type = typeObj, // тип объекта
                            ParentId = i.Id // роль
                        };

                        objs.Add(obj); // в список
                    }
                }

                // если список не пустой, то сохраняем данные
                if (objs.Count() > 0)
                {
                    context.Objects.AddRange(objs); // добавляем
                    context.SaveChanges(); // сохраняем
                }

                return true;
            }

            return false;
        }

        // удаление ролей пользователя
        public bool RemoveRolesFromUser(string username)
        {
            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t1.Name == username && t2.Name == "UserGroups"
                    select t1;

            context.Objects.RemoveRange(q);
            context.SaveChanges();

            return true;
        }

        // удаление разрешений роли
        public bool RemoveRolePermissions(int Id)
        {
            var q = from t1 in context.Properties where t1.ObjectId == Id select t1;
            context.Properties.RemoveRange(q);
            context.SaveChanges();

            return true;
        }
    }
}
