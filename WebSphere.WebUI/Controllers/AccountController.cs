using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;
using WebSphere.WebUI.App_Start;
using WebSphere.WebUI.Infrastructure;
using WebSphere.WebUI.Models;

namespace WebSphere.WebUI.Controllers
{
    public class AccountController : Controller
    {
        #region настройки

        // Аккаунт
        IAccount account;

        // Json
        IJSON json;

        // Log
        ILogging logging;

        public AccountController(IAccount account, IJSON json, ILogging logging)
        {
            this.account = account;
            this.json = json;
            this.logging = logging;
        }

        #endregion

        // список пользователей
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string users = "")
        {
            return PartialView("IndexPartial", account.UsersList(""));
        }

        // вход в систему
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            // если пользователь аутентифицирован, то не показываем ему страницу входа
            if (User.Identity.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (account.Authenticate(model.UserName, model.Password))
                {
                    // данные потльзователя
                    List<User> user = new List<User>();
                    user = account.UsersList(model.UserName);

                    // активен ли пользователь
                    if (user[0].IsActive == 1)
                    {
                        FormsAuthentication.SetAuthCookie(model.UserName, false);

                        // лог
                        logging.Logged(
                              "Info"
                            , "Пользователь '" + model.UserName + "' вошел в систему"
                            , this.GetType().Namespace
                            , this.GetType().Name
                        );

                        return Json(new { result = "Redirect", url = Url.Action("Index", "Home") });
                    }
                    else
                    {
                        // лог
                        logging.Logged(
                              "Error"
                            , "Отказано в доступе: пользователь '" + model.UserName + "' неактивен"
                            , this.GetType().Namespace
                            , this.GetType().Name
                        );

                        ModelState.AddModelError("", "Отказано в доступе");
                    }
                }
                else
                {
                    //// лог
                    //logging.Logged(
                    //      "Error"
                    //    , "Ошибка аутентификации: '" + model.UserName + "' - неправильный логин или пароль"
                    //    , this.GetType().Namespace
                    //    , this.GetType().Name
                    //);

                    ModelState.AddModelError("", "Неправильный логин или пароль");
                }
            }
            //else
            //{
            //    // лог
            //    logging.Logged(
            //            "Error"
            //        , "Ошибка аутентификации: '" + model.UserName + "' - неправильный логин или пароль"
            //        , this.GetType().Namespace
            //        , this.GetType().Name
            //    );

            //    ModelState.AddModelError("", "Ошибка аутентификации");
            //}

