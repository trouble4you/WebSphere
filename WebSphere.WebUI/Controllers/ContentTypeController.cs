using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Entities;
using WebSphere.WebUI.App_Start;
using WebSphere.WebUI.Helpers;
using WebSphere.WebUI.Models;

namespace WebSphere.WebUI.Controllers
{
    public class ContentTypeController : Controller
    {
        #region настройки

        // ContentType
        ICSContentType contenttype;

        // Json
        IJSON json;

        // Log
        ILogging logging;

        public ContentTypeController(ICSContentType contenttype, IJSON json, ILogging logging)
        {
            this.contenttype = contenttype;
            this.json = json;
            this.logging = logging;
        }

        #endregion

        // список типов контента
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string contents = "")
        {
            return PartialView("IndexPartial", contenttype.ContentTypeList(null, ""));
        }

        // создание типа контента
        [HttpGet]
        public ActionResult Create()
        {
            // модель
            CreateContentTypeViewModel model = new CreateContentTypeViewModel();

            // список контроллеров и их экшенов
            List<ControllerAction> caList = ControllerActionList.GetList();

            // данные Всех типов контента
            List<ContentType> ctAll = new List<ContentType>();
            ctAll = contenttype.ContentTypeList(null, "");

            // список контроллеров и их экшенов из сборки 'WebSphere.WebUI'
            foreach (var i in caList.ToList())
            {
                // список контроллеров и их экшенов из БД
                foreach (var j in ctAll)
                {
                    // удаляем из 'caList' занятые контроллеры с экшенами
                    //if (i.Controller == j.Controller && j.Actions[0].Selected)
                    //{
                    //    caList.Remove(i);
                    //}
                }
            }

            // проверка 'caList' есть ли свободные контроллеры
            // невозможно создать новый тип контента без свободного контроллера
            if (caList.Count() != 0)
            {
                ViewBag.EmptyCaListKey = 1; // список 'caList' не пуст

                // список групп типов контента
                List<ContentGroup> contentgrouplist = contenttype.GetContentTypeGroups(null);
                ViewBag.ContentGroupList = contentgrouplist;

                // закидываем пересобранный список во 'ViewBag'
                ViewBag.caList = caList;

                // сериализуем 'caList' для 'ChangePartial'
                model.caList = json.Serialize(caList);

                // экшены
                List<Permission> actions = new List<Permission>();

                // прогоняем все экшены в 'caList[0]', 
                // т.к. по-умолчанию в списке контроллеров именно тот, кот. в [0], то и экшены соответсвующие
                foreach (var i in caList[0].Actions)
                {
                    // экшен
                    Permission action = new Permission();

                    action.Name = i;
                    action.Selected = false; // по-умолчанию

                    // добавляем в экшены
                    actions.Add(action);
                }

                // добавляем в модель
                model.Actions = actions;

                return PartialView(model);
            }

            ViewBag.EmptyCaListKey = null; // список 'caList' пуст

            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateContentTypeViewModel model)
        {
            // список контроллеров и их экшенов
            List<ControllerAction> caList = new List<ControllerAction>();

            // десериализуем 'model.caList'
            var new_caList = json.Deserialize(model.caList, caList.GetType());

            // наполняем список
            caList = (List<ControllerAction>)new_caList;
            ViewBag.caList = caList;

            // список групп типов контента
            List<ContentGroup> contentgrouplist = contenttype.GetContentTypeGroups(null);
            ViewBag.ContentGroupList = contentgrouplist;

            ViewBag.EmptyCaListKey = 1; // список 'caList' не пуст

            // валидация модели
            if (ModelState.IsValid)
            {
                int selected = 0;
                int title = 0;

                // проверка списка экшенов на Selected (должен быть хотя бы один и его название)
                foreach (var i in model.Actions)
                {
                    if (i.Selected)
                    {
                        selected = 1;
                        title = 0; // сброс флага названия, т.к. найден отмеченный экшен

                        // пусто / null ли название
                        if (!string.IsNullOrEmpty(i.Title))
                        {
                            title = 1;
                        }
                        else
                        {
                            break; // выход из цикла, если нет названия для текущего экшена
                        }
                    }
                }

                // если выбран хотя бы один экшен и есть его название
                if (selected == 1 && title == 1)
                {
                    // восстанавливаем нулевую запись экшена - контроллер
                    Permission action = new Permission()
                    {
                        Name = model.Controller,
                        Title = "",
                        Selected = true
                    };

                    // помещаем наш контроллер в начало 'model.Actions'
                    model.Actions.Insert(0, action);

                    // наполняем объект данными
                    ContentType ct = new ContentType()
                    {
                        Name = model.Name,
                        Controller = model.Controller,
                        Actions = model.Actions,
                        contentGroup = model.contentGroup
                    };

                    // создание нового типа контента
                    if (contenttype.CreateContentType(ct))
                    {
                        // лог
                        logging.Logged(
                              "Info"
                            , "Пользователь " + User.Identity.Name + " добавил тип контента: '" + model.Name + "'"
                            , this.GetType().Namespace
                            , this.GetType().Name
                        );

                        return Json(new { result = "Redirect", url = Url.Action("Index", "ContentType") });
                    }

                    // удаляем из 'model.Actions' нулевой элемент - контроллер,
                    // чтобы не нарушить структуру при возвращении 'CreatePartial'
                    model.Actions.RemoveAt(0);

                    ModelState.AddModelError("", "Этот тип контента уже используется");
                }
                else
                {
                    ModelState.AddModelError("", "Укажите хотя бы один экшен. Для указанных экшенов название обязательно.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }

            return PartialView(model);
        }

        // изменение типа контента
        [HttpGet]
        public ActionResult Change(string obj)
        {
            // модель
            ChangeContentTypeViewModel model = new ChangeContentTypeViewModel();

            // список контроллеров и их экшенов
            List<ControllerAction> caList = ControllerActionList.GetList();

            // данные Всех типов контента
            List<ContentType> ctAll = new List<ContentType>();
            ctAll = contenttype.ContentTypeList(null, "");

            // данные типа контента по 'Name'
            List<ContentType> ct = new List<ContentType>();
            ct = contenttype.ContentTypeList(null, obj);

            // список групп типов контента
            List<ContentGroup> contentgrouplist = contenttype.GetContentTypeGroups(null);
            ViewBag.ContentGroupList = contentgrouplist;

            // наполняем модель
            model.Id = ct[0].Id;
            model.Name = ct[0].Name;
            model.Controller = ct[0].Controller;
            model.contentGroup = ct[0].contentGroup;

            // список контроллеров и их экшенов из сборки 'WebSphere.WebUI'
            foreach (var i in caList.ToList())
            {
                // список контроллеров и их экшенов из БД
                foreach (var j in ctAll)
                {
                    // удаляем из 'caList' занятые контроллеры с экшенами, кроме текущего контроллера 'ct[0].Controller'
                    if (i.Controller == j.Controller && i.Controller != ct[0].Controller && j.Actions[0].Selected)
                    {
                        caList.Remove(i);
                    }
                }
            }

            // закидываем пересобранный список во 'ViewBag'
            ViewBag.caList = caList;

            // сериализуем 'caList' для 'ChangePartial'
            model.caList = json.Serialize(caList);

            // удаляем контроллер из списка экшенов, кот. в 'ct[0]', оставляем только экшены
            ct[0].Actions.RemoveAt(0);

            // прогоняем весь список экшенов по текущему контроллеру в 'ct[0]' по 'caList'
            foreach (var i in caList)
            {
                // поиск контроллера в списке
                if (i.Controller == ct[0].Controller)
                {
                    // экшены
                    List<Permission> actions = new List<Permission>();

                    // прогоняем все экшены
                    foreach (var j in i.Actions)
                    {
                        // экшен
                        Permission action = new Permission();

                        action.Name = j;
                        action.Selected = false; // по-умолчанию
                        action.Title = "";

                        // ищем совпадения экшенов по 'ContentType', кот. в 'ct[0]' для того, чтобы
                        // указать 'Selected' в общий список экшенов
                        // данный подход необходим в случае, если в контроллер добавились новые экшены и 
                        // мы должны видеть их в списке
                        foreach (var f in ct[0].Actions)
                        {
                            if (j == f.Name)
                            {
                                action.Selected = f.Selected;
                                action.Title = f.Title;
                            }
                        }

                        // добавляем в экшены
                        actions.Add(action);
                    }

                    // добавляем в модель
                    model.Actions = actions;
                }
            }

            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Change(ChangeContentTypeViewModel model)
        {
            // список контроллеров и их экшенов
            List<ControllerAction> caList = new List<ControllerAction>();

            // десериализуем 'model.caList'
            var new_caList = json.Deserialize(model.caList, caList.GetType());

            // наполняем список
            caList = (List<ControllerAction>)new_caList;
            ViewBag.caList = caList;

            // список групп типов контента
            List<ContentGroup> contentgrouplist = contenttype.GetContentTypeGroups(null);
            ViewBag.ContentGroupList = contentgrouplist;

            // валидация модели
            if (ModelState.IsValid)
            {
                int selected = 0;
                int title = 0;

                // проверка списка экшенов на Selected (должен быть хотя бы один и его название)
                foreach (var i in model.Actions)
                {
                    if (i.Selected)
                    {
                        selected = 1;
                        title = 0; // сброс флага названия, т.к. найден отмеченный экшен

                        // пусто / null ли название
                        if (!string.IsNullOrEmpty(i.Title))
                        {
                            title = 1;
                        }
                        else
                        {
                            break; // выход из цикла, если нет названия для текущего экшена
                        }
                    }
                }

                // если выбран хотя бы один экшен и есть его название
                if (selected == 1 && title == 1)
                {
                    // восстанавливаем нулевую запись экшена - контроллер
                    Permission action = new Permission()
                    {
                        Name = model.Controller,
                        Title = "",
                        Selected = true
                    };

                    // помещаем наш контроллер в начало 'model.Actions'
                    model.Actions.Insert(0, action);

                    // наполняем объект данными
                    ContentType ct = new ContentType()
                    {
                        Id = model.Id,
                        Name = model.Name,
                        Controller = model.Controller,
                        Actions = model.Actions,
                        contentGroup = model.contentGroup
                    };

                    // обновление данных типа контента
                    if (contenttype.ChangeContentType(ct))
                    {
                        // обновляем хранилище ролей
                        RoleConfig.RepositoryRoles();

                        // лог
                        logging.Logged(
                              "Info"
                            , "Пользователь " + User.Identity.Name + " изменил тип контента: '" + model.Name + "'"
                            , this.GetType().Namespace
                            , this.GetType().Name
                        );

                        return Json(new { result = "Redirect", url = Url.Action("Index", "ContentType") });
                    }

                    // удаляем из 'model.Actions' нулевой элемент - контроллер,
                    // чтобы не нарушить структуру при возвращении 'ChangePartial'
                    model.Actions.RemoveAt(0);

                    ModelState.AddModelError("", "Этот тип контента уже используется");
                }
                else
                {
                    ModelState.AddModelError("", "Укажите хотя бы один экшен. Для указанных экшенов название обязательно.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }

            return PartialView(model);
        }

        // удаление типа контента
        [HttpGet]
        public ActionResult Delete(string json_string)
        {
            // экземпляр модели
            DeleteContentTypeViewModel model = new DeleteContentTypeViewModel();

            // объект для десерелизации
            List<ContentType> contentTypes = new List<ContentType>();

            // десерелизация json_string
            var props = json.Deserialize(json_string, contentTypes.GetType());

            // наполняем
            contentTypes = (List<ContentType>)props;

            // теперь записываем в модель
            model.contenttypes = contentTypes;

            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(DeleteContentTypeViewModel model)
        {
            // валидация модели
            if (ModelState.IsValid)
            {
                // удаление
                if (contenttype.DeleteContentType(model.contenttypes))
                {
                    // обновляем хранилище ролей
                    RoleConfig.RepositoryRoles();

                    // лог
                    logging.Logged(
                          "Info"
                        , "Пользователь " + User.Identity.Name + " удалил тип(ы) контента: '" + string.Join(", ", model.contenttypes.Select(x => x.Name.ToString()).ToArray())
                        , this.GetType().Namespace
                        , this.GetType().Name
                    );

                    return Json(new { result = "Redirect", url = Url.Action("Index", "ContentType") });
                }
            }

            ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");

            return PartialView(model);
        }

        // смена списка экшенов при смене контроллера
        [HttpPost]
        public ActionResult ActionsList(int? contentTypeId, string controllerName)
        {
            // модель
            ActionsListViewModel model = new ActionsListViewModel();

            // список контроллеров и их экшенов
            List<ControllerAction> caList = ControllerActionList.GetList();

            // данные типа контента
            List<ContentType> ct = new List<ContentType>();
            ct = contenttype.ContentTypeList(contentTypeId, "");

            // прогоняем весь список
            foreach (var i in caList)
            {
                // поиск контроллера в списке
                if (i.Controller == controllerName)
                {
                    // экшены
                    List<Permission> actions = new List<Permission>();

                    // прогоняем все экшены
                    foreach (var j in i.Actions)
                    {
                        // экшен
                        Permission action = new Permission();

                        action.Name = j;
                        action.Selected = false; // по-умолчанию
                        action.Title = "";

                        // может быть ситуация, что, к примеру, еще нет ни одного типа контента...
                        // поэтому делаем следующее, если список типов контента не пуст
                        if (ct.Count() != 0)
                        {
                            // если 'contentTypeId' = null, то условие ниже никогда не выполнится, т.к.
                            // в списке 'caList' не будет контроллеров, кот. в 'ct[0].Controller', потому что в 
                            // 'caList' только свободные контроллеры

                            // условие ниже только для изменения, а не на создание типа контента

                            // ищем совпадения экшенов по 'ContentType', кот. в 'ct[0]' для того, чтобы
                            // указать 'Selected' в общий список экшенов
                            // данный подход необходим в случае, если в контроллер добавились новые экшены и 
                            // мы должны видеть их в списке
                            // если мы возращаемся к предыдущему контроллеру, то восстанавливаем все отмеченные экшены
                            if (controllerName == ct[0].Controller)
                            {
                                foreach (var f in ct[0].Actions)
                                {
                                    if (j == f.Name)
                                    {
                                        action.Selected = f.Selected;
                                        action.Title = f.Title;
                                    }
                                }
                            }
                        }

                        // добавляем в экшены
                        actions.Add(action);
                    }

                    // добавляем в модель
                    model.Actions = actions;
                }
            }

            return PartialView(model);
        }

        // список групп типов контента
        [HttpGet]
        public ActionResult Group()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Group(string groups = "")
        {
            return PartialView("GroupPartial", contenttype.ContentGroupList(""));
        }

        // создание группы типа контента
        [HttpGet]
        public ActionResult CreateGroup()
        {
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateGroup(CreateContentTypeGroupViewModel model)
        {
            // валидация модели
            if (ModelState.IsValid)
            {
                // создание новой группы контента
                if (contenttype.CreateContentTypeGroup(model.Name))
                {
                    // лог
                    logging.Logged(
                          "Info"
                        , "Пользователь " + User.Identity.Name + " добавил группу типа контента: '" + model.Name + "'"
                        , this.GetType().Namespace
                        , this.GetType().Name
                    );

                    return Json(new { result = "Redirect", url = Url.Action("Group", "ContentType") });
                }

                ModelState.AddModelError("", "Эта группа уже используется");
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }

            return PartialView(model);
        }

        // изменение группы типа контента
        [HttpGet]
        public ActionResult ChangeGroup(string obj)
        {
            // модель
            ChangeContentTypeGroupViewModel model = new ChangeContentTypeGroupViewModel();

            // группа типа контента
            List<ContentGroup> contentGroup = new List<ContentGroup>();
            contentGroup = contenttype.ContentGroupList(obj);

            // наполняем модель
            model.Id = contentGroup[0].Id; // Id
            model.Name = contentGroup[0].Name; // Name

            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeGroup(ChangeContentTypeGroupViewModel model)
        {
            // валидация модели
            if (ModelState.IsValid)
            {
                // изменение группы контента
                if (contenttype.ChangeContentTypeGroup(model.Id, model.Name))
                {
                    // лог
                    logging.Logged(
                          "Info"
                        , "Пользователь " + User.Identity.Name + " изменил группу типа контента: '" + model.Name + "'"
                        , this.GetType().Namespace
                        , this.GetType().Name
                    );

                    return Json(new { result = "Redirect", url = Url.Action("Group", "ContentType") });
                }

                ModelState.AddModelError("", "Эта группа уже используется");
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }

            return PartialView(model);
        }

        // удаление группы типа контента
        [HttpGet]
        public ActionResult DeleteGroup(string json_string)
        {
            // экземпляр модели
            DeleteContentTypeGroupViewModel model = new DeleteContentTypeGroupViewModel();

            // объект для десерелизации
            List<ContentGroup> contentgroups = new List<ContentGroup>();

            // десерелизация json_string
            var props = json.Deserialize(json_string, contentgroups.GetType());

            // наполняем
            contentgroups = (List<ContentGroup>)props;

            // теперь записываем в модель
            model.contentgroups = contentgroups;

            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGroup(DeleteContentTypeGroupViewModel model)
        {
            // валидация модели
            if (ModelState.IsValid)
            {
                // удаление
                if (contenttype.DeleteContentTypeGroup(model.contentgroups))
                {
                    // лог
                    logging.Logged(
                          "Info"
                        , "Пользователь " + User.Identity.Name + " удалил группу(ы) типа(ов) контента: " + string.Join(", ", model.contentgroups.Select(x => x.Name.ToString()).ToArray())
                        , this.GetType().Namespace
                        , this.GetType().Name
                    );

                    return Json(new { result = "Redirect", url = Url.Action("Group", "ContentType") });
                }
                ModelState.AddModelError("", "Группа(ы) используется контентом(и). Для начала освободите группу(ы) от всех типов контента.");
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }

            return PartialView(model);
        }
    }
}