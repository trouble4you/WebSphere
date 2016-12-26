using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebSphere.Domain.Entities;

namespace WebSphere.WebUI.Helpers
{
    public class ControllerActionList
    {
        public static List<ControllerAction> GetList()
        {
            // экзепляр списка 'controlleractionlist'
            List<ControllerAction> controlleractionlist = new List<ControllerAction>();

            // по нашему проекту 'WebSphere.WebUI' достаем все котроллеры и их экшены
            Assembly asm = Assembly.GetAssembly(typeof(WebSphere.WebUI.MvcApplication));

            var list = asm.GetTypes()
            .Where(type => typeof(System.Web.Mvc.Controller).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
            .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
            .Select(x => new { Controller = x.DeclaringType.Name, Action = x.Name, ReturnType = x.ReturnType.Name, Attributes = String.Join(",", x.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", ""))) })
            .OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();

            foreach(var i in list)
            {
                // вырезаем часть названия - Controller от 'i' контроллера
                var iController = i.Controller.Substring(0, i.Controller.Length - 10);

                // не добавляем контроллер, если он уже есть в списке
                List<ControllerAction> controllerIsExists = controlleractionlist.FindAll(c => c.Controller.StartsWith(iController));

                // не пропускаем контроллер ContentType, т.к. его не нужно отображать в списке
                if (controllerIsExists.Count() <= 0 && iController != "ContentType")
                {
                    // экземпляр контроллера с экшенами
                    ControllerAction controlleraction = new ControllerAction();

                    // контроллер
                    controlleraction.Controller = iController;

                    // экземпляр списка с экшенами
                    List<string> actions = new List<string>();

                    // вытаскиваем все экшены по данному контроллеру
                    foreach (var j in list)
                    {
                        // вырезаем часть названия - Controller от 'j' контроллера 
                        var jcontroller = j.Controller.Substring(0, j.Controller.Length - 10);

                        if (jcontroller == iController)
                        {
                            // не добавляем экшен, если он уже есть в списке
                            List<string> actionIsExists = actions.FindAll(c => c.StartsWith(j.Action));

                            if (actionIsExists.Count() <= 0)
                            {
                                actions.Add(j.Action);
                            }
                        }
                    }

                    // закидываем список экшенов в данный контроллер
                    controlleraction.Actions = actions;

                    // закидываем 'controlleraction' в список 'controlleractionlist'
                    controlleractionlist.Add(controlleraction);
                }
            }

            return controlleractionlist;
        } 
    }
}

#region еще варианты...
//private IEnumerable<Type> GetControllers()
//{
//    return from t in typeof(Controller).Assembly.GetTypes()
//           where t.IsAbstract == false
//           where typeof(Controller).IsAssignableFrom(t)
//           where t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
//           select t;
//}

//Assembly asm = Assembly.GetExecutingAssembly();

//var ff = asm.GetTypes()
//        .Where(type => typeof(Controller).IsAssignableFrom(type)) //filter controllers
//        .SelectMany(type => type.GetMethods())
//        .Where(method => method.IsPublic && !method.IsDefined(typeof(NonActionAttribute)));
//var pp = ff.ToList();

//var names = ff.FirstOrDefault();


//List<string> controllers = new List<string>();
//var str = "";

//foreach (var i in pp)
//{

//    var pppp = i.Name;

//    str += i.ReflectedType.Name + "; ";
//    var method = i.GetRuntimeBaseDefinition().Name;
//    var ttt = i.ReflectedType.Name;
//}



//ViewBag.Str = str;
#endregion