            return PartialView("LoginPartial", model);
        }

        // регистрация аккаунта
        [HttpGet]
        public ActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();

            // добавляем в модель
            model.Roles = RoleConfig.GetAllRoles();

            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // регистрация пользователя ч/з сущность User
                User user = new User()
                {
                    UserName = model.UserName,
                    Password = model.Password,
                    Name = model.Name,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    Email = model.Email,
                    IsActive = model.IsActive ? 1 : 0,
                    IsSuperuser = model.Superuser ? 1 : 0,
                    Roles = model.Roles
                };

                // получилось ли зарегистрировать нового пользователя
                if (account.CreateUser(user))
                {
                    // лог
                    logging.Logged(
                          "Info"
                        , "Пользователь '" + User.Identity.Name + "' добавил нового пользователя: '" + model.UserName + "'"
                        , this.GetType().Namespace
                        , this.GetType().Name
                    );

                    return Json(new { result = "Redirect", url = Url.Action("User", "System") });
                }
                else
                {
                    ModelState.AddModelError("", "Этот пользователь уже зарегистрирован");
                }
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }

            return PartialView(model);
        }

        // изменение аккаунта
        [HttpGet]
        public ActionResult Change(string obj)
        {
            ChangeViewModel model = new ChangeViewModel();

            // данные пользователя
            List<User> user = new List<User>();
            user = account.UsersList(obj);

            // список всех ролей
            List<Role> allRoles = RoleConfig.GetAllRoles();

            // наполняем модель
            model.Id = user[0].Id;
            model.UserName = user[0].UserName;
            model.Name = user[0].Name;
            model.LastName = user[0].LastName;
            model.MiddleName = user[0].MiddleName;
            model.Email = user[0].Email;
            model.Roles = null;
            model.IsActive = user[0].IsActive == 1 ? true : false;
            model.Superuser = user[0].IsSuperuser == 1 ? true : false;

            // отмечаем роли, если не Суперпользователь и если есть роли
            if (!model.Superuser && allRoles.Count() != 0)
            {
                // роли
                List<Role> roles = new List<Role>();

                for (var i = 0; i < allRoles.Count(); i++)
                {
                    // роль
                    Role role = new Role()
                    {
                        Id = allRoles[i].Id,
                        Name = allRoles[i].Name,
                        Type = allRoles[i].Type,
                        Permissions = allRoles[i].Permissions,
                        Selected = false
                    };

                    // ищем роль в ролях пользователя
                    foreach (var j in user[0].Roles)
                    {
                        // если у пользователя эта роль есть, то отмечаем ее
                        if (allRoles[i].Name == j.Name)
                        {
                            role.Selected = true;
                            break;
                        }
                    }

                    roles.Add(role); // в общий список ролей
                }

                // перезаписываем роли в модель
                model.Roles = roles;
            }

            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Change(ChangeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // наполняем объект данными
                User user = new User()
                {
                    Id = model.Id,
                    UserName = model.UserName,
                    Password = model.Password,
                    Name = model.Name,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    Email = model.Email,
                    IsActive = model.IsActive ? 1 : 0,
                    IsSuperuser = model.Superuser ? 1 : 0,
                    Roles = model.Roles
                };

                // обновление данных пользователя
                if (account.ChangeUser(user))
                {
                    // лог
                    logging.Logged(
                          "Info"
                        , "Пользователь '" + User.Identity.Name + "' изменил данные пользователя: '" + model.UserName + "'"
                        , this.GetType().Namespace
                        , this.GetType().Name
                    );

                    return Json(new { result = "Redirect", url = Url.Action("User", "System") });
                }
                else
                {
                    ModelState.AddModelError("", "Этот пользователь уже зарегистрирован");
                }
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }
            
            return PartialView(model);
        }

        // удаление аккаунта
        [HttpGet]
        public ActionResult Delete(string json_string)
        {
            // экземпляр модели
            DeleteViewModel model = new DeleteViewModel();

            // объект из списка пользователей для десерелизации
            List<User> users = new List<User>();

            // десерелизация json_string
            var props = json.Deserialize(json_string, users.GetType());

            // наполняем users списком пользователей с их свойствами
            users = (List<User>)props;

            // теперь записываем в модель
            model.users = users;

            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(DeleteViewModel model)
        {
            if (ModelState.IsValid)
            {
                // ищем, указал ли пользователь на удаление админа
                List<User> users = model.users.FindAll(u => u.UserName.StartsWith("admin"));

                // нельзя удалить пользователя 'admin'
                if (users.Count() > 0)
                {
                    ModelState.AddModelError("", "Ошибка, нельзя удалить пользователя 'admin'");
                }
                else
                {
                    // удаление пользователей
                    if (account.DeleteUser(model.users))
                    {
                        // лог
                        logging.Logged(
                              "Info"
                            , "Пользователь '" + User.Identity.Name + "' удалил пользователя(ей): " + string.Join(", ", model.users.Select(x => x.UserName.ToString()).ToArray())
                            , this.GetType().Namespace
                            , this.GetType().Name
                        );

                        return Json(new { result = "Redirect", url = Url.Action("User", "System") });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Не удалось удалить объекты");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, пожалуйста проверьте данные");
            }
            
            return PartialView(model);
        }

        // выход из системы
        [AllowAnonymous]
        public ActionResult Logout()
        {
            // если пользователь аутентифицирован, только тогда он может выйти из системы
            if (User.Identity.IsAuthenticated == true)
            {
                // лог
                logging.Logged(
                      "Info"
                    , "Пользователь '" + User.Identity.Name + "' вышел из системы"
                    , this.GetType().Namespace
                    , this.GetType().Name
                );

                FormsAuthentication.SignOut();
            }

            return RedirectToAction("Index", "Home");
        }
	}
}