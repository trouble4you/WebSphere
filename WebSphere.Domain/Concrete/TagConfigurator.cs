using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Entities;



namespace WebSphere.Domain.Concrete
{
    public class TagConfigurator : ITagConfigurator
    {
        private readonly EFDbContext context = new EFDbContext();
        private static readonly JSON json = new JSON();
        //Получить тип узла
        public int GetNodeType(int id)
        {
            var sel2 = context.Objects.Where(c => c.Id == id).Select(c => c.Type).FirstOrDefault();
            return sel2;
        }

        //Название OPC-сервера
        public Dictionary<int, string> getOpcServersName()
        {
            var Opcs = context.Objects.Where(c => c.Type == 1).ToList();
            Dictionary<int, string> OpcDictionary = new Dictionary<int, string>();

            foreach (var item in Opcs)
            {
                OpcDictionary.Add(item.Id, item.Name);
            }

            return OpcDictionary;
        }
        //получение родительской группы 
        //public int? getParentGroup(int id)
        //{
        //    int? parentGroup = context.Objects.Where(c => c.Id == id).FirstOrDefault().ParentId;
        //    return parentGroup;

        //}


        //получаем узлы, соответствующие каналам радио и gprs
        public Dictionary<int, string> getChannels(int idCallingNode)
        {
            Dictionary<int, string> channelsList = new Dictionary<int, string>();
            using (var context = new EFDbContext())
            {
                var channels = context.Objects.Where(c => c.Type == 17 || c.Type == 18);
                foreach (var channel in channels)
                {
                    channelsList.Add(channel.Id, channel.Name);
                }
                return channelsList;
            }
        }
        public List<int> getChannelsInFolder(int folderId) //возвращает дочерние узлы папки
        {
            List<int> channelsInFolder = new List<int>();
            using (var context = new EFDbContext())
            {
                channelsInFolder = context.Objects.Where(o => o.ParentId == folderId).Select(o => o.Id).ToList();
            }
            return channelsInFolder;
        }



        public void saveObjectProps(ObjectProps objectProps, int id)
        {
            var serProps = json.Serialize(objectProps);
            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = serProps;
            context.SaveChanges();
        }

        public void saveTagProps(TagProps tagProps, int id)
        {
            var serProps = json.Serialize(tagProps);
            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = serProps;
            context.SaveChanges();
        }
        public void saveOPCProps(OPCProps opcProps, int id)
        {
            var serProps = json.Serialize(opcProps);
            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = serProps;
            context.SaveChanges();
        }

        public void savePollingGroupProps(PollingGroupProps pollingGroupProps, int id)
        {
            var serProps = json.Serialize(pollingGroupProps);
            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = serProps;
            context.SaveChanges();
        }
        public void saveRadioChannelProps(RadioChannelProps radioChannelProps, int id)
        {
            var serProps = json.Serialize(radioChannelProps);
            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = serProps;
            context.SaveChanges();
        }
        public void saveGPRSChannelProps(GPRSChannelProps gprsChannelProps, int id)
        {
            var serProps = json.Serialize(gprsChannelProps);
            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = serProps;
            context.SaveChanges();
        }


        public TagProps getTagProps(int id)
        {
            var sel1 = context.Properties.AsNoTracking().Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var sel2 = context.Objects.Find(id);
            var tagProps = new TagProps();
            string JSONprop = sel1.Value;
            var props = json.Deserialize(JSONprop, tagProps.GetType());
            tagProps = (TagProps)props;
            tagProps.Id = id;
            tagProps.Name = sel2.Name;
            return tagProps;
        }

        public RadioChannelProps getRadioChannelProps(int id)
        {
            var sel1 = context.Properties.AsNoTracking().Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var sel2 = context.Objects.Find(id);
            var Props = new RadioChannelProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (RadioChannelProps)propsDes;
            Props.Id = id;
            Props.Name = sel2.Name;
            return Props;
        }

        public GPRSChannelProps getGPRSChannelProps(int id)
        {
            var sel1 = context.Properties.AsNoTracking().Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var sel2 = context.Objects.Find(id);
            var Props = new GPRSChannelProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (GPRSChannelProps)propsDes;
            Props.Id = id;
            Props.Name = sel2.Name;
            return Props;
        }
        public ObjectProps getObjectProps(int id)
        {
            var sel1 = context.Properties.AsNoTracking().Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var sel2 = context.Objects.Find(id);
            var Props = new ObjectProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (ObjectProps)propsDes;
            Props.Id = id;
            Props.Name = sel2.Name;
            return Props;
        }
        public PollingGroupProps getPollingGroupProps(int id)
        {
            var sel1 = context.Properties.AsNoTracking().Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var sel2 = context.Objects.Find(id);
            var Props = new PollingGroupProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (PollingGroupProps)propsDes;
            Props.Id = id;
            Props.Name = sel2.Name;
            return Props;
        }
        public OPCProps getOPCProps(int id)
        {
            var sel1 = context.Properties.AsNoTracking().Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var sel2 = context.Objects.Find(id);
            var Props = new OPCProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (OPCProps)propsDes;
            Props.Id = id;
            Props.Name = sel2.Name;
            return Props;
        }

