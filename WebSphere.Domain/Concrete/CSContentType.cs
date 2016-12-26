using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Concrete
{
    public class CSContentType : ICSContentType
    {
        #region настройки
        // коннект к репозиторию данных
        private readonly EFDbContext context = new EFDbContext();

        // json
        private static readonly JSON json = new JSON();
        #endregion

        // список типов контента (можно запросить полный список / по Id / по Name типа контента)
        public List<ContentType> ContentTypeList(int? id = null, string name = "")
        {
            // список
            List<ContentType> ContentTypes = new List<ContentType>();

            // выборка
            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    join t3 in context.Properties on t1.Id equals t3.ObjectId
                    join t4 in context.PropTypes on t3.PropId equals t4.Id
                    where t2.Name == "ContentType" && t4.Name == "JSON obj" && (id == null || t1.Id == id) && (string.IsNullOrEmpty(name) || t1.Name == name)
                    select new
                    {
                        Id = t1.Id,
                        Name = t1.Name,
                        Type = t1.Type,
                        Props = t3.Value
                    };

            // список
            foreach (var i in q)
            {
                // экземпляр объекта
                ContentType contenttype = new ContentType();

                // экземпляр объекта для десереализации JSON
                List<Permission> rootobject = new List<Permission>();

                // десерелизуем свойства
                var props = json.Deserialize(i.Props, rootobject.GetType());

                // добавляем свойства
                rootobject = (List<Permission>)props;

                contenttype.Id = i.Id; // Id
                contenttype.Name = i.Name; // имя
                contenttype.Controller = rootobject[0].Name; // имя контроллера

                // удаляем контроллер из списка, оставляем только экшены
                //rootobject.RemoveAt(0);

                contenttype.Actions = rootobject; // экшены
                ContentTypes.Add(contenttype); // добавляем в список
            }

            // имя группы, в кот. входит контент
            foreach(var i in ContentTypes)
            {
                var ctg = GetContentTypeGroups(i.Id);

                foreach (var j in ctg)
                {
                    ContentGroup cg = new ContentGroup()
                    {
                        Id = j.Id,
                        Name = j.Name
                    };
                    i.contentGroup = cg;
                }
            }

            return ContentTypes;
        }

        // список групп типов контента
        public List<ContentGroup> GetContentTypeGroups(int? id)
        {
            List<ContentGroup> contentgroups = new List<ContentGroup>();

            IQueryable<Objects> q;

            // группы контентов
            if (id == null)
            {
                q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t2.Name == "ContentGroup"
                    select t1;
            }
            else
            { // группа контента
                q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    join t3 in context.Objects on t1.Id equals t3.ParentId
                    join t4 in context.ObjectTypes on t3.Type equals t4.Id
                    where t2.Name == "ContentGroup" && t3.Name == id.ToString() && t4.Name == "ContentGroupContentTypes"
                    select t1;
            }

            foreach (var i in q)
            {
                ContentGroup cg = new ContentGroup()
                {
                    Id = i.Id,
                    Name = i.Name
                };
                contentgroups.Add(cg);
            }

            return contentgroups;
        }

        // cписок групп типов контена (именно для групп)
        public List<ContentGroup> ContentGroupList(string obj)
        {
            // список групп контента
            List<ContentGroup> contentgroups = new List<ContentGroup>();

            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t2.Name == "ContentGroup" && (string.IsNullOrEmpty(obj) || t1.Name == obj)
                    select t1;

            foreach (var i in q)
            {
                // экзепляр группы контента
                ContentGroup cg = new ContentGroup()
                {
                    Id = i.Id,
                    Name = i.Name
                };
                contentgroups.Add(cg);
            }

            return contentgroups;
        }

        // существует ли группа контента
        public bool IsExistsContentGroup(string name)
        {
            // поиск
            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t1.Name == name && t2.Name == "ContentGroup"
                    select t1;
            // если кол-во строк > 0, то группа уже существует
            if (q.Count() > 0)
            {
                return true;
            }

            return false;
        }

        // входит ли группу контента какой-либо тип контента
        public bool IsAnyContentInContentGroup(int id)
        {
            // проверяем, входят ли какие-либо типы контента в нее
            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t1.ParentId == id && t2.Name == "ContentGroupContentTypes"
                    select t1;

            // если группа используется контентом
            if (q.Count() > 0)
            {
                return false;
            }

            return true;
        }

        // обновление всех ролей
        public bool GetUpdateAllRoles(List<Permission> permissions, string oldController = "")
        {
            // все роли
            var q = from t1 in context.Objects
                    join t2 in context.Properties on t1.Id equals t2.ObjectId
                    join t3 in context.ObjectTypes on t1.Type equals t3.Id
                    where t3.Name == "Group"
                    select new
                    {
                        t1.Id,
                        t1.Name,
                        t2.Value
                    };

            // экземпляр ролей для сохранения изменений
            List<Role> roles = new List<Role>();

            // прогоняем все роли
            foreach (var i in q)
            {
                // шаблоны разрешений ролей
                List<RolePermissionsTemplate> ro = new List<RolePermissionsTemplate>();

                // десерилизуем
                var props = json.Deserialize(i.Value, ro.GetType());

                // наполняем
                ro = (List<RolePermissionsTemplate>)props;

                // указывает нужно ли добавить в список ролей
                var save = 0;

                // прогоняем все экшены роли
                foreach (var j in ro.ToList())
                {
                    // поиск контроллера (если строка 'oldController' пуста, меняются только экшены)
                    if (j.Name == permissions[0].Name && string.IsNullOrEmpty(oldController))
                    {
                        // экшены из типа контента
                        foreach (var f in permissions)
                        {
                            // если экшен отключен, ищем экшен в роли
                            if (!f.Selected)
                            {
                                // экшены из роли
                                foreach (var g in j.Permission.ToList())
                                {
                                    // поиск экшена, экшен в роли включен
                                    if (f.Name == g.Name && g.Selected)
                                    {
                                        // удаляем экшен из роли
                                        j.Permission.Remove(g);

                                        // нужно добавить в список ролей
                                        save = 1;
                                    }
                                }
                            }
                        }
                    } // поиск контроллера (если строка 'oldController' не пуста, удаляем старый контроллер из роли)
                    else if (j.Name == oldController && !string.IsNullOrEmpty(oldController))
                    {
                        // убираем весь контроллер с экшенами
                        ro.Remove(j);

                        // нужно добавить в список ролей
                        save = 1;
                    }
                }

                // добавляем в список ролей
                if (save == 1)
                {
                    // роль
                    Role role = new Role()
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Permissions = ro
                    };

                    // список
                    roles.Add(role);
                }
            }

            // если список ролей не пуст, сохраняем изменения
            if (roles.Count() > 0)
            {
                foreach (var i in roles)
                {
                    // обновляем свойства роли
                    context.Properties.FirstOrDefault(p => p.ObjectId == i.Id).Value = json.Serialize(i.Permissions);

                    // сохраняем изменения
                    context.SaveChanges();
                }
            }

            return true;
        }

        // создание типа контента
        public bool CreateContentType(ContentType ct)
        {
            // ищем занят ли тип контента
            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t1.Name == ct.Name && t2.Name == "ContentType"
                    select t1;

            // если тип не занят, то результат запроса - '0' строк
            if (q.Count() == 0)
            {
                // создание нового типа контента
                var new_ct = context.Objects.Create();
                new_ct.Name = ct.Name; // тип контента
                new_ct.Type = context.ObjectTypes.FirstOrDefault(c => c.Name == "ContentType").Id; // тип объекта

                // добавляем в Objects
                context.Objects.Add(new_ct);
                // сохраняем
                context.SaveChanges();

                // добавление нового типа контента в группу
                var new_cg = context.Objects.Create();
                new_cg.Name = new_ct.Id.ToString(); // id нового типа контента (узнаем с помощью new_ct.Id - это как Last Inserted Id)
                new_cg.Type = context.ObjectTypes.FirstOrDefault(c => c.Name == "ContentGroupContentTypes").Id; // тип объекта
                new_cg.ParentId = ct.contentGroup.Id; // id группы

                // добавляем в Objects
                context.Objects.Add(new_cg);
                // сохраняем
                context.SaveChanges();

                // создание разрешений
                var new_perms = context.Properties.Create();
                new_perms.ObjectId = new_ct.Id; // id нового типа контента (узнаем с помощью new_ct.Id - это как Last Inserted Id)
                new_perms.PropId = context.PropTypes.FirstOrDefault(c => c.Name == "JSON obj").Id; // тип свойтсва
                new_perms.Value = json.Serialize(ct.Actions); // серелизация разрешений(экшенов) типа контента

                // добавляем в Properties
                context.Properties.Add(new_perms);
                // сохраняем
                context.SaveChanges();

                return true;
            }

            return false;
        }

        // обновление типа контента
        public bool ChangeContentType(ContentType ct)
        {
            // ищем занят ли тип контента, исключая собственный тип
            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t1.Name == ct.Name && t1.Id != ct.Id && t2.Name == "ContentType"
                    select t1;

            // если тип не занят, то результат запроса - '0' строк
            if (q.Count() == 0)
            {
                // обновляем тип контента
                var q1 = context.Objects.FirstOrDefault(c => c.Id == ct.Id);
                q1.Name = ct.Name;

                // выборка прежней группы типа контента
                var q2 = from t1 in context.Objects
                         join t2 in context.ObjectTypes on t1.Type equals t2.Id
                         where t1.Name == ct.Id.ToString() && t2.Name == "ContentGroupContentTypes"
                         select t1;

                // обновляем группу типа контента
                q2.FirstOrDefault().ParentId = ct.contentGroup.Id;
                
                // выборка разрешений
                var q3 = from t1 in context.Properties
                         join t2 in context.PropTypes on t1.PropId equals t2.Id
                         where t1.ObjectId == ct.Id && t2.Name == "JSON obj"
                         select t1;

                // ***
                // перед сохранением свойств типа контента необходимо внести изменения в роли
                //

                // свойства
                var val = q3.FirstOrDefault().Value;

                // экземпляр типа контента
                List<Permission> perms = new List<Permission>();

                // десерелизация старых свойств
                var props = json.Deserialize(val, perms.GetType());

                // наполняем
                perms = (List<Permission>)props;

                var done = false;

                // если контроллеры равны, значит меняются только экшены
                if (perms[0].Name == ct.Actions[0].Name)
                {
                    done = GetUpdateAllRoles(ct.Actions, "");
                }
                else
                {
                    // иначе удаляем данный контроллер из всех ролей
                    done = GetUpdateAllRoles(ct.Actions, perms[0].Name);
                }

                // если все норм
                if (done)
                {
                    // обновляем разрешения (включают в себя Actions - там же и контроллер в 0 элементе)
                    q3.FirstOrDefault().Value = json.Serialize(ct.Actions);

                    // сохраняем изменения
                    context.SaveChanges();

                    return true;
                }

                return false;
            }

            return false;
        }
    
        // удаление типа контента
        public bool DeleteContentType(List<ContentType> ct)
        {
            // прогоняем весь список
            foreach (var i in ct)
            {
                // получаем все свойства и данные контента
                var contentType = ContentTypeList(i.Id, "");

                // обновление всех ролей, содержащих разрешения данного типа контента
                if(GetUpdateAllRoles(contentType[0].Actions, contentType[0].Actions[0].Name))
                {
                    // удаление типа контента из группы
                    var q = from t1 in context.Objects
                            join t2 in context.ObjectTypes on t1.Type equals t2.Id
                            where t1.Name == i.Id.ToString() && t2.Name == "ContentGroupContentTypes"
                            select t1;

                    context.Objects.RemoveRange(q);

                    // сохраняем изменения
                    context.SaveChanges();

                    // удаление свойств типа контента
                    context.Properties.Remove(context.Properties.FirstOrDefault(p => p.ObjectId == i.Id));

                    // сохраняем изменения
                    context.SaveChanges();

                    // удаление типа контента
                    context.Objects.Remove(context.Objects.FirstOrDefault(p => p.Id == i.Id));

                    // сохраняем изменения
                    context.SaveChanges();
                }
            }

            return true;
        }

        // создание группы типа контента
        public bool CreateContentTypeGroup(string name)
        {
            // занято ли имя группы
            if (IsExistsContentGroup(name))
            {
                return false;
            }

            // создаем группу
            var group = context.Objects.Create();
            group.Name = name;
            group.Type = context.ObjectTypes.FirstOrDefault(c => c.Name == "ContentGroup").Id; // тип объекта

            // добавляем в Objects
            context.Objects.Add(group);

            // сохраняем
            context.SaveChanges();

            return true;
        }

        // изменение группы типа контента
        public bool ChangeContentTypeGroup(int id, string name)
        {
            // ищем занята ли группа контента, исключая себя
            var q = from t1 in context.Objects
                    join t2 in context.ObjectTypes on t1.Type equals t2.Id
                    where t1.Name == name && t1.Id != id && t2.Name == "ContentGroup"
                    select t1;

            // если группа не занята, то результат запроса - '0' строк
            if (q.Count() == 0)
            {
                // обновляем группу типа контента
                context.Objects.FirstOrDefault(c => c.Id == id).Name = name;

                // сохраняем изменения
                context.SaveChanges();

                return true;
            }

            return false;
        }

        // удаление группы типа контента
        public bool DeleteContentTypeGroup(List<ContentGroup> groups)
        {
            // проверяем, входят ли какие-либо типы контента в нее
            foreach (var i in groups)
            {
                if (!IsAnyContentInContentGroup(i.Id))
                {
                    return false;
                }
            }

            // прогоняем список групп контента
            foreach (var i in groups)
            {
                var q = from t1 in context.Objects where t1.Id == i.Id select t1;

                // удаляем группу
                context.Objects.RemoveRange(q);

                // сохраняем изменения
                context.SaveChanges();
            }

            return true;
        }
    }
}
