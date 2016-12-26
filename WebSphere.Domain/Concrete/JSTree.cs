using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Concrete
{
    public class JSTree : IJSTree
    {
        private readonly EFDbContext context = new EFDbContext();
        private static readonly JSON json = new JSON();
        public string CreateJsTree(int parID)
        {
            string childstring = "";


            using (var context1 = new EFDbContext())
            {
                var root = context1.Objects.Where(c => c.Id == parID).Select(c => c).FirstOrDefault();
                var childs = context1.Objects.Where(c => c.ParentId == parID).ToList();
                int counter = 0;
                foreach (var child in childs)
                {
                    if (counter != 0)
                        childstring += ",";
                    if (child.Id == 0) continue;
                    childstring += CreateJsTree(child.Id);
                    counter++;
                }

                return "{ " +
                      "'id' : '" + root.Id + "', " +
                      "'text': '" + root.Name + "', " +
                      "'icon': 'jstree-type_" + root.Type + "', " +
                "'children': [" + childstring + "] " +
                "}";
            }
        }

        string connProp = "";
        int OPCID;
        //получает строку подключения
        public string getConnectionProp(int idElemToPaste)
        {
            string connProp = "";
            using (var context = new EFDbContext())
            {
                var parent = context.Objects.Where(c => c.Id == idElemToPaste).FirstOrDefault();

                var olderParent = Convert.ToString(parent.ParentId);

                if (parent.Type != 1)
                {
                    connProp += getConnectionProp(Convert.ToInt32(olderParent));
                    return connProp += parent.Name + "/";

                }
                else
                {
                    if (parent.Type == 1)
                    {
                        OPCID = parent.Id;
                    }
                    return parent.Name + "/";
                }
            }

        }
        //дает ID OPC-сервера, к которому относится узел
        public int getOPCID()
        {
            return OPCID;
        }

        //Находит типы из таблицы ObjectTypes, не относящиеся к стандартным или типа, которые использует ядро
        public Dictionary<int, string> findObjForDefaultNode()
        {
            var defaultObj = new Dictionary<int, string>();
            var exceptionList=new List<int> {1, 2, 3, 4, 5, 7, 9, 10, 11,12,13,14,15,16,17,18,19,20,21,22};
            var sql = context.ObjectTypes.Where(m=>!exceptionList.Contains(m.Id)).Select(m => new { m.Id, m.Name }).ToList();
            foreach(var item in sql)
            {
                defaultObj.Add(item.Id, item.Name);
            }
            return defaultObj;
        }
        public void addNode(string newNodeName, int newNodeType, int idNodeToAdd, int defNodeObjType)
        {
            var nodeType = 0;


            string jsonPropEnd = "";
            switch (newNodeType)
            {
                //OPC-сервер
                case 1:
                    jsonPropEnd = ",\"Type\":\"\",\"Connection\":\"\",\"Connect\":false}";
                    nodeType = 1;
                    break;
                //КТПН дальний
                case 2:
                    jsonPropEnd = ",\"Address\":0,\"Driver\":\"\",\"RetrCount\":0,\"ParentGroup\":" + idNodeToAdd + ",\"PrimaryChannel\":0,\"SecondaryChannel\":0}";
                    nodeType = 7;
                    break;
                //АГЗУ
                case 3:
                    jsonPropEnd = ",\"Address\":0,\"Driver\":\"\",\"RetrCount\":0,\"ParentGroup\":" + idNodeToAdd + ",\"PrimaryChannel\":0,\"SecondaryChannel\":0}";
                    nodeType = 5;
                    break;
                ////БДР
                //case 4:
                //    jsonProp = "{\"Id\":\"\",\"Name\":\"\",\"Address\":0,\"Driver\":\"\",\"RetrCount\":0,\"ParentGroup\":0,\"PrimaryChannel\":0,\"SecondaryChannel\":0}";
                //    nodeType = 9;
                //    break;
                //Радио канал
                case 5:
                    jsonPropEnd = ",\"ChannelType\":\"Serial\",\"InterPollPause\":0,\"MaxErrorsToSwitchChannel\":3,\"MaxErrorsToBadQuality\":3,\"TimeTryGoBackToPrimary\":300,\"PortName\":\"COM1\",\"BaudRate\":115200,\"Parity\":0,\"StopBits\":1,\"WriteTimeout\":2000,\"ReadTimeout\":2000}";
                    nodeType = 18;
                    break;
                //GPRS канал
                case 6:
                    jsonPropEnd = ",\"ChannelType\":\"Tcp\",\"InterPollPause\":0,\"MaxErrorsToSwitchChannel\":3,\"MaxErrorsToBadQuality\":3,\"TimeTryGoBackToPrimary\":300,\"IpAddress\":\"127.0.0.1\",\"Port\":80,\"WriteTimeout\":2000,\"ReadTimeout\":2000}";
                    nodeType = 17;
                    break;
                //PollingGroup
                case 7:
                    jsonPropEnd = ",\"Start\":0,\"Count\":0,\"Function\":0}";
                    nodeType = 22;
                    break;

                //Тег
                case 8:
                    string ConnString = getConnectionProp(idNodeToAdd) + newNodeName;
                    jsonPropEnd = ",\"Opc\":" + OPCID + ",\"Connection\":\"" + ConnString + "\",\"Alarm_IsPermit\":false,\"HiHiText\":null,\"HiText\":null,\"NormalText\":null,\"LoText\":null,\"LoLoText\":null";
                    jsonPropEnd += ",\"HiHiSeverity\":null,\"HiSeverity\":null,\"LoSeverity\":null,\"LoLoSeverity\":null,\"ControllerType\":0,\"RealType\":0,\"Register\":\"\",\"AccessType\":0,";
                    jsonPropEnd += "\"Order\":1,\"InMin\":0,\"InMax\":1,\"OutMin\":1,\"OutMax\":1,\"IsSpecialTag\":false,\"History_IsPermit\":false,\"RegPeriod\":0,\"Deadbend\":0.1}";
                    nodeType = 2;
                    break;
                //Папка
                case 9:
                    jsonPropEnd = "";
                    nodeType = 21;
                    break;
                //Другое
                case 10:
                    jsonPropEnd = "}";
                    nodeType = defNodeObjType;
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
            var newNodeId = context.Objects.Select(c => c.Id).Max();
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
        }
        public void renameNode(int idRenameNode, string newNodeName, int typeProp)
        {
            var renameNode = context.Objects.Where(c => c.Id == idRenameNode).FirstOrDefault();
            renameNode.Name = newNodeName;
            if (typeProp != 21)
            {
                var renameNodeProps = context.Properties.FirstOrDefault(c => c.ObjectId == idRenameNode && c.PropId == 0);
                var jsonProps = renameNodeProps.Value;
                var getArrOfProps = jsonProps.Split(',');
                var oldName = getArrOfProps[1];
                var newName = "\"Name\":\"" + newNodeName + "\"";
                var newjsonProps = jsonProps.Replace(oldName, newName);
                renameNodeProps.Value = newjsonProps;
            }
            var Childnodes = context.Objects.Where(c => c.ParentId == idRenameNode && c.Type == 2).ToList();
            foreach (var child in Childnodes)
            { 
                var renameChildNodeProps = context.Properties.FirstOrDefault(c => c.ObjectId == child.Id && c.PropId == 0);
                if (renameChildNodeProps != null)
                {
              var jsonProps = renameChildNodeProps.Value;

                    var getArrOfProps = jsonProps.Split(',');
               /*   if (getArrOfProps.Contains();
                    var newName = "\"Name\":\"" + newNodeName + "\"";
                    var newjsonProps = jsonProps.Replace(oldName, newName);
                    renameChildNodeProps.Value = newjsonProps;
              
            */  }
            }
            context.SaveChanges();
        }
        public void deleteJsTreeNode(int parID)
        {
            using (var context = new EFDbContext())
            {

                //удаление объекта из табл Objects
                var root = context.Objects.Where(c => c.Id == parID).Select(c => c).FirstOrDefault();
                var childs = context.Objects.Where(c => c.ParentId == parID).Select(c => c);
                context.Objects.Remove(root);
                context.SaveChanges();
                //удаление связанных с объектом свойств из Properties
                var rootProp = context.Properties.Where(c => c.ObjectId == parID && c.PropId == 0).Select(c => c).FirstOrDefault();
                context.Properties.Remove(rootProp);
                context.SaveChanges();
                foreach (var child in childs)
                {
                    deleteJsTreeNode(child.Id);
                }
            }
            return;
        }


        int nodePaste;
        public void pasteNode(int idPasteParentElem, int idCopyParentElem)
        {


            nodePaste = idPasteParentElem;
            int nodeCopy = idCopyParentElem;
            int maxID1;
            //var OPCName = nameOPC;
            using (var context = new EFDbContext())
            {
                var rootCopy2 = context.Objects.Where(c => c.Id == nodeCopy).Select(c => c).FirstOrDefault();


                //узел, в который будет произведена вставка
                var nodePasteName = from o1 in context.Objects
                                    where o1.Id == nodePaste
                                    select o1.Name;
                //Извлечем копируемый корневой узел с его свойствами
                var rootCopy = from o1 in context.Objects
                           join o2 in context.Properties on o1.Id equals o2.ObjectId
                           where o1.Id == nodeCopy && o2.PropId == 0
                           select new
                           {
                               id = o1.Id,
                               name = o1.Name,
                               parentID = o1.ParentId,
                               type = o1.Type,
                               propValue = o2.Value
                           };
                //Преобразуем в список, чтобы не использвать foreach. В дальнейшем избавиться от листа
                var rootCopyNode = rootCopy.ToList();//корневой для копировани
                var pasteNode = nodePasteName.ToList();//узел для вставки
                //подготовим корневую сущность для вставки в таблицу Objects
                Objects obj = new Objects
                {
                    //id = maxID1++,
                    ParentId = idPasteParentElem,
                    Type = rootCopyNode[0].type,
                    Name = rootCopyNode[0].name
                };
                context.Objects.Add(obj);
                context.SaveChanges();

                string newPropJson;
                //найдем ID только что вставленного корневого элемента
                maxID1 = context.Objects.Select(c => c.Id).Max();
                if (rootCopy2.Type == 21)
                {
                    newPropJson = "";
                }
                else
                {
                    //распилим свойства корневого вставляемого элемента
                    var getNewPropArr = rootCopyNode[0].propValue.Split(',');

                    var oldID = getNewPropArr[0];
                    var newID = "{\"Id\":\"" + maxID1 + "\"";
                    //Заменим ID на актуальный в свойствах
                    newPropJson = rootCopyNode[0].propValue.Replace(oldID, newID);
                    if (rootCopy2.Type == 2)
                    {
                        string newConnSrt = getConnectionProp(idPasteParentElem) + rootCopy2.Name;
                        var oldConnection = getNewPropArr[3];
                        var newConnection = "\"Connection\":\"" + newConnSrt + "\"";
                        var oldOPCID = getNewPropArr[2];
                        var newOPCID = "\"Opc\":\"" + OPCID + "\"";
                        newPropJson = newPropJson.Replace(oldOPCID, newOPCID);
                        newPropJson = newPropJson.Replace(oldConnection, newConnection);

                    }
                }

                Property objProp = new Property
                {
                    ObjectId = maxID1,
                    PropId = 0,
                    Value = newPropJson
                };
                context.Properties.Add(objProp);
                context.SaveChanges();

            }
            pasteJsTreeNodes(nodeCopy, maxID1);
        }

        int maxIdForProp;
        public void pasteJsTreeNodes(int parID, int parIDCopy)
        {
            using (var context = new EFDbContext())
            {
                //Достанем самый верхний копируемый узел
                var root = context.Objects.Where(c => c.Id == parID).Select(c => c).FirstOrDefault();

                //var childs1 = context.Objects.Where(c => c.ParentId == parID).Select(c => c);
                //Извлечем все дочерние узлы
                var childs = from o1 in context.Objects
                             join o2 in context.Properties on o1.Id equals o2.ObjectId
                             where o1.ParentId == parID && o2.PropId == 0
                             select new
                             {
                                 id = o1.Id,
                                 name = o1.Name,
                                 parentID = o1.ParentId,
                                 type = o1.Type,
                                 propValue = o2.Value
                             };

                int counter = 0;

                foreach (var child in childs)
                {
                    //сохранить в базу строку с ID нового узла и его родительским узлом
                    //а еще скопировать свойства из Properties, но учесть, что в JSON свойствах есть ID старого элемента и его надо менять
                    //сделать запрос на макс используемый ID в базе

                    using (var context1 = new EFDbContext())
                    {
                        //проверка чтобы избежать бесконечной вставки набора копируемых узлов.
                        // то есть 
                        if (child.parentID == nodePaste)
                            return;
                        //Добавление записи в табл Object
                        Objects obj1 = new Objects
                        {
                            ParentId = parIDCopy,
                            Type = child.type,
                            Name = child.name
                        };
                        context1.Objects.Add(obj1);
                        context1.SaveChanges();

                        //ID только что вставленного элемента
                        using (var context2 = new EFDbContext())
                        {
                            maxIdForProp = context2.Objects.Select(c => c.Id).Max();
                        }
                        string newPropJson = "";
                        //если тип узла- папка, то редактировать json свойство не нужно и посылаем пустую строку
                        if (child.type == 13)
                        {
                            newPropJson = "";
                        }
                        else
                        {
                            //поправим значение ID в строке свойств 
                            var getNewIDArr = child.propValue.Split(',');
                            var oldID = getNewIDArr[0];
                            var newID = "{\"Id\":\"" + maxIdForProp + "\"";
                            newPropJson = child.propValue.Replace(oldID, newID);

                            if (child.type == 2)
                            {
                                string newConnSrt = getConnectionProp(parIDCopy) + child.name;
                                var oldConnection = getNewIDArr[3];
                                var newConnection = "\"Connection\":\"" + newConnSrt + "\"";
                                newPropJson = newPropJson.Replace(oldConnection, newConnection);
                                var oldOPCID = getNewIDArr[2];
                                var newOPCID = "\"Opc\":\"" + OPCID + "\"";
                                newPropJson = newPropJson.Replace(oldOPCID, newOPCID);
                            }

                        }

                        //вставка строки со свойствами в таблицу свойств
                        Property objProp = new Property
                        {
                            ObjectId = maxIdForProp,
                            PropId = 0,
                            Value = newPropJson
                        };
                        context1.Properties.Add(objProp);
                        context1.SaveChanges();
                        //Сделала новый Using, потому что возникала ошибка. Не знаю верное ли решение
                        //{System.Data.Entity.Core.EntityException: An error occurred while starting a transaction on the provider connection. See the inner exception for details. --->
                        //System.InvalidOperationException:
                        //Существует назначенный этой команде Command открытый DataReader, который требуется предварительно закрыть.
                        //using (var db2 = new EFDbContext())
                        //{
                        //    maxID = db2.Objects.Select(c => c.Id).Max();
                        //    maxID2 =maxID;
                        //}
                    }
                    pasteJsTreeNodes(child.id, maxIdForProp);
                    counter++;
                }
            }
        }

    }
}