        public List<moduleCondition> GetModules()
        {
            List<moduleCondition> moduleConnectedList = new List<moduleCondition>();

            using (var context = new EFDbContext())
            {
                var moduleList = context.Objects.Where(t => t.Type == 3).Join(context.Properties, o => o.Id, p => p.ObjectId,
                    (o, p) => new
                    {
                        Id = o.Id,
                        Name = o.Name,
                        IsConnected = context.Properties.Where(q => q.PropId == 100 && q.ObjectId == o.Id).Select(q => q.Value).FirstOrDefault(),
                        IsRun = context.Properties.Where(q => q.PropId == 101 && q.ObjectId == o.Id).Select(q => q.Value).FirstOrDefault(),
                        Description = context.Properties.Where(q => q.PropId == 102 && q.ObjectId == o.Id).Select(q => q.Value).FirstOrDefault()
                    }).Distinct();
                foreach (var item in moduleList)
                {
                    var module = new moduleCondition
                    {
                        idModule = item.Id,
                        isConnected = Convert.ToBoolean(item.IsConnected),
                        isRun = item.IsRun,
                        descrModule = item.Description,
                        nameModule = item.Name
                    };
                    moduleConnectedList.Add(module);
                }
            }
            return moduleConnectedList;

        }

        //public List<moduleCondition> GetConnectedModules()
        //{
        //    var moduleConnectedList = new List<moduleCondition>();
        //    var sel1 = from o1 in context.Properties
        //               where o1.PropId == 100 && o1.Value == "1"
        //               select o1.ObjectId;
        //    var sel2 = from o1 in context.Properties
        //               join o2 in context.Objects on o1.ObjectId equals o2.Id
        //               where o1.PropId == 101 && sel1.Contains(o1.ObjectId)
        //               select new
        //               {

        //                   modId = o1.ObjectId,
        //                   modRun = o1.Value,
        //                   modName = o2.Name
        //               };
        //    var a = sel2.Any();
        //    if (a)
        //    {
        //        foreach (var item in sel2)
        //        {
        //            var result = new moduleCondition();
        //            result.idModule = item.modId;
        //            result.nameModule = item.modName;
        //            result.isRun = item.modRun;
        //            result.isConnected = true;
        //            moduleConnectedList.Add(result);
        //        }
        //    }
        //    else
        //    {
        //        var result = new moduleCondition();
        //        moduleConnectedList.Add(result);
        //    }
        //    return moduleConnectedList;
        //}

        public List<moduleCondition> modulesToConnect()
        {
            var listOfAvaibleModules = new List<moduleCondition>();
            var sel = from o1 in context.Properties
                      join o2 in context.Objects on o1.ObjectId equals o2.Id
                      join o3 in context.Properties on o1.ObjectId equals o3.ObjectId
                      where o1.PropId == 100 && o1.Value == "0" && o3.PropId == 102
                      select new
                      {
                          modId = o1.ObjectId,
                          modName = o2.Name,
                          modDescr = o3.Value
                      };
            var a = sel.Any();
            if (a)
            {
                foreach (var item in sel)
                {
                    var result = new moduleCondition();
                    result.idModule = item.modId;
                    result.nameModule = item.modName;
                    result.descrModule = item.modDescr;
                    //result.isRun = item.modRun;
                    listOfAvaibleModules.Add(result);
                }
            }
            else
            {
                var result = new moduleCondition();
                listOfAvaibleModules.Add(result);
            }
            return listOfAvaibleModules;
        }

        public void deleteModule(int id)
        {
            //модуль не подключен
            var modForDel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 100).FirstOrDefault();
            modForDel.Value = "false";
            //модуль остановлен
            var modForDelRun = context.Properties.Where(c => c.ObjectId == id && c.PropId == 101).FirstOrDefault();
            modForDelRun.Value = "0";
            context.SaveChanges();
        }

        public void ChangeModuleStatus(int id, string moduleStatus)
        {
            var modForDel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 101).FirstOrDefault();
            modForDel.Value = moduleStatus;
            context.SaveChanges();
        }

        public List<moduleCondition> AddModuleDialog()
        {
            var listOfAvaibleModules = new List<moduleCondition>();

            var sel = from o1 in context.Properties
                      join o2 in context.Objects on o1.ObjectId equals o2.Id
                      join o3 in context.Properties on o1.ObjectId equals o3.ObjectId
                      where o1.PropId == 100 && o1.Value == "false" && o3.PropId == 102
                      select new
                      {
                          modId = o1.ObjectId,
                          modName = o2.Name,
                          modDescr = o3.Value
                      };
            var a = sel.Any();
            if (a)
            {
                foreach (var item in sel)
                {
                    var result = new moduleCondition();
                    result.idModule = item.modId;
                    result.nameModule = item.modName;
                    result.isConnected = false;
                    result.isRun = "0";
                    result.descrModule = item.modDescr;
                    listOfAvaibleModules.Add(result);

                }
            }
            else
            {
                var result = new moduleCondition();
                listOfAvaibleModules.Add(result);
            }
            return listOfAvaibleModules;
        }

        public void AddModule(List<int> idModList)
        {
            var moduleForAdd = context.Properties.Where(c => idModList.Contains(c.ObjectId) && c.PropId == 100);
            foreach (var item in moduleForAdd)
            {
                item.Value = "true";
            }
            context.SaveChanges();
        }
    }

    public class moduleCondition //in use
    {
        public int idModule { get; set; }
        public string nameModule { get; set; }
        public string isRun { get; set; }
        public bool isConnected { get; set; }
        public string descrModule { get; set; }
    }
}

