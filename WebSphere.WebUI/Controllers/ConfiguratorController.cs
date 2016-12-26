using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;
using WebSphere.WebUI.Models;

namespace WebSphere.WebUI.Controllers
{
    public class ConfiguratorController : Controller
    {
        IJSTree jstree;
        ITagConfigurator tagConfigurator;
        IJSON json;
        IFileWork filework;

        public ConfiguratorController(IJSTree jstree, ITagConfigurator tagConfigurator, IJSON json, IFileWork filework)
        {
            this.jstree = jstree;
            this.tagConfigurator = tagConfigurator;
            this.json = json;
            this.filework = filework;
        }

        //
        // GET: /Configurator/
        public ActionResult Index()
        {
            ViewBag.JStreeStr = jstree.CreateJsTree(0);
            var df = tagConfigurator.GetConnectedModules();
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            return View();
        }


       
        //Выполняет подмену input'a при добавлении стандартного свойства
        [HttpPost]
        public ActionResult ChangeStandartRegExp(string typeNumber, int parentGroup, int idNode)
        {
            ViewBag.OpcServers = tagConfigurator.getOpcServersName();
            ViewBag.SelectChannel = tagConfigurator.getChannels();
            //var connectionStr = jstree.getConnectionProp(idNode);
            var OPCId = jstree.getOPCID();

            var fff = new AddStandartPropModelHelp();
            fff.selectValueStd = Convert.ToInt32(typeNumber);
            fff.ParentGroup = parentGroup;
            fff.Id = idNode;
            fff.Opc = OPCId;
            return PartialView("ChangeInputStandartPropPartial", fff);
        }
        //Выполняет подмену input'a при добавлении пользовательского свойства
        [HttpPost]
        public ActionResult ChangeRegExp(string typeNumber)
        {
            var model = new AddUserPropModel();
            model.BoolenType = false;
            model.ByteType = false;
            model.WordType = false;
            model.DWordType = false;
            model.ShortIntType = false;
            model.SmallIntType = false;
            model.LongIntType = false;
            model.FloatType = false;
            model.DoubleType = false;
            model.StringType = false;


            switch (typeNumber)
            {
                case "1":
                    {
                        model.ByteType = true;
                        break;
                    }
                case "2":
                    {
                        model.WordType = true;
                        break;
                    }
                case "3":
                    {
                        model.DWordType = true;
                        break;
                    }
                case "4":
                    {
                        model.ShortIntType = true;
                        break;
                    }
                case "5":
                    {
                        model.SmallIntType = true;
                        break;
                    }
                case "6":
                    {
                        model.LongIntType = true;
                        break;
                    }
                case "7":
                    {
                        model.FloatType = true;
                        break;
                    }
                case "8":
                    {
                        model.DoubleType = true;
                        break;
                    }
                case "9":
                    {
                        model.BoolenType = true;
                        break;
                    }
                case "10":
                    {
                        model.StringType = true;
                        break;
                    }
            }
            return PartialView("ChangeInputUserPropPartial", model);
        }

        //Проверка на существование название узла
        //[HttpGet]
        //public JsonResult CheckNameNode(string propName)
        //{
        //    var data = tagConfigurator.checkExistingNodeName(propName);
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}


        [HttpPost]
        public ActionResult DeleteProps(List<string> deletePropsArr, int id)
        {
            tagConfigurator.deleteProps(id, deletePropsArr);
            return showTabProps(id);
        }

        //[HttpPost]
        //public ActionResult DeleteProp(int nodeId, string propForDelete1)
        //{
        //    tagConfigurator.deleteProp(nodeId, propForDelete1);
        //    return showTabProps(nodeId);
        //}


        //[HttpPost]
        ////при добавлении стандартных свойств не учитывается их тип,то есть что писать в json строку "1" или 1
        //public ActionResult AddProp(string propName, string propValue, int propType, int typeNode, int nodeId)
        //{

