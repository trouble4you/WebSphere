using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
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
        public static List<moduleCondition> connectedModules;
        public static Dictionary<int, string> OPCServersName;
        public static bool isCopyNow;
        //public static List<ConfiguratorState> configuratorState = new List<ConfiguratorState>();

        public ConfiguratorController(IJSTree jstree, ITagConfigurator tagConfigurator, IJSON json)
        {
            this.jstree = jstree;
            this.tagConfigurator = tagConfigurator;
            this.json = json;
        }
        public ActionResult Index()
        {
            int rootNodeId = jstree.getRootNodeId();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            ViewBag.JStreeStr = jstree.CreateJsTreeHelp(rootNodeId);

            sw.Stop();
            connectedModules = tagConfigurator.GetModules();
            OPCServersName = tagConfigurator.getOpcServersName();
            List<moduleCondition> ActiveModules = connectedModules.Where(c => c.isConnected == true).ToList();
            return View("Index", ActiveModules);
        }

        //public ActionResult WatchUserManipulation(int nodeId)
        //{
        //    //if(nodeId!=0)//если на момент вызова ajax запроса пользователем уже был выбран какой-то узел
        //    //{
        //        if (ConfiguratorStateList.configuratorStateList.Count == 0)
        //        {
        //            ConfiguratorStateList.AddConfNode(nodeId, User.Identity.Name);
        //        }
        //        else
        //        {
        //            ConfiguratorStateList.CheckPropsChanges(nodeId, User.Identity.Name);

        //            //ConfiguratorState nodeState = configuratorState.Where(c => c.NodeId == nodeId).FirstOrDefault();
        //            //if (nodeState.flags.ChangeProps == true)//если есть изменения свойств 
        //            //{
        //            //    bool stateUserNotification = nodeState.Users.First(u => u.UserName == User.Identity.Name).NeedShowNews;
        //            //    nodeState.Users.First(u => u.UserName == User.Identity.Name).NeedShowNews = false;//выставим для текущего пользователя флаг NeedShowNews в false
        //            //    if (!nodeState.Users.Any(u => u.NeedShowNews))//проверим есть ли еще пользователи, у которых NeedShowNews true
        //            //        nodeState.flags.ChangeProps = false;//если нет, то установим флаг ChangeProps в false для узла
        //            //    if (stateUserNotification == true)//если пользователю нужно показать уведомление
        //            //        return PartialView("notificationToUserPartial");
        //            //    else
        //            //        return Json(new { valid = true }, JsonRequestBehavior.AllowGet);
        //            //}
        //            //else
        //            //    return Json(new { valid = true }, JsonRequestBehavior.AllowGet);
        //        }

        //    //}
        //    //else
        //    //{
        //    //    return Json(new { valid = true }, JsonRequestBehavior.AllowGet);
        //    //}

        //}

        [HttpGet]
        public ActionResult CopyControllerNode(int nodeId)//вспомогательный метод при копировании/вставке узла-контроллера
        {
            //if (isCopyNow)//если он уже копируется
            //    return PartialView("copyBanPartial");
            //else
            //{
            if (jstree.CheckNodeExists(nodeId))//проверим-ка есть ли копируемый узел в базе
            {
                return PartialView("copyControllerPartial");//если существует, то выводим окно об указании имени вставляемого узла
            }
            else
                return PartialView("copyBanPartial", false);//либо выводим окно об отсутствии узла
            //}



        }


        //Добавление узла
        [HttpGet]
        public ActionResult AddNode(int idParentElem)
        {

            if (isCopyNow)
                return PartialView("copyBanPartial", true);
            else
            {
                var model = new AddNodeModel();
                if (jstree.CheckNodeExists(idParentElem))
                {

                    model.idNodeToAdd = idParentElem;
                    model.nodeType2 = 1;
                    model.typeParentNode = tagConfigurator.GetNodeType(idParentElem);
                    return PartialView("addNodePartial", model);
                }
                else
                {
                    return PartialView("copyBanPartial", false);
                }


            }

        }


        [HttpPost]
        public ActionResult AddNode(AddNodeModel model)
        {
            if (ModelState.IsValid)
            {
                int newNodeId = jstree.addNode(model.Name, model.nodeType2, model.idNodeToAdd);
                var data = new
                {
                    valid = true,
                    id = newNodeId,
                    name = model.Name,
                    type = model.nodeType2
                };
                //List<string> userNames = configuratorState.SelectMany(c => c.Users.Select(e => e.UserName)).ToList();//возьмем всех пользователей из configuratorState

                //userNames.Distinct();
                //userNames.Remove(User.Identity.Name);

                //ConfiguratorState confState = new ConfiguratorState
                //{
                //    NodeId = newNodeId,
                //    Users = new List<ConfiguratorUser> { new ConfiguratorUser { UserName = User.Identity.Name, IsNeedReboot = false, NeedShowNews = false } },
                //    flags = new Flags { AddNodes = true, ChangeProps = false, DeleteNodes = false }
                //};
                //foreach (var item in userNames)//добавим остальных пользователей с выставленным флагом обновления
                //{
                //    confState.Users.Add(new ConfiguratorUser { UserName = item, IsNeedReboot = true, NeedShowNews = false });
                //}

                //configuratorState.Add(confState);

                if (model.nodeType2 == 1)
                {
                    OPCServersName.Add(newNodeId, model.Name);
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return PartialView("addNodeForm", model);
            }
        }

        //вставка узла
        [HttpPost]
        public ActionResult PasteNode(int idPasteParentElem, int idCopyParentElem, string newContName)
        {
            if (isCopyNow)
                return PartialView("copyBanPartial", true);
            else
            {

                if (jstree.CheckNodeExists(idPasteParentElem) && jstree.CheckNodeExists(idCopyParentElem))
                {
                    isCopyNow = true;
                    string newPartOfTree = jstree.pasteNodeRoot(idPasteParentElem, idCopyParentElem, newContName);

                    isCopyNow = false;
                    return Content(newPartOfTree);
                }
                else
                    return PartialView("copyBanPartial", false);
            }
        }

        //удаление узла
        [HttpPost]
        public void DeleteNode(int idDeleteElem)
        {
            List<int> IdToDeleteList = jstree.deleteJsTreeNodeRoot(idDeleteElem);//получим список ID всех объектов, подлежащих удалению.
            if (IdToDeleteList.Count != 0)
            {
                jstree.deleteJsTreeNodeBulk(IdToDeleteList);//Непосредственно удаление
                if (OPCServersName.Keys.Contains(idDeleteElem))//вдруг удалили OPC сервер.Тогда извлечем его из списка
                {
                    OPCServersName.Remove(idDeleteElem);
                }
            }

        }

        //удаление узла канала
        [HttpPost]
        public ActionResult DeleteChannelNode(int channelId, int folderId)
        {
            List<int> idDeleteElems = new List<int>();
            List<int> objectsId = jstree.findObjectNodes();

            List<ObjectProps> objects = new List<ObjectProps>();
            foreach (var item in objectsId)
            {
                objects.Add(tagConfigurator.getObjectProps(item));
            }
            if (channelId != 0 && folderId == 0)//если удаляется отдельно узел
            {
                idDeleteElems.Add(channelId);
            }
            else//если удаляется папка с узлами каналов связи
            {
                idDeleteElems = tagConfigurator.getChannelsInFolder(folderId);
            }
            //получим те узлы(объекты), которые используют эти каналы
            List<ObjectProps> busyChannels1 = objects.Where(o => idDeleteElems.Contains(o.PrimaryChannel) || idDeleteElems.Contains(o.SecondaryChannel)).ToList();

            Dictionary<int, string> objectsForView = new Dictionary<int, string>();
            foreach (var item in busyChannels1)//если есть задействованные узлы, то вернем словарь
            {
                objectsForView.Add(item.Id, item.Name);
            }
            if (objectsForView.Count != 0)
            {
                return PartialView("DeleteNodePartial", objectsForView);
            }
            else//если нет, то JSON
            {
                var data = new { valid = true };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        //переименование узла
        [HttpGet]
        public void RenameNode(int idRenameNode, string newNodeName)
        {
            var id = idRenameNode;

            int typeProp = tagConfigurator.GetNodeType(id);
            jstree.renameNode(idRenameNode, newNodeName, typeProp);
        }

        //отображает свойства по клику на узел
        [HttpGet]
        public ActionResult showTabProps(int id)
        {
            //получим тип узла по id. В соответствии с типом отобразим  нужное представление
            List<moduleCondition> ActiveModules = connectedModules.Where(c => c.isConnected == true).ToList();
            //var node=configuratorState.FirstOrDefault(n => n.NodeId == id);
            //if(node==null)//если такого узла в просматриваемых нет, то добавим его
            //{
            //    ConfiguratorState confState = new ConfiguratorState()
            //    {
            //        NodeId = id,
            //        flags = new Flags { AddNodes = false, ChangeProps = false, DeleteNodes = false },
            //        Users = new List<ConfiguratorUser>() { new ConfiguratorUser { UserName = User.Identity.Name, NeedShowNews = false, IsNeedReboot = false } }
            //    };
            //}
            //else//если он есть проверим просматривает ли его кто-то другой
            //{
            //    var checkUserWatchNode = node.Users.Where(u => u.UserName == User.Identity.Name);//может этот пользователь был списке, потому что пользователь повторно кликнул на узел
            //    if(!checkUserWatchNode.Any())//если такого просматривающего узел пользователя не было, то добавим его в список пользователей узла
            //    {
            //        ConfiguratorUser confUser = new ConfiguratorUser() { UserName = User.Identity.Name, IsNeedReboot = false, NeedShowNews = false };//создадим заготовку нового пользователя узла
            //        var node2 = configuratorState.FirstOrDefault(x => x.Users.Any(y => y.UserName == User.Identity.Name));//проверим, просматривал ли он до этого другие узлы
            //        if(node2!=null)//если просмативал
            //        {
            //            var user=configuratorState.Select(x => x.Users.First(y => y.UserName == User.Identity.Name)).ToArray();//получим этого пользователя
            //            //var user2 = configuratorState.Where(x => x.Users.First(y => y.UserName == User.Identity.Name)).ToArray();//получим этого пользователя
            //            node2.Users.Remove(user[0]);//и удалим из списка
            //        }
            //        node.Users.Add(confUser);//добавим этого пользователя в список пользователей этого узла
            //    }
            //    var addedNode = configuratorState.Select(n => n.flags.AddNodes == true);
            //    if(addedNode.Any())
            //    {

            //    }

            //}
            ViewBag.ActiveModules = ActiveModules;
            int typeProp = tagConfigurator.GetNodeType(id);
            switch (typeProp)
            {

                //OPC
                case 1:
                    var OPCProps = tagConfigurator.getOPCProps(id);
                    return PartialView("OPCPartial", OPCProps);
                //тег 
                case 2:
                    ViewBag.OPCservers = OPCServersName;
                    var tagProps = tagConfigurator.getTagProps(id);
                    if (tagProps.Alarms == null)
                    {
                        WebSphere.Domain.Entities.Alarms alarms = new WebSphere.Domain.Entities.Alarms();
                        alarms.Permit = false;
                        tagProps.Alarms = alarms;
                    }
                    return PartialView("TagPartial", tagProps);
                //контроллер
                case 5:
                    var ObjectProps = tagConfigurator.getObjectProps(id);
                    ViewBag.SelectChannel = tagConfigurator.getChannels(id);
                    return PartialView("ObjectPartial", ObjectProps);
                //GPRSChannel
                case 17:
                    var GPRSChannelProps = tagConfigurator.getGPRSChannelProps(id);
                    return PartialView("GPRSChannelPartial", GPRSChannelProps);
                //RadioChannel
                case 18:
                    var RadioChannelProps = tagConfigurator.getRadioChannelProps(id);
                    return PartialView("RadioChannelPartial", RadioChannelProps);
                //корневой элемент дерева
                case 23:
                //узел без свойства(обычная папка)
                case 21:
                    return PartialView("NoPropsPartial");
                //PollingGroup
                case 22:
                    var PollingGroupProps = tagConfigurator.getPollingGroupProps(id);
                    return PartialView("PollingGroupPartial", PollingGroupProps);
                default:
                    return PartialView("NoNodePartial");
            }
        }


        //редактирование узла типа Tag
        [HttpPost]
        public ActionResult EditTagProps(TagProps model)
        {
            if (ModelState.IsValid)
            {
                //если валидны и польз и стандратные, то сохраняем
                var IdForSave = model.Id;
                tagConfigurator.saveTagProps(model, IdForSave);
                ViewBag.Notification = 1;

                //ConfiguratorState confState = configuratorState.Where(c => c.NodeId == model.Id).FirstOrDefault();//найдем состояние для данного узла
                //confState.flags.ChangeProps = true;//установим для узла флаг изменений
                //for (int i = 0; i < confState.Users.Count; i++)
                //{
                //    if (confState.Users[i].UserName != User.Identity.Name)
                //    {
                //        confState.Users[i].NeedShowNews = true;
                //    }
                //}

            }
            else
            {
                ViewBag.Notification = 0;
            }
            List<moduleCondition> ActiveModules = connectedModules.Where(m => m.isConnected == true).ToList();
            ViewBag.ActiveModules = ActiveModules;
            ViewBag.OPCservers = OPCServersName;
            ViewBag.SelectChannel = tagConfigurator.getChannels(model.Id);
            return PartialView("TagPartial", model);
        }

        //редактирование узла типа Object
        //[ValidateAntiForgeryToken]
        [HttpPost]
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
            }
            List<moduleCondition> ActiveModules = connectedModules.Where(m => m.isConnected == true).ToList();
            ViewBag.ActiveModules = ActiveModules;

            ViewBag.SelectChannel = tagConfigurator.getChannels(model.Id);
            return PartialView("ObjectPartial", model);
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
            return PartialView("GPRSChannelPartial", model);
        }


        ////диалогое окно с модулями для подключения
        //[HttpGet]
        //public ActionResult showModulesToConnect()
        //{

        //    List<moduleCondition> avaibleModules = tagConfigurator.modulesToConnect();
        //    //return PartialView("addModulePartial", avaibleModules);
        //    return PartialView("addModulePartial", connectedModules);
        //}

        //меняет состояние модуля (активен/нет)
        [HttpGet]
        public ActionResult ChangeModuleStatus(int id, string moduleStatus)
        {
            tagConfigurator.ChangeModuleStatus(id, moduleStatus);
            moduleCondition module = connectedModules.Where(m => m.idModule == id).FirstOrDefault();
            module.isRun = moduleStatus;
            List<moduleCondition> ActiveModules = connectedModules.Where(m => m.isConnected == true).ToList();
            return PartialView("ModulesListPartial", ActiveModules);
        }

        //Диалоговое окно для добавления модуля
        [HttpGet]
        public ActionResult AddModule()
        {
            //ViewBag.ModulesToAdd = tagConfigurator.AddModuleDialog();
            List<moduleCondition> modules = tagConfigurator.GetModules();
            List<moduleCondition> model = modules.Where(m => m.isConnected == false).ToList();
            return PartialView("addModulePartial", model);
        }

        [HttpPost]
        public ActionResult AddModule(List<moduleCondition> model)
        {
            //добавить фунцию для отправки данных и посмотреть закрывается ли диалог окно
            List<int> connectedBefore = connectedModules.Select(m => m.idModule).ToList();
            List<int> connectedAfter = model.Where(m => m.isConnected == true).Select(m => m.idModule).ToList();
            List<int> connectNewModuleHelp = connectedBefore.Union(connectedAfter).ToList();//получим ID модулей, которые являются подключенными
            List<int> connectNewModule = connectNewModuleHelp.Except(connectedBefore).ToList();
            if (connectNewModule.Count() != 0) //если появились модули, которых нет в глобальном статическом списке, то надо их туда добавить
            {
                List<moduleCondition> connectedAfterModules = model.Where(m => connectNewModule.Contains(m.idModule)).ToList();
                foreach (moduleCondition item in connectedAfterModules)
                {
                    connectedModules.Add(item);
                }
            }
            //поменяем состояние модуля на подключено
            foreach (var item in connectedAfter)
            {
                moduleCondition module = connectedModules.Where(m => m.idModule == item).FirstOrDefault();
                module.isConnected = true;
            }
            //connectedModules = model;
            tagConfigurator.AddModule(connectedAfter);
            List<moduleCondition> ActiveModules = connectedModules.Where(m => m.isConnected == true).ToList();
            return PartialView("ModulesListPartial", ActiveModules);

        }
        //удаление модуля
        [HttpGet]
        public ActionResult DeleteModule(int id)
        {
            moduleCondition module = connectedModules.Find(m => m.idModule == id);
            module.isConnected = false;
            tagConfigurator.deleteModule(id);
            List<moduleCondition> ActiveModules = connectedModules.Where(m => m.isConnected == true).ToList();
            return PartialView("ModulesListPartial", ActiveModules);
        }

        [HttpPost]
        public PartialViewResult AddEvent(TagProps model) //вспомогательный метод для добавления событий тегу
        {
            var newEvent = new WebSphere.Domain.Entities.EventValMessage { Value = 0, Message = "Сообщение сигнала с ID=" + model.Id };
            if (model.Events.EventMessages == null)
            {
                var evMsgList = new List<WebSphere.Domain.Entities.EventValMessage>();
                model.Events.EventMessages = new List<WebSphere.Domain.Entities.EventValMessage>();

            }
            model.Events.EventMessages.Add(newEvent);

            return PartialView("AddEventPartial", model);
        }

        //[HttpGet]
        //public string СheckTreeUpdates()//при клике на узел проверим проводились ли манипуляции с деревом
        //{
        //    //список на случай если 2 пользователя что-то добавили или удалили
        //    //List<ConfiguratorState> treeUpdate = configuratorState.Where(c => c.flags.AddNodes == true || c.flags.DeleteNodes == true).ToList();
        //    //если изменения были таки
        //    //if (treeUpdate != null)
        //    //{
        //    //    foreach (var item in treeUpdate)
        //    //    {
        //    //        List<string> treeUpdateUsers = item.Users.Select(u => u.UserName).ToList();
        //    //        //проверка на случай если сейчас пользователь которого либо добавили до изменений либо пользователь не был в конфигураторе на момент изменений
        //    //        if (treeUpdateUsers.Contains(User.Identity.Name))
        //    //        {
        //    //            item.Users.First(u => u.UserName == User.Identity.Name).IsNeedReboot = false;
        //    //        }
        //    //        var lala = item.Users.Select(t => t.IsNeedReboot == true);//проверим у всех ли пользователей перезагрузилось дерево
        //    //        if (lala == null)//если у всех теперь IsNeedReboot=true
        //    //            item.flags.AddNodes = false;

        //    //    }
        //    //    int rootNodeId = jstree.getRootNodeId();
        //    //    string newTree = jstree.CreateJsTreeHelp(rootNodeId);

        //    //    return newTree;


        //    //}
        //    //else
        //    //{
        //    //    return "0";
        //    //}

        //}

    }
}