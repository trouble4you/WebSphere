using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Entities;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Reflection;
using System.Collections;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.MappingAPI;
using EntityFramework.Extensions;

namespace WebSphere.Domain.Concrete
{
    public class JSTree : IJSTree
    {
        private readonly EFDbContext context;
        private static readonly JSON json = new JSON();
        public static List<Objects> copyObjects;
        public static List<Property> propertyList;
        public static List<Objects> pasteObjects;
        public static List<Property> pastePropertyList;
        private static TagProps tagPropsForCopy;
        public JSTree()
        {
            this.context = new EFDbContext();
        }
        public int getRootNodeId()
        {
            int id = context.Objects.Where(m => m.Type == 23).AsNoTracking().Select(m => m.Id).FirstOrDefault();
            return id;
        }

        List<Objects> objList;

        public string CreateJsTreeHelp(int parID)
        {
            //Stopwatch sw1 = new Stopwatch();
            //sw1.Start();
            objList = context.Objects.AsNoTracking().ToList();
            //sw1.Stop();

            return CreateJsTree(parID);
        }

        public MetaObject CreateMetaObject(int parentId, ref Dictionary<int, object> Nodes)
        {

            MetaObject metaObject = new MetaObject();
            Objects root = objList.Where(c => c.Id == parentId).FirstOrDefault();
            var childs = objList.Where(c => c.ParentId == parentId).Select(c => c);

            metaObject.Id = root.Id;
            metaObject.Name = root.Name;
            metaObject.ParentId = root.ParentId;
            metaObject.Type = root.Type;
            if (childs.Count() != 0)
            {
                foreach (var child in childs)
                {
                    MetaObject metaObjectChild = CreateMetaObject(child.Id, ref Nodes);
                    if (metaObject.Children == null)
                    {
                        metaObject.Children = new List<MetaObject>();
                    }
                    metaObject.Children.Add(metaObjectChild);
                }
                Nodes.Add(root.Id, metaObject.Children);
            }
            else
            {
                metaObject.Children = null;
                Nodes.Add(metaObject.Id, null);
            }
            return metaObject;
        }

        public string CreateJsTree(int parId)
        {
            string childstring = "";
            Objects root = objList.Where(c => c.Id == parId).FirstOrDefault();
            List<Objects> childs = objList.Where(c => c.ParentId == parId).Select(c => c).ToList();
            int counter = 0;
            int max = childs.Count();

            foreach (var child in childs)
            {
                if (counter != 0)
                    childstring += ",";
                childstring += CreateJsTree(child.Id);
                counter++;
            }

            objList.Remove(root);
            if (root.Type == 17 || root.Type == 18)
            {
                return "{ " +
                  "\"id\" : \"" + root.Id + "\", " +
                  "\"text\": \"" + root.Name + "\", " +
                  "\"icon\": \"jstree-type_" + root.Type + "\", " +
                  "\"li_attr\":{\"name\":\"channelNode\"}," +
                  "\"children\": [" + childstring + "] " +
                  "}";
            }
            else if (root.Type == 21)
            {
                return "{ " +
                  "\"id\" : \"" + root.Id + "\", " +
                  "\"text\": \"" + root.Name + "\", " +
                  "\"icon\": \"jstree-type_" + root.Type + "\", " +
                  "\"li_attr\":{\"name\":\"folder\"}," +
                  "\"children\": [" + childstring + "] " +
                  "}";
            }
            else
            {
                return "{ " +
                   "\"id\" : \"" + root.Id + "\", " +
                   "\"text\": \"" + root.Name + "\", " +
                   "\"icon\": \"jstree-type_" + root.Type + "\", " +
                   "\"children\": [" + childstring + "] " +
                   "}";
            }
        }


        //string connProp = "";
        //получает строку подключения и Id OPC сервера
        public string getConnectionProp(int? idElemToPaste, out int OPCId)
        {
            string connProp = "";
            int Id = 0;
            using (var context = new EFDbContext())
            {
                var parent = context.Objects.Where(c => c.Id == idElemToPaste).AsNoTracking().FirstOrDefault();
                if (parent.Type != 1)
                {
                    connProp += getConnectionProp(parent.ParentId, out OPCId);
                    return connProp += parent.Name + "/";
                }
                else
                {
                    if (parent.Type == 1)
                    {
                        Id = parent.Id;

                    }
                    OPCId = Id;
                    return parent.Name + "/";
                }
            }

        }