        //    tagConfigurator.addProp(propName, propValue, propType, nodeId);
        //    if (typeNode == 1)
        //    {
        //        ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
        //        var tagProps = tagConfigurator.getTagProps(nodeId);
        //        return PartialView("TagPartial", tagProps);
        //    }
        //    if (typeNode == 2)
        //    {
        //        var RadioChannelProps = tagConfigurator.getRadioChannelProps(nodeId);
        //        return PartialView("RadioChannelPartial", RadioChannelProps);
        //    }
        //    if (typeNode == 3)
        //    {
        //        var GPRSChannelProps = tagConfigurator.getGPRSChannelProps(nodeId);
        //        return PartialView("GPRSChannelPartial", GPRSChannelProps);
        //    }
        //    if (typeNode == 4)
        //    {
        //        var ObjectProps = tagConfigurator.getObjectProps(nodeId);
        //        return PartialView("ObjectPartial", ObjectProps);
        //    }
        //    if (typeNode == 5)
        //    {
        //        var PollingGroupProps = tagConfigurator.getPollingGroupProps(nodeId);
        //        return PartialView("PollingGroupPartial", PollingGroupProps);
        //    }
        //    if (typeNode == 6)
        //    {
        //        var OPCProps = tagConfigurator.getOPCProps(nodeId);
        //        return PartialView("OPCPartial", OPCProps);
        //    }
        //    else
        //    {
        //        var noTypeNodeProps = tagConfigurator.getNoTypeNodeProps(nodeId);
        //        var userProps = tagConfigurator.getUserProps(nodeId);
        //        var standartProps = tagConfigurator.getStandartProps(nodeId);
        //        ViewBag.StandartProps = standartProps;
        //        ViewBag.UserProps = userProps;
        //        return PartialView("NoTypeNodePartial", noTypeNodeProps);
        //    }

        //}

        //переименование узла
        [HttpGet]
        public void RenameNode(int idRenameNode, string newNodeName)
        {

            var id = idRenameNode;

            int typeProp = tagConfigurator.GetPropType(id);
            jstree.renameNode(idRenameNode, newNodeName, typeProp);

        }
        //редактирование нетипизированного узла
        [HttpPost]
        public ActionResult EditNoTypesProps(NoTypesPropsHelp model)
        {
            //проблема с определением типа во вью. Везде видит инт64 как бы не выходит так тонко провалидировать пользовательские свойства
            //json строка с польз свойствами
            string specialProp = model.notypesforSave.special;
            string forSerialize = "";
            //если есть пользовательске свойства
             if (!string.IsNullOrEmpty(specialProp))
            {
                forSerialize = "{" + specialProp.Remove(0, 1) + "}";
            }

            //собираем сюда возможные ошибки
            List<ErrorUserProp> test = tagConfigurator.checkUserPropsValidity(forSerialize);
            //если польз свойства не валидны
            if (test.Count > 0)
            {
                var callBackString = json.Serialize(test);
                ViewBag.StdPropsValidityError = callBackString;
            }

            var IdForSave = model.notypesforSave.Id;
            if (ModelState.IsValid)
            {
                //даже если модель, включающая стандартные свойства валидна, смотрим есть ли ошибки в польз-х свойствах
                if (test.Count > 0)
                {
                    ViewBag.Notification = 0;
                    //перетираем строку модели в соответствии с ошибками
                }
                else
                {
                    //если валидны и польз и стандратные, то сохраняем
                    tagConfigurator.saveNoTypesProps1(model);
                    ViewBag.Notification = 1;
                }
            }
            else
            {
                ViewBag.Notification = 0;
            }
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            ViewBag.OPCservers = tagConfigurator.getOpcServersName();

            //здесь собраны станд свойства,полученные с формы (а не из БД) по результатам изменения пользователем
            //ибо если пользователь исправит  одно из невалидных значений,а другое по результатам исправлений останется невалидным,
            //то при возвращении формы с ошибками снова вернутся старые значения
            //и только когда все валидно произойдет сохранение
            Dictionary<string, dynamic> userProps=new Dictionary<string,dynamic>();
            if (!string.IsNullOrEmpty(specialProp))
            {
                userProps = tagConfigurator.getUserPropsAfterValidate(specialProp);
            }
            //var userProps = tagConfigurator.getUserProps(IdForSave);
            ViewBag.SelectChannel = tagConfigurator.getChannels();
            ViewBag.UserProps = userProps;
            var standartProps = tagConfigurator.getStandartProps(IdForSave);
            ViewBag.StandartProps = standartProps;
            //если не создать объект класса NoTypeNodeHelp, то при null значении вью дает NullPointerException
            if(model.notypesModel==null)
            {
                var stdProp = new NoTypeNodeHelp();
                model.notypesModel = stdProp;
            }
            return PartialView("NoTypeNodePartial", model);
            //return PartialView("NoTypeNodePartial");
        }

