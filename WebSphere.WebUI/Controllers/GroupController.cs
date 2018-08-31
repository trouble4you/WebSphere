using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Entities;
using WebSphere.WebUI.App_Start;
using WebSphere.WebUI.Models;

namespace WebSphere.WebUI.Controllers
{
    public class GroupController : Controller
    {
        #region настройки

        // Аккаунт
        IAccount account;

        // Json
        IJSON json;

        // ContentType
        ICSContentType contenttype;

        // Log
        ILogging logging;

        public GroupController(IAccount account, IJSON json, ICSContentType contenttype, ILogging logging)
        {
            this.account = account;
            this.json = json;
            this.contenttype = contenttype;
            this.logging = logging;
        }

        #endregion

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string groups = "")
        {
            // берем список ролей из 'RoleConfig'
            return PartialView("IndexPartial", RoleConfig.GetAllRoles());
        }

        // добавление роли
        [HttpGet]
        public ActionResult Add()
        {
            // ActionResult для ajax BeginForm
            ViewBag.ActRes = "Add";

            // модель
            CreateChangeRoleViewModel model = new CreateChangeRoleViewModel();

            // список групп типов контента
            List<ContentGroup> cgList = contenttype.ContentGroupList("");

            // закидываем список групп типов контента
            ViewBag.cgList = cgList;

            // список типов контента
            List<ContentType> ctList = contenttype.ContentTypeList(null, "");

            // если список типов контента пуст, то невозможно создать роль
            ViewBag.Empty_ctList = ctList.Count() > 0 ? 1 : 0;

            // перебираем 'ctLict', нам нужны только 'Selected' экшены
            foreach (var i in ctList)
            {
                foreach (var j in i.Actions.ToList())
                {
                    // если Selected = false, то удаляем
                    if (!j.Selected)
                    {
                        i.Actions.Remove(j);
                    }
                }
            }

            // перебираем 'ctLict', теперь все 'Selected' ставим в false
            foreach (var i in ctList)
            {
                foreach (var j in i.Actions)
                {
                    j.Selected = false;
                }

                // вовращаем 'Selected = true' в нулевой позиции - там контроллер
                i.Actions[0].Selected = true;
            }

            // наполняем модель
            model.ContentTypes = ctList;

            return PartialView("CreateChangeRole", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(CreateChangeRoleViewModel model)
        {
            // ActionResult для ajax BeginForm
            ViewBag.ActRes = "Add";

            // список групп типов контента
            List<ContentGroup> cgList = contenttype.ContentGroupList("");

            // закидываем список групп типов контента
            ViewBag.cgList = cgList;

            // если список типов контента пуст, то невозможно создать роль (он уже не пуст)
            ViewBag.Empty_ctList = 1;

            // валидация модели
            if (ModelState.IsValid)
            {
                int selected = 0;

                // проверка списка экшенов (мин. 1 должен быть 'Selected', исключая контроллер)
                foreach (var i in model.ContentTypes)
                {
                    // отключаем 'Selected' у контроллера
                    i.Actions[0].Selected = false;

                    // остальные экшены
                    foreach (var j in i.Actions)
                    {
                        if (j.Selected)
                        {
                            selected = 1;
                        }
                    }

                    // включаем 'Selected' у контроллера
                    i.Actions[0].Selected = true;
                }

                // если выбрано хотя бы одно разрешение
                if (selected == 1)
                {
                    // создание новой группы пользователей
                    if (account.CreateRole(model.Name, model.ContentTypes))
                    {
                        // обновляем хранилище ролей
                        RoleConfig.RepositoryRoles();

                        // лог
                        logging.Logged(
                              "Info"
                            , "Пользователь '" + User.Identity.Name + "' добавил роль: " + model.Name.ToString()
                            , this.GetType().Namespace
                            , this.GetType().Name
                        );

                        return Json(new { result = "Redirect", url = Url.Action("Group", "System") });
                    }

                    ModelState.AddModelError("", "Эта группа уже используется");
                }
                else
                {
                    ModelState.AddModelError("", "У группы должно быть хотя бы одно разрешение.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }

            return PartialView("CreateChangeRole", model);
        }

        // изменение роли
        [HttpGet]
        public ActionResult Change(string obj)
        {
            // ActionResult для ajax BeginForm
            ViewBag.ActRes = "Change";

            // модель
            CreateChangeRoleViewModel model = new CreateChangeRoleViewModel();

            // список групп типов контента
            List<ContentGroup> cgList = contenttype.ContentGroupList("");

            // если список типов контента пуст, то невозможно создать роль
            ViewBag.Empty_cgList = cgList.Count() > 0 ? 1 : 0;

            // закидываем список групп типов контента
            ViewBag.cgList = cgList;

            // список типов контента
            List<ContentType> ctList = contenttype.ContentTypeList(null, "");

            // если список типов контента пуст, то невозможно создать роль
            ViewBag.Empty_ctList = ctList.Count() > 0 ? 1 : 0;

            // получаем данные роли 'obj' из списка ролей 'RoleConfig.Roles'
            var role = account.GetRoleData(obj, RoleConfig.GetAllRoles());

            // перебираем 'ctLict', нам нужны только 'Selected' экшены
            foreach (var i in ctList)
            {
                foreach (var j in i.Actions.ToList())
                {
                    // если Selected = false, то удаляем
                    if (!j.Selected)
                    {
                        i.Actions.Remove(j);
                    }
                }
            }

            // перебираем 'ctLict', теперь все 'Selected' ставим в false
            // кроме 'obj'
            foreach (var i in ctList)
            {
                // список разрешений в 'ctList' по 'i'
                foreach (var j in i.Actions)
                {
                    j.Selected = false; // по-умолчанию

                    // список разрешений 'role'
                    foreach (var f in role.Permissions)
                    {
                        // ищем контроллер
                        if (i.Actions[0].Name == f.Name)
                        {
                            // разрешения роли
                            foreach (var g in f.Permission)
                            {
                                // если экшены совпали
                                if (j.Name == g.Name)
                                {
                                    // если 'Selected', то и тип контента 'Selected'
                                    if (g.Selected)
                                    {
                                        i.Selected = true;
                                    }

                                    j.Selected = g.Selected;
                                }

                            }

                            break; // на  след. итерацию, т.к. контроллер может быть только один
                        }
                    }
                }

                // вовращаем 'Selected = true' в нулевой позиции - там контроллер
                i.Actions[0].Selected = true;
            }

            // наполняем модель
            model.Id = role.Id;
            model.Name = role.Name;
            model.ContentTypes = ctList;

            return PartialView("CreateChangeRole", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Change(CreateChangeRoleViewModel model)
        {
            // ActionResult для ajax BeginForm
            ViewBag.ActRes = "Change";

            // список групп типов контента
            List<ContentGroup> cgList = contenttype.ContentGroupList("");

            // закидываем список групп типов контента
            ViewBag.cgList = cgList;

            // если список типов контента пуст, то невозможно создать роль (он уже не пуст)
            ViewBag.Empty_ctList = 1;

            // валидация модели
            if (ModelState.IsValid)
            {
                int selected = 0;

                // проверка списка экшенов (мин. 1 должен быть 'Selected', исключая контроллер)
                foreach (var i in model.ContentTypes)
                {
                    // отключаем 'Selected' у контроллера
                    i.Actions[0].Selected = false;

                    // остальные экшены
                    foreach (var j in i.Actions)
                    {
                        if (j.Selected)
                        {
                            selected = 1;
                        }
                    }

                    // включаем 'Selected' у контроллера
                    i.Actions[0].Selected = true;
                }

                // если выбрано хотя бы одно разрешение
                if (selected == 1)
                {
                    // изменение группы пользователей
                    if (account.ChangeRole(model.Id, model.Name, model.ContentTypes))
                    {
                        // обновляем хранилище ролей
                        RoleConfig.RepositoryRoles();

                        // лог
                        logging.Logged(
                              "Info"
                            , "Пользователь '" + User.Identity.Name + "' изменил роль: " + model.Name.ToString()
                            , this.GetType().Namespace
                            , this.GetType().Name
                        );

                        return Json(new { result = "Redirect", url = Url.Action("Group", "System") });
                    }

                    ModelState.AddModelError("", "Эта группа уже используется");
                }
                else
                {
                    ModelState.AddModelError("", "У группы должно быть хотя бы одно разрешение.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }

            return PartialView("CreateChangeRole", model);
        }

        // удаление роли
        [HttpGet]
        public ActionResult Delete(string json_string)
        {
            // экземпляр модели
            DeleteRoleViewModel model = new DeleteRoleViewModel();

            // объект из списка ролей для десерелизации
            List<Role> roles = new List<Role>();

            // десерелизация json_string
            var props = json.Deserialize(json_string, roles.GetType());

            // наполняем roles списком ролей с их свойствами
            roles = (List<Role>)props;

            // записываем список ролей в модель
            model.roles = roles;

            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(DeleteRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                // удаление роли
                if (account.DeleteRole(model.roles))
                {
                    // обновляем хранилище ролей
                    RoleConfig.RepositoryRoles();

                    // лог
                    logging.Logged(
                          "Info"
                        , "Пользователь '" + User.Identity.Name + "' удалил роль(и): " + string.Join(", ", model.roles.Select(x => x.Name.ToString()).ToArray())
                        , this.GetType().Namespace
                        , this.GetType().Name
                    );

                    return Json(new { result = "Redirect", url = Url.Action("Group", "System") });
                }
                else
                {
                    ModelState.AddModelError("", "Не удалось удалить объекты");
                }
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }

            return PartialView(model);
        }
    }
}