        public int addNode(string newNodeName, int newNodeType, int idNodeToAdd)
        {
            var nodeType = 0;
            int OPCId = 0;
            string jsonPropEnd = "";
            switch (newNodeType)
            {
                //OPC-сервер
                case 1:
                    jsonPropEnd = ",\"Type\":\"\",\"Connection\":\"\",\"Connect\":false}";
                    nodeType = 1;
                    break;
                //Тег
                case 2:
                    string ConnString = getConnectionProp(idNodeToAdd, out OPCId).Trim('/');
                    jsonPropEnd = ",\"Opc\":" + OPCId + ",\"Connection\":\"" + ConnString + "\",\"Description\":null";
                    jsonPropEnd += ",\"ControllerType\":0,\"RealType\":0,\"Register\":\"\",\"AccessType\":0";
                    jsonPropEnd += ",\"Order\":1,\"InMin\":0,\"InMax\":1,\"OutMin\":1,\"OutMax\":1,\"IsSpecialTag\":null,\"History_IsPermit\":false,\"RegPeriod\":0,\"Deadbend\":0.1,\"UpdateAnyway\":false";
                    jsonPropEnd += ",\"Alarms\":{\"Permit\":false,\"Enabled\":false,\"Sound\":false,\"HiHiText\":null,\"HiText\":null,\"NormalText\":null,\"LoText\":null,\"LoLoText\":null";
                    jsonPropEnd += ",\"HiHiSeverity\":null,\"HiSeverity\":null,\"LoSeverity\":null,\"LoLoSeverity\":null}";
                    jsonPropEnd += ",\"Events\":{\"Enabled\":false,\"EventMessages\":[]}}";
                    nodeType = 2;
                    break;
                //Контроллер
                case 5:
                    jsonPropEnd = ",\"Address\":0,\"Driver\":\"\",\"RetrCount\":0,\"ParentGroup\":" + idNodeToAdd + ",\"PrimaryChannel\":0,\"SecondaryChannel\":0,\"PgPause\":0}";
                    nodeType = 5;
                    break;
                //GPRS канал
                case 17:
                    jsonPropEnd = ",\"ChannelType\":\"Tcp\",\"InterPollPause\":0,\"MaxErrorsToSwitchChannel\":3,\"MaxErrorsToBadQuality\":3,\"TimeTryGoBackToPrimary\":300,\"IpAddress\":\"127.0.0.1\",\"Port\":80,\"WriteTimeout\":2000,\"ReadTimeout\":2000}";
                    nodeType = 17;
                    break;
                //Радио канал
                case 18:
                    jsonPropEnd = ",\"ChannelType\":\"Serial\",\"InterPollPause\":0,\"MaxErrorsToSwitchChannel\":3,\"MaxErrorsToBadQuality\":3,\"TimeTryGoBackToPrimary\":300,\"PortName\":\"COM1\",\"BaudRate\":115200,\"Parity\":0,\"StopBits\":1,\"WriteTimeout\":2000,\"ReadTimeout\":2000}";
                    nodeType = 18;
                    break;
                //Папка
                case 21:
                    jsonPropEnd = "";
                    nodeType = 21;
                    break;
                //PollingGroup
                case 22:
                    jsonPropEnd = ",\"Start\":0,\"Count\":0,\"Function\":0,\"UserData\":null}";
                    nodeType = 22;
                    break;
            }
            Objects newNode = new Objects
            {
                ParentId = idNodeToAdd,
                Type = nodeType,
                Name = newNodeName
            };
            context.Objects.Add(newNode);
            context.SaveChanges();
            //найдем ID узла, добавленного в таблицу Objects
            var newNodeId = newNode.Id;
            string jsonPropBegin = "{\"Id\":" + newNodeId + ",\"Name\":\"" + newNodeName + "\"";

            Property newNodeProps;
            //Если тип добавляемого узла - папка, то в таблице Properties строка с величиной будет пустая
            if (newNodeType == 21)
            {
                newNodeProps = new Property
                {
                    ObjectId = newNodeId,
                    PropId = 0,
                    Value = ""
                };
            }
            else
            {
                newNodeProps = new Property
                {
                    ObjectId = newNodeId,
                    PropId = 0,
                    Value = jsonPropBegin + jsonPropEnd
                };
            }
            context.Properties.Add(newNodeProps);
            context.SaveChanges();
            return newNodeId;
        }
        public void renameNode(int idRenameNode, string newNodeName, int typeProp)
        {
            var renameNode = context.Objects.Where(c => c.Id == idRenameNode).FirstOrDefault();
            renameNode.Name = newNodeName;
            //if (typeProp != 21)
            //{
            //    var renameNodeProps = context.Properties.Where(c => c.ObjectId == idRenameNode && c.PropId == 0).FirstOrDefault();
            //    var jsonProps = renameNodeProps.Value;
            //    var getArrOfProps = jsonProps.Split(',');
            //    var oldName = getArrOfProps[1];
            //    var newName = "\"Name\":\"" + newNodeName + "\"";
            //    var newjsonProps = jsonProps.Replace(oldName, newName);
            //    renameNodeProps.Value = newjsonProps;
            //}
            context.SaveChanges();
        }
        List<int> IdToDeleteList = new List<int>();
        List<Objects> obj;
        public List<int> deleteJsTreeNodeRoot(int parID)
        {
            //сюда собираем список Id  всех объектов, подлежащих удалению
            List<int> IdToDeleteList = new List<int>();
            Objects delElem = context.Objects.FirstOrDefault(o => o.Id == parID);
            if (delElem != null)
            {
                //получаем список объектов из таблицы Objects в глоб перем
                obj = context.Objects.Where(o => o.Id >= parID).ToList();
                IdToDeleteList = deleteJsTreeNode(parID);
            }
            return IdToDeleteList;
        }