        //редактирование узла типа Object
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditObjectProps(ObjectProps model)
        {
            if (ModelState.IsValid)
            {
                var IdForSave = model.Id;
                tagConfigurator.saveObjectProps(model, IdForSave);
                ViewBag.Notification = 1;
            }
            else
            {
                ViewBag.Notification = 0;
                //ModelState.AddModelError("", "Введены некорректные данные!");
            }

            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            var userProps = tagConfigurator.getUserProps(model.Id);
            ViewBag.UserProps = userProps;
            ViewBag.SelectChannel = tagConfigurator.getChannels();
            return PartialView("ObjectPartial", model);
        }

        //редактирование узла типа Tag
        [HttpPost]
        public ActionResult EditTagProps(TagProps model)
        {
            if (ModelState.IsValid)
            {
                var IdForSave = model.Id;
                tagConfigurator.saveTagProps(model, IdForSave);
                ViewBag.Notification = 1;
            }
            else
            {
                ViewBag.Notification = 0;
            }
            var userProps = tagConfigurator.getUserProps(model.Id);
            ViewBag.UserProps = userProps;
            ViewBag.OPCservers = tagConfigurator.getOpcServersName();
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            return PartialView("TagPartial", model);
        }

        //редактирование узла типа OPC
        [HttpPost]
        public ActionResult EditOPCProps(OPCProps model)
        {
            if (ModelState.IsValid)
            {
                var IdForSave = model.Id;
                tagConfigurator.saveOPCProps(model, IdForSave);
                ViewBag.Notification = 1;
            }
            else
            {
                ViewBag.Notification = 0;
            }
            var userProps = tagConfigurator.getUserProps(model.Id);
            ViewBag.UserProps = userProps;
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            return PartialView("OPCPartial", model);
        }

        //редактирование узла типа PollingGroup
        [HttpPost]
        public ActionResult EditPollingGroupProps(PollingGroupProps model)
        {
            if (ModelState.IsValid)
            {
                var IdForSave = model.Id;
                tagConfigurator.savePollingGroupProps(model, IdForSave);
                ViewBag.Notification = 1;
            }
            else
            {
                ViewBag.Notification = 0;
            }
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            var userProps = tagConfigurator.getUserProps(model.Id);
            ViewBag.UserProps = userProps;
            return PartialView("PollingGroupPartial", model);
        }
        //редактирование узла типа RadioChannel
        [HttpPost]
        public ActionResult EditRadioChannelProps(RadioChannelProps model)
        {
            if (ModelState.IsValid)
            {
                var IdForSave = model.Id;
                tagConfigurator.saveRadioChannelProps(model, IdForSave);
                ViewBag.Notification = 1;
            }
            else
            {
                ViewBag.Notification = 0;
            }
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            var userProps = tagConfigurator.getUserProps(model.Id);
            ViewBag.UserProps = userProps;
            return PartialView("RadioChannelPartial", model);
        }
        //редактирование узла типа GPRSChannel
        [HttpPost]
        public ActionResult EditGPRSChannelProps(GPRSChannelProps model)
        {
            if (ModelState.IsValid)
            {
                var IdForSave = model.Id;
                tagConfigurator.saveGPRSChannelProps(model, IdForSave);
                ViewBag.Notification = 1;
            }
            else
            {
                ViewBag.Notification = 0;
            }
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            var userProps = tagConfigurator.getUserProps(model.Id);
            ViewBag.UserProps = userProps;
            return PartialView("GPRSChannelPartial", model);
        }


        //отображает свойства по клику на узел
        [HttpGet]
        public ActionResult showTabProps(int id)
        {
            //получим тип узла по id. В соответствии с типом отобразим  нужное представление
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            int typeProp = tagConfigurator.GetPropType(id);
            switch (typeProp)
            {
                //тег 
                case 2:
                    ViewBag.OPCservers = tagConfigurator.getOpcServersName();
                    var userPropsTag = tagConfigurator.getUserProps(id);
                    ViewBag.UserProps = userPropsTag;
                    var tagProps = tagConfigurator.getTagProps(id);
                    return PartialView("TagPartial", tagProps);
                case 7:
                case 5:
                    var ObjectProps = tagConfigurator.getObjectProps(id);
                    //словарь с пользовательскими свойствами
                    var userPropsObject = tagConfigurator.getUserProps(id);
                    ViewBag.driversName = filework.getDriversName(); 
                    ViewBag.UserProps = userPropsObject;
                    ViewBag.SelectChannel = tagConfigurator.getChannels();
                    return PartialView("ObjectPartial", ObjectProps);
                //OPC
                case 1:
                    var userPropsOPC = tagConfigurator.getUserProps(id);
                    ViewBag.UserProps = userPropsOPC;
                    var OPCProps = tagConfigurator.getOPCProps(id);
                    return PartialView("OPCPartial", OPCProps);
                //узел без свойства(обычная папка)
                case 21:
                return PartialView("NoPropsPartial");
                //PollingGroup
                case 22:
                var PollingGroupProps = tagConfigurator.getPollingGroupProps(id);
                var userPropsPollingGroup = tagConfigurator.getUserProps(id);
                ViewBag.UserProps = userPropsPollingGroup;
                return PartialView("PollingGroupPartial", PollingGroupProps);
                //RadioChannel
                case 18:
                var RadioChannelProps = tagConfigurator.getRadioChannelProps(id);
                var userPropsRadioChannel = tagConfigurator.getUserProps(id);
                ViewBag.UserProps = userPropsRadioChannel;
                return PartialView("RadioChannelPartial", RadioChannelProps);
                //GPRSChannel
                case 17:
                var GPRSChannelProps = tagConfigurator.getGPRSChannelProps(id);
                var userPropsGPRSChannel = tagConfigurator.getUserProps(id);
                ViewBag.UserProps = userPropsGPRSChannel;
                return PartialView("GPRSChannelPartial", GPRSChannelProps);
                //пользовательский узел
                default:
                //получаем 2 типа для передачи. 
                //словарь типа динамик
                //получаем стандартные свойства
                NoTypeNodeHelp noTypeNodeProps = tagConfigurator.getNoTypeNodeProps(id);
                //получаем объект, содержащий строковое поле для пользовательских свойств, а также поля Id, Name
                NoTypesProps noTypeNodeIdName = tagConfigurator.getNoTypeNodePropsIdName(id);
                //объект для передачи в частичное представление
                NoTypesPropsHelp model = new NoTypesPropsHelp();

                //получим узлы типа канал(радио, жпрс)
                ViewBag.SelectChannel = tagConfigurator.getChannels();
                //получим узлы типа OPC-сервер
                ViewBag.OPCservers = tagConfigurator.getOpcServersName();
                //строка подключения
                //string ConnString =jstree.getConnectionProp(id);
                int OPCid=jstree.getOPCID();
                noTypeNodeProps.Opc = OPCid;

                model.notypesforSave = noTypeNodeIdName;
                model.notypesModel = noTypeNodeProps;


                //словарь с пользовательскими свойствами
                var userProps = tagConfigurator.getUserProps(id);
                //словарь со стандартными свойствами
                var standartProps = tagConfigurator.getStandartProps(id);
                ViewBag.StandartProps = standartProps;
                ViewBag.UserProps = userProps;

                return PartialView("NoTypeNodePartial", model);
            }
        }
        //диалогое окно с модулями для подключения
        [HttpGet]
        public ActionResult showModulesToConnect()
        {
            List<moduleCondition> avaibleModules = tagConfigurator.modulesToConnect();
            return PartialView("addModulePartial", avaibleModules);
        }
        //меняет состояние модуля (активен/нет)
        [HttpGet]
        public ActionResult ChangeStatus(int id, string moduleStatus)
        {
            tagConfigurator.changeStatus(id, moduleStatus);
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            return PartialView("ModulesListPartial");
        }
        //удаление модуля
        [HttpGet]
        public ActionResult DeleteModule(int id)
        {
            tagConfigurator.deleteModule(id);
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            return PartialView("ModulesListPartial");

        }
        //добавление стандартного свойства
        [HttpPost]
        //[OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
        public ActionResult AddStandartProp(AddStandartPropModelHelp model)
        {
            //имя свойства
            var propName = ModelState.Keys.ElementAt(2);
            //список уже существующих стандратных свойств
            var standartProps = tagConfigurator.getStandartProps(model.Id);
            if (standartProps.ContainsKey(propName))
            {
                ModelState.AddModelError("selectValueStd", "Свойство уже добавлено");
            }

            if (ModelState.IsValid)
            {
                var dsdds = ModelState.Values.ElementAt(2).Value.AttemptedValue;
                PropertyInfo pi = model.GetType().GetProperty(propName);
                string value = "";
                //подготовим данные для записи в json строку
                if (pi.PropertyType == typeof(string))
                {
                    value = "\"" + dsdds + "\"";
                }
                else if (pi.PropertyType == typeof(bool))
                {
                    switch (model.selectValueStd)
                    {
                        case 3:
                            {
                                var boolExp = model.Alarm_IsPermit;
                                value = "\"" + boolExp.ToString().ToLower() + "\"";
                                break;
                            }
                        case 22:
                            {
                                var boolExp = model.History_IsPermit;
                                value = "\"" + boolExp.ToString().ToLower() + "\"";
                                break;
                            }
                        case 25:
                            {
                                var boolExp = model.IsSpecialTag;
                                value = "\"" + boolExp.ToString().ToLower() + "\"";
                                break;
                            }
                        case 49:
                            {
                                var boolExp = model.Connect;
                                value = "\"" + boolExp.ToString().ToLower() + "\"";
                                break;
                            }
                    }
                }
                else
                {
                    value = dsdds;
                }
                string forWrite = ",\"" + propName + "\":" + value;
                tagConfigurator.addProp(forWrite, model.Id);

                //отправим в onSuccess функцию данные с результатом true, чтобы обновилась
                //страница с перечнем свойств и закрылось окно добавления свойств
                var data = new { valid = true };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return PartialView("addStandartPropPartial", model);
            }


        }