        public List<int> deleteJsTreeNode(int parID)//сюда собираем список Id  всех объектов, подлежащих удалению
        {
            IdToDeleteList.Add(parID);
            var childs = obj.Where(c => c.ParentId == parID).Select(c => c).ToList();
            Objects currObj = obj.Where(o => o.Id == parID).FirstOrDefault();
            obj.Remove(currObj);

            foreach (var child in childs)
            {
                deleteJsTreeNode(child.Id);
            }
            return IdToDeleteList;
        }

        public void deleteJsTreeNodeBulk(List<int> IdList)//для быстрого удаления узлов. Использует EntityFramework.Extended
        {
            int n = 100;
            int iterationCount = IdList.Count / n;
            List<int> IdListSorted = IdList.OrderBy(i => i).ToList();

            for (int i = 0; i < iterationCount + 1; i++)
            {
                var some = IdListSorted.Skip(n * i).Take(100);
                context.Objects.Where(o => some.Contains(o.Id)).Delete();
                context.Properties.Where(p => some.Contains(p.ObjectId)).Delete();
            }
        }

        public List<int> findObjectNodes()//ищет все узлы типа контроллер
        {
            List<int> objects = new List<int>();
            using (var context = new EFDbContext())
            {
                var root = context.Objects.Where(c => c.Type == 5).Select(c => c).ToList();
                foreach (var item in root)
                {
                    objects.Add(item.Id);
                }
            }
            return objects;
        }