        //Добавление пользовательского свойства
        [HttpPost]
        //[OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
        public ActionResult AddUserProp(AddUserPropModel model)
        {
            //Название совпадает со стандартным
            List<string> propsNames = new List<string>{
            
            "special","Id","Name","Opc","Connection", "Alarm_IsPermit","HiHiText","HiText","NormalText","LoText", "LoLoText", "HiHiSeverity", "HiSeverity","LoSeverity",
            "LoLoSeverity", "ControllerType","RealType", "RealType","Register","AccessType", "Order","InMin", "InMax", "OutMin", "OutMax",
            "History_IsPermit","RegPeriod",  "Deadbend", "IsSpecialTag", "ChannelType", "InterPollPause", "MaxErrorsToSwitchChannel",
            "MaxErrorsToBadQuality", "TimeTryGoBackToPrimary","IpAddress", "Port", "ReadTimeout", "WriteTimeout","PortName", "BaudRate","Parity",
            "StopBits", "Address","Driver", "RetrCount", "ParentGroup","PrimaryChannel", "SecondaryChannel","Start","Count", "Function", "Type","Connect"
            };

            if (propsNames.IndexOf(model.Name) != -1)
            {
                ModelState.AddModelError("Name", "Совпадает с названием стандартного свойства");
            }
            if(model.selectValue==10)
            {
                var validString = tagConfigurator.checkUserStrProp(model.StringValue);
                if(validString==false)
                {
                    ModelState.AddModelError("StringValue", "Строка содержит запрещенные символы");
                }
            }
            Dictionary<int, string> standartPropTypes = new Dictionary<int, string>
                { {1,"byte"},
                {2, "word"},
                {3, "dword"},
                {4, "shortInt"},
                {5, "smallInt"},
                {6, "longInt"},
                {7, "float"},
                {8, "double"},
                {9, "bool"},
                {10,"string"} };

            var userProps = tagConfigurator.getUserProps(model.Id);
            //var standartProps = tagConfigurator.getStandartProps(model.Id);
            var upgradeName = model.Name + "_" + standartPropTypes[model.selectValue];
            if (!String.IsNullOrEmpty(model.Name))
            {
                if (userProps.ContainsKey(upgradeName))
                {
                    ModelState.AddModelError("Name", "Свойство с таким названием уже добавлено");
                }
            }

            if (ModelState.IsValid)
            {
                var propName = model.Name;
                //var stdTypeName = ModelState.Keys.Last();
                var propValue = ModelState.Values.Last().Value.AttemptedValue;

                //PropertyInfo pi = model.GetType().GetProperty(stdTypeName);

                string value = "";
                //подготовим свойство для записи в строку в зависимости от типа, который выбрал пользователь
                switch (model.selectValue)
                {
                        //строковое
                    case 10:
                        value = "\"" + propValue + "\"";
                        break;
                    //булево
                    case 9:
                        value = (model.BoolenValue).ToString().ToLower();
                        break;
                    //float double
                    case 7:
                    case 8:
                        value = propValue.Replace(",", ".");
                        break;
                        //value = propValue.Replace(",", ".");
                        //break;
                    default:
                        value = propValue;
                        break;
                }
                //в дополнение к имени, которое вписал пользователь, будет прописан тип, выбранный юзером
                //дабы суметь валидировать измененное значение
                string forWrite = ",\"" + propName + "_" + standartPropTypes[model.selectValue] + "\":" + value;
                //пишем в базу
                tagConfigurator.addProp(forWrite, model.Id);
                //даем знать странице, что диалоговое окно надо надо закрыть, контейнер со свойствами перерисовать
                //улетает в checkValid
                var data = new { valid = true };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return PartialView("addUserPropPartial", model);
            }
        }
        //отображение диалогового окна добавления свойств
        [HttpGet]
        public ActionResult AddPropDialog(int idNode)
        {
            //получим OPC сервера
            var OpcServers = tagConfigurator.getOpcServersName();
            ViewBag.OpcServers = OpcServers;
            //строка подключения
            //var connectionStr = jstree.getConnectionProp(idNode);
            //ID сервера к которому относится узел
            var OPCId = jstree.getOPCID();
            //получим родительский узел
            var parentGroup = tagConfigurator.getParentGroup(idNode);
            //каналы
            ViewBag.SelectChannel = tagConfigurator.getChannels();
            var model = new AddPropViewModel();
            var userModel = new AddUserPropModel();
            var standartModel = new AddStandartPropModelHelp();
            //проинициализируем объекты пользовательских и стандартных свойств
            userModel.selectValue = 9;
            userModel.BoolenType = true;
            userModel.Id = idNode;
            standartModel.selectValueStd = 1;
            standartModel.ParentGroup = parentGroup;
            standartModel.Id = idNode;
            standartModel.Opc = OPCId;

            model.userPropModel = userModel;
            model.standartPropModel = standartModel;
            //ViewBag.ModulesToAdd = tagConfigurator.AddModuleDialog();
            //ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            return PartialView("addPropPartial", model);
        }
        //показывает окно добавления свойств для типизированных узлов(могут быть добавлены только пользовательские свойства)
        [HttpGet]
        public ActionResult AddPropDialogTypedNode(int idNode)
        {
            var OpcServers = tagConfigurator.getOpcServersName();
            var parentGroup = tagConfigurator.getParentGroup(idNode);
            ViewBag.OpcServers = OpcServers;
            ViewBag.SelectChannel = tagConfigurator.getChannels();
            ViewBag.ParentGroup = parentGroup;
            var userModel = new AddUserPropModel();

            userModel.selectValue = 9;
            userModel.BoolenType = true;
            userModel.Id = idNode;
            return PartialView("addPropPartialTypedNode", userModel);
        }
        //выводит диалог для сохранения при переключения на другую вкладку при имеющихся несохраненных свойствах
        [HttpGet]
        public ActionResult SaveDialog()
        {
            return PartialView("SaveDialogPartial");
        }
        //Диалоговое окно для добавления модуля
        [HttpGet]
        public ActionResult AddModuleDialog()
        {
            ViewBag.ModulesToAdd = tagConfigurator.AddModuleDialog();
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            return PartialView("addModulePartial");
        }
        //Не удалять!!! не получилось, надо проанализировать почему
        //[HttpGet]
        //public ActionResult AddNode1(int idParentElem)
        //{
        //    var model = new AddNodeModel();
        //    model.idNodeToAdd = idParentElem;
        //    model.nodeType = 1;
        //    ViewBag.ForDefaultNode = jstree.findObjForDefaultNode();
        //    return PartialView("addNodeAigPartial", model);
        //}
       

        //[HttpPost]
        //public ActionResult AddNode1(AddNodeModel model)
        //{
        //    var existName = tagConfigurator.checkExistingNodeName(model.Name);

        //    if (existName == true)
        //    {
        //        ModelState.AddModelError("Name", "Узел с таким названием уже существует");
        //    }
        //    if(ModelState.IsValid)
        //    {
        //        jstree.addNode(model.Name, model.nodeType, model.idNodeToAdd, model.userNodeObjType);
        //        var data = new { valid = true };
        //        return Json(data, JsonRequestBehavior.AllowGet);

        //    }
        //    else
        //    {
        //        ViewBag.ForDefaultNode = jstree.findObjForDefaultNode();
        //        return PartialView("addNodePartial1", model);
        //    }
        //}

        //диалоговое окно добавления узла
        [HttpGet]
        public ActionResult AddNodeDialog(int idParentElem)
        {

            ViewBag.ForDefaultNode = jstree.findObjForDefaultNode();
            return PartialView("addNodePartial", idParentElem);

        }
        //запись в базу нового узла
        [HttpPost]
        public void AddNode(AddNodeModel model)
        {
            jstree.addNode(model.Name, model.nodeType, model.idNodeToAdd, model.userNodeObjType);
        }

        [HttpGet]
        public ActionResult AddModule(List<string> idModStr)
        {
            //добавить фунцию для отправки данных и посмотреть закрывается ли диалог окно 
            tagConfigurator.AddModule(idModStr);
            ViewBag.ActiveModules = tagConfigurator.GetConnectedModules();
            return PartialView("ModulesListPartial");

        }
        //вставка узла
        [HttpPost]
        public void PasteNode(int idPasteParentElem, int idCopyParentElem)
        {
            jstree.pasteNode(idPasteParentElem, idCopyParentElem);
        }
        //удаление узла
        [HttpPost]
        public void DeleteNode(int idDeleteElem)
        {
            jstree.deleteJsTreeNode(idDeleteElem);
        }

    }
}