        int nodePaste;//Хранит Id корневого копируемого узла
        int baseId = 0;//базовое значение Id, полученное при вставке 1го копируемого элемента. От него будут отсчитываться последующие Id
        string oldControllerName = "";//для правки свойства Connection
        string newControllerName = "";//для правки свойства Connection
        int OPCIDglobal = 0;
        public string pasteNodeRoot(int idPasteParentElem, int idCopyParentElem, string newContName)
        {
            pasteObjects = new List<Objects>();
            pastePropertyList = new List<Property>();
            nodePaste = idPasteParentElem;
            copyObjects = context.Objects.Where(o => o.Id >= idCopyParentElem).AsNoTracking().ToList();//попытка хоть немного уменьшить количество объектов по которым идет перебор
            propertyList = context.Properties.Where(p => p.PropId == 0 && p.ObjectId >= idCopyParentElem).AsNoTracking().ToList();
            var rootCopy = copyObjects.Find(m => m.Id == idCopyParentElem);//корневой узел
            oldControllerName = rootCopy.Name;
            newControllerName = newContName;//название,которое ввел пользователь
            string Name = rootCopy.Name;//если узел конечный, то название оставим
            if (rootCopy.Type == 5 && newContName != "")//если же нет, то заменим на пользовательское
            {
                Name = newContName;
            }

            Property rootPropsCopy = propertyList.Find(p => p.ObjectId == idCopyParentElem);

            Objects newNode = new Objects()
            {
                Name = Name,
                Type = rootCopy.Type,
                ParentId = idPasteParentElem
            };

            context.Objects.Add(newNode);
            context.SaveChanges();
            baseId = newNode.Id;//От него будут отсчитываться последующие Id
            string newPropJson = "";
            if (rootPropsCopy.Value != "")
            {
                newPropJson = rootPropsCopy.Value;

                string newConnSrtId = getConnectionProp(idPasteParentElem, out OPCIDglobal).Trim('/');
                if (rootCopy.Type == 2)
                {

                    tagPropsForCopy = new TagProps();
                    tagPropsForCopy = (TagProps)json.Deserialize(rootPropsCopy.Value, tagPropsForCopy.GetType());
                    tagPropsForCopy.Opc = OPCIDglobal;
                    tagPropsForCopy.Connection = newConnSrtId;
                    newPropJson = json.Serialize(tagPropsForCopy);
                }

            }

            Property objProp = new Property
            {
                ObjectId = newNode.Id,
                PropId = 0,
                Value = newPropJson
            };
            context.Properties.Add(objProp);
            context.SaveChanges();
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            pasteNode(idCopyParentElem, newNode.Id);
            int countPasteNodes = pasteObjects.Count();
            context.Configuration.AutoDetectChangesEnabled = false;
            context.BulkInsert(pasteObjects);

            context.SaveChanges();
            context.BulkInsert(pastePropertyList);
            context.SaveChanges();

            //sw.Stop();

            return CreateJsTreeHelp(newNode.Id);
        }


        public void pasteNode(int copyNode, int newNode)
        {
            Objects rootCopy = copyObjects.Where(r => r.Id == copyNode).FirstOrDefault();
            var rootCopyChilds = copyObjects.Where(r => r.ParentId == copyNode).ToList();
            int childrenCount = rootCopyChilds.Count();
            for (int i = 0; i < childrenCount; i++)
            {
                int currentIdd = ++baseId;
                copyObjects.Remove(rootCopy);//типа оптимизирую
                //проверка чтобы избежать бесконечной вставки набора копируемых узлов.
                if (rootCopyChilds[i].ParentId == nodePaste)
                    return;

                Objects newPasteNode = new Objects()
                {
                    Id = currentIdd,
                    ParentId = newNode,
                    Type = rootCopyChilds[i].Type,
                    Name = rootCopyChilds[i].Name
                };
                pasteObjects.Add(newPasteNode);
                Property rootPropsCopy = propertyList.Where(p => p.ObjectId == rootCopyChilds[i].Id).FirstOrDefault();
                propertyList.Remove(rootPropsCopy);//типа оптимизирую
                string newPropJson = "";
                //если у узла есть свойства
                if (rootPropsCopy.Value != "")
                {
                    //поправим значение OpcID и Connection в строке свойств 
                    newPropJson = rootPropsCopy.Value;

                    if (rootCopyChilds[i].Type == 2)
                    {
                        tagPropsForCopy = new TagProps();
                        tagPropsForCopy = (TagProps)json.Deserialize(rootPropsCopy.Value, tagPropsForCopy.GetType());
                        string newConnStr = tagPropsForCopy.Connection;
                        string newConnStrRepl = newConnStr.Replace(oldControllerName, newControllerName);
                        tagPropsForCopy.Opc = OPCIDglobal;
                        tagPropsForCopy.Connection = newConnStrRepl;
                        newPropJson = json.Serialize(tagPropsForCopy);
                    }
                }
                //вставка строки со свойствами в таблицу свойств
                Property objProp = new Property
                {
                    ObjectId = currentIdd,
                    PropId = 0,
                    Value = newPropJson
                };
                pastePropertyList.Add(objProp);

                int nextParentId = 0;
                if (i == 0)//если это первый потомок, то просто наращиваем на 1 Id
                    nextParentId = newNode + 1;
                else // если же последующие, то присваиваем макс Id для родителя
                    nextParentId = pasteObjects.Last().Id;
                pasteNode(rootCopyChilds[i].Id, nextParentId);
            }
        }

        public bool CheckNodeExists(int Id)
        {
            var node = context.Objects.FirstOrDefault(n => n.Id == Id);
            if (node == null)
                return false;
            else
                return true;
        }

    }
}
