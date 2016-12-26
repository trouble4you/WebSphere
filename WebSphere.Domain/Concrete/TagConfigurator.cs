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
        public int GetPropType(int id)
        {

            var sel2 = context.Objects.Where(c => c.Id == id).Select(c => c.Type).FirstOrDefault();
            return sel2;
        }

        //public Dictionary<string, string> getListProps(int id)
        //{
        //    var jss = new JavaScriptSerializer();
        //}

        public void deleteProps(int id, List<string> deletePropsArr)
        {
            var node = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var props = node.Value;
            foreach (string item in deletePropsArr)
            {
                string prop = "," + item;
                int removePropIndex = props.IndexOf(prop);
                var removePropLength = item.Length + 1;
                props = props.Remove(removePropIndex, removePropLength);
            }
            node.Value = props;
            context.SaveChanges();
        }

        public void deleteProp(int id, string propForDelete)
        {
            //int endInd;
            //int startInd;
            var item = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var props = item.Value;
            //var helpSearchStr = "\":\"" + propName + "\",\"";
            var helpSearchStr = "," + propForDelete;
            var removePropIndex = props.IndexOf(helpSearchStr);
            var removePropLength = helpSearchStr.Length;
            var newProps = props.Remove(removePropIndex, removePropLength);
            //var isAdditionalProp = props.IndexOf(helpSearchStr);
            ////если это дополнительно добавленное свойство
            //if (isAdditionalProp != -1)
            //{
            //    var helpSearchStr2 = "},{";
            //    var isManyAdditionalProp = props.IndexOf(helpSearchStr2);
            //    //если есть еще дополнительные свойства
            //    if (isManyAdditionalProp != -1)
            //    {

            //        var endIndexHelp = props.IndexOf(helpSearchStr2, isAdditionalProp);
            //        //если это последнее из нескольких добавленных свойств
            //        if (endIndexHelp == -1)
            //        {
            //            //startInd = isAdditionalProp - 6;
            //            //endInd = props.Length - 1;
            //            startInd = isAdditionalProp - 7;
            //            endInd = props.Length - 2;
            //        }
            //        else
            //        {
            //            startInd = isAdditionalProp - 6;
            //            endInd = endIndexHelp + 2;
            //            //var newProps = props.Remove(startInd, endInd - startInd);
            //        }


            //    }
            //    else
            //    {
            //        startInd = isAdditionalProp - 19;
            //        endInd = props.Length - 1;

            //    }
            //}
            //else
            //{
            //    startInd = props.IndexOf(propName) - 1;
            //    int endIndHelp = props.IndexOf(",", startInd);

            //    //если это не последнее свойство по умолчанию
            //    if (endIndHelp != -1)
            //    {
            //        endInd = endIndHelp - 1;
            //    }
            //    else
            //    {
            //        endInd = props.IndexOf("}") - 1;
            //    }


            //}
            //var newProps = props.Remove(startInd, endInd - startInd);
            item.Value = newProps;
            context.SaveChanges();
        }

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

        public int? getParentGroup(int id)
        {
            int? parentGroup = context.Objects.Where(c => c.Id == id).FirstOrDefault().ParentId;
            return parentGroup;

        }

        public Dictionary<int, string> getChannels()
        {
            var channels = context.Objects.Where(c => c.Type == 17 || c.Type == 18).ToList();
            Dictionary<int, string> ChannelsDictionary = new Dictionary<int, string>();
            if(channels.Count>0)
            {
                foreach (var item in channels)
                {
                    ChannelsDictionary.Add(item.Id, item.Name);
                }
            }
            return ChannelsDictionary;
        }

        public void addProp(string newProp, int nodeId)
        {
            var props = context.Properties.Where(c => c.ObjectId == nodeId && c.PropId == 0).FirstOrDefault().Value;

            //var IsAdditionalProps = props.IndexOf("newProps");
            var insertPoint = props.LastIndexOf('}');

            props = props.Insert(insertPoint, newProp);
            // }
            var newPropsObj = context.Properties.Where(c => c.ObjectId == nodeId && c.PropId == 0).FirstOrDefault();
            newPropsObj.Value = props;
            context.SaveChanges();
        }

        //public void addUserProp (AddUserPropModel model)

        //public void addUserProp(string newProp, int nodeId)
        //{
        //    var props = context.Properties.Where(c => c.ObjectId == nodeId && c.PropId == 0).FirstOrDefault().Value;

        //    //var IsAdditionalProps = props.IndexOf("newProps");
        //    var insertPoint = props.LastIndexOf('}');

        //    props = props.Insert(insertPoint, newProp);
        //    // }
        //    var newPropsObj = context.Properties.Where(c => c.ObjectId == nodeId && c.PropId == 0).FirstOrDefault();
        //    newPropsObj.Value = props;
        //    context.SaveChanges();
        //}

        public void addProp(string propName, string propValue, int propType, int nodeId)
        {
            var props = context.Properties.Where(c => c.ObjectId == nodeId && c.PropId == 0).FirstOrDefault().Value;

            //var IsAdditionalProps = props.IndexOf("newProps");
            var insertPoint = props.LastIndexOf('}');
            //если свойство пользовательское
            //if(userOrStandart==0)
            //{

            string newPropSubStr = "";
            ////если дополнительных свойств нет
            ////if (IsAdditionalProps == -1)
            ////{
            //    var insertPoint = props.LastIndexOf('}');
            //    if (propType == 1)
            //    {
            //        //int propValueInt = Convert.ToInt32(propValue);
            //        newPropSubStr = ",\"newProps\":[{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValue + "\"}]";
            //    }

            //    if (propType == 2)
            //    {

            //        //float propValueFloat = float.Parse(propValue);
            //        newPropSubStr = ",\"newProps\":[{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValue + "\"}]";
            //    }
            //    if (propType == 3)
            //    {
            //        //double propValueDouble = double.Parse(propValue);
            //        newPropSubStr = ",\"newProps\":[{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValue + "\"}]";
            //    }
            //    if (propType == 4)
            //    {
            //        //int propValueBool = Convert.ToInt32(propValue);
            //        newPropSubStr = ",\"newProps\":[{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValue + "\"}]";
            //    }
            //    if (propType == 5)
            //    {
            //        newPropSubStr = ",\"newProps\":[{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValue + "\"}]";
            //    }
            //    props = props.Insert(insertPoint, newPropSubStr);
            // }

            //else
            //{
            //var insertPoint = props.LastIndexOf("]}");
            if (propType == 1)
            {
                int propValueInt = Convert.ToInt32(propValue);
                //newPropSubStr = ",{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValueInt + "\"}";
                newPropSubStr = ",\"" + propName + "\":" + propValueInt;
            }

            if (propType == 2)
            {
                //float propValueInt = Convert.ToSingle(propValue);
                float propValueFloat = float.Parse(propValue);
                //newPropSubStr = ",{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValueFloat + "\"}";
                newPropSubStr = ",\"" + propName + "\":" + propValueFloat;
            }
            if (propType == 3)
            {
                double propValueDouble = double.Parse(propValue);
                //newPropSubStr = ",{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValueDouble + "\"}";
                newPropSubStr = ",\"" + propName + "\":" + propValueDouble;
            }
            if (propType == 4)
            {
                int propValueBool = Convert.ToInt32(propValue);
                //newPropSubStr = ",{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValueBool + "\"}";
                newPropSubStr = ",\"" + propName + "\":" + propValueBool;
            }
            if (propType == 5)
            {
                //newPropSubStr = ",{\"Name\":\"" + propName + "\",\"Type\":" + propType + ",\"Value\":\"" + propValue + "\"}";
                newPropSubStr = ",\"" + propName + "\":\"" + propValue + "\"";
            }

            props = props.Insert(insertPoint, newPropSubStr);
            // }
            var newPropsObj = context.Properties.Where(c => c.ObjectId == nodeId && c.PropId == 0).FirstOrDefault();
            newPropsObj.Value = props;
            context.SaveChanges();
            //}
            ////если свойство стандартное
            //else
            //{
            // //если нет дополнительных свойств
            //                     string insertPropValue= "";
            //        if (propType == 1)
            //        {
            //            int propValueInt = Convert.ToInt32(propValue);
            //            insertPropValue= ",\""+propName+"\":"+propValueInt;
            //        }
            //        if (propType == 2)
            //        {
            //            //float propValueInt = Convert.ToSingle(propValue);
            //            float propValueFloat = float.Parse(propValue);
            //            insertPropValue= ",\""+propName+"\":"+propValueFloat;
            //        }
            //        if (propType == 3)
            //        {
            //            double propValueDouble = double.Parse(propValue);
            //            insertPropValue= ",\""+propName+"\":"+propValueDouble;
            //        }
            //        if (propType == 4)
            //        {
            //            int propValueBool = Convert.ToInt32(propValue);
            //            insertPropValue= ",\""+propName+"\":"+propValueBool;
            //        }
            //        if (propType == 5)
            //        {
            //            insertPropValue= ",\""+propName+"\":\""+propValue+"\"";
            //        }
            // if (IsAdditionalProps == -1)
            // {
            //     //здесь думаю надо типизированно сохранить свойства
            //     props = props.Insert((props.Length - 2), insertPropValue);
            // }
            // else
            // {
            //     props=props.Insert(IsAdditionalProps-2, insertPropValue);
            // }


            // }
        }


        public bool checkExistingNodeName(string Name)
        {
            var sel = context.Objects.Where(c => c.Name == Name).FirstOrDefault();
            if (sel != null)
                return true;
            else
                return false;

        }
        //public bool checkExistingPropName(string Name)
        //{
        //    var sel = context.Objects.Where(c => c.Name == Name).FirstOrDefault();
        //    if (sel != null)
        //        return false;
        //    else
        //        return true;
        //}


        public void saveNoTypesProps(NoTypesProps noTypesProps, int id)
        {
            var serProps = json.Serialize(noTypesProps);
            int indexSpecial = serProps.IndexOf(",\"special\"");
            int endIndex = serProps.Length - 1;
            int countForRemove = endIndex - indexSpecial;
            var newProps = serProps.Remove(indexSpecial, countForRemove);
            var additionalProps = noTypesProps.special;
            var newProps2 = newProps.Insert(indexSpecial, additionalProps);
            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = newProps2;
            context.SaveChanges();
        }




        public void saveNoTypesProps1(NoTypesPropsHelp model)
        {
            var propsStdModel = model.notypesModel;
            var propsUserModel = model.notypesforSave;
            string stdProps = "{\"Id\":" + model.notypesforSave.Id + ",\"Name\":\"" + model.notypesforSave.Name + "\"";
            string userProps = "";
            if(propsStdModel!= null)
            {
   
            PropertyInfo[] propertiesInfo = propsStdModel.GetType().GetProperties();
                //string stdProps = "{\"Id\":" + model.notypesforSave.Id + ",\"Name\":\"" + model.notypesforSave.Name + "\",";
                //проверку на то, существует ли свойство модели, т.е. наполнена ли она, чтобы обойти нуллбл
                   foreach (PropertyInfo pi in propertiesInfo)
                   {
                       //типа можно заменить на findAll....и оно само будет заключать строковые значения в кавычки
                       var value = pi.GetValue(model.notypesModel);
                       if (value != null)
                       {
                           string propForSave = "";
                           var type = (pi.PropertyType).ToString();
                   
                           if (type == "System.String")
                           {
                               propForSave = ",\"" + pi.Name + "\":\"" + value + "\"";
                           }
                           else if (type == "System.Nullable`1[System.Boolean]")
                           {
                               propForSave = ",\"" + pi.Name + "\":" + value.ToString().ToLower();
                           }
                           else
                           {
                               propForSave = ",\"" + pi.Name + "\":" + value;
                           }
                   
                           stdProps += propForSave;
                       }
                   }
                //var serUserProps = json.Serialize(propsUserModel);
                //var stdPropsLength = stdProps.Length;
                //stdProps = stdProps.Remove(stdPropsLength - 1);
                //userProps = model.notypesforSave.special + "}";
          }
            //else
            //{
            if (model.notypesforSave.special != null)
                userProps = (model.notypesforSave.special) + "}";
            else
                userProps = "}";
                //stdProps1 = "{";
            //}

            string total = stdProps + userProps;
            var sel = context.Properties.Where(c => c.ObjectId == model.notypesforSave.Id && c.PropId == 0).FirstOrDefault();
            sel.Value = total;
            context.SaveChanges();
            //var serStdProps = json.Serialize(propsStdModel);
        }
        public void saveObjectProps(ObjectProps objectProps, int id)
        {
            var serProps = json.Serialize(objectProps);
            int indexOfUserProps = serProps.IndexOf(",\"special");
            var standartProps = serProps.Remove(indexOfUserProps);
            var userProps = objectProps.special;
            var totalProps = standartProps + userProps + "}";

            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = totalProps;
            context.SaveChanges();
        }

        public void saveTagProps(TagProps tagProps, int id)
        {
            var serProps = json.Serialize(tagProps);
            int indexOfUserProps = serProps.IndexOf(",\"special");
            var standartProps = serProps.Remove(indexOfUserProps);
            var userProps = tagProps.special;
            var totalProps = standartProps + userProps + "}";
            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = totalProps;
            context.SaveChanges();
        }
        public void saveOPCProps(OPCProps opcProps, int id)
        {
            var serProps = json.Serialize(opcProps);
            int indexOfUserProps = serProps.IndexOf(",\"special");
            var standartProps = serProps.Remove(indexOfUserProps);
            var userProps = opcProps.special;
            var totalProps = standartProps + userProps + "}";

            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = totalProps;
            context.SaveChanges();
        }

        public void savePollingGroupProps(PollingGroupProps pollingGroupProps, int id)
        {
            var serProps = json.Serialize(pollingGroupProps);

            int indexOfUserProps = serProps.IndexOf(",\"special");
            var standartProps = serProps.Remove(indexOfUserProps);
            var userProps = pollingGroupProps.special;
            var totalProps = standartProps + userProps + "}";

            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = totalProps;
            context.SaveChanges();
        }
        public void saveRadioChannelProps(RadioChannelProps radioChannelProps, int id)
        {
            var serProps = json.Serialize(radioChannelProps);
            int indexOfUserProps = serProps.IndexOf(",\"special");
            var standartProps = serProps.Remove(indexOfUserProps);
            var userProps = radioChannelProps.special;
            var totalProps = standartProps + userProps + "}";
            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = totalProps;
            context.SaveChanges();
        }
        public void saveGPRSChannelProps(GPRSChannelProps gprsChannelProps, int id)
        {
            var serProps = json.Serialize(gprsChannelProps);
            int indexOfUserProps = serProps.IndexOf(",\"special");
            var standartProps = serProps.Remove(indexOfUserProps);
            var userProps = gprsChannelProps.special;
            var totalProps = standartProps + userProps + "}";

            var sel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            sel.Value = totalProps;
            context.SaveChanges();
        }

        //public AddStandartPropModelHelp DeserModel(string model)
        //{

        //}
        public bool checkUserStrProp (string checkStr)
        {
            try
            {
                string Value = "{\"prop\":\"" + checkStr+"\"}";
                var help = new Dictionary<string, string>();
                var prop = json.Deserialize(Value, help.GetType());
                help = (Dictionary<string, string>)prop;

            }
            catch
            {
                return false;
            }
            return true;

        }

        public Dictionary<string, string> getCommonProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var commonProps = new Dictionary<string, string>();
            //var commonProps1 = new Dictionary<string, dynamic>();
            string JSONprop = sel1.Value;
            var props = json.Deserialize(JSONprop, commonProps.GetType());
            //var props1 = json.Deserialize(JSONprop, commonProps1.GetType());
            commonProps = (Dictionary<string, string>)props;
            //commonProps1 = (Dictionary<string, dynamic>)props1;
            return commonProps;
        }

        //public Dictionary<string, dynamic> getUserProps(int id)
        //{
        //    var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
        //    var commonProps1 = new Dictionary<string, dynamic>();
        //    string JSONprop = sel1.Value;
        //    var props1 = json.Deserialize(JSONprop, commonProps1.GetType());
        //    commonProps1 = (Dictionary<string, dynamic>)props1;
        //    List<string> propsNames = new List<string>{

        //    "special","Id","Name","Opc","Connection", "Alarm_IsPermit","HiHiText","HiText","NormalText","LoText", "LoLoText", "HiHiSeverity", "HiSeverity","LoSeverity",
        //    "LoLoSeverity", "ControllerType","RealType", "RealType","Register","AccessType", "Order","InMin", "InMax", "OutMin", "OutMax",
        //    "History_IsPermit","RegPeriod",  "Deadbend", "IsSpecialTag", "ChannelType", "InterPollPause", "MaxErrorsToSwitchChannel",
        //    "MaxErrorsToBadQuality", "TimeTryGoBackToPrimar","IpAddress", "Port", "ReadTimeout", "WriteTimeout","PortName", "BaudRate","Parity",
        //    "StopBits", "Address","Driver", "RetrCount", "ParentGroup","PrimaryChannel", "SecondaryChannel","Start","Count", "Function", "Type","Connect"
        //    };
        //    var commonProps2 = new Dictionary<string, dynamic>();
        //    foreach (var item in commonProps1)
        //    {
        //        if (propsNames.IndexOf(item.Key) == -1)
        //        {
        //            commonProps2.Add(item.Key, item.Value);
        //        }
        //    }
        //    return commonProps2;
        //}


        public List<ErrorUserProp> checkUserPropsValidity(string propsString)
        {
            var test = new Dictionary<string, string>();
            var test2 = new List<ErrorUserProp>();
            if (!string.IsNullOrEmpty(propsString))
            {
                var testSer = new Dictionary<string, dynamic>();
                //List<ErrorUserProp> propsObj = new List<ErrorUserProp>();
                //List<NewProp> test1 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NewProp>>(propsString);
                var propObjDeser = json.Deserialize(propsString, testSer.GetType());
                testSer = (Dictionary<string, dynamic>)propObjDeser;
                //Dictionary<int, string> standartPropTypes = new Dictionary<int, string>
                //    { {1,"System.Byte"},
                //    {2, "System.UInt16"},
                //    {3, "System.UInt32"},
                //    {4, "System.SByte"},
                //    {5, "System.Int16"},
                //    {6, "System.Int32"},
                //    {7, "System.Single"},
                //    {8, "System.Double"},
                //    {9, "System.Boolean"},
                //    {10, "System.String"} };
                //var listlist = testSer.Keys;
                foreach (var item in testSer)
                {
                    //var typeName = standartPropTypes[item.Type];
                    var propValue = item.Value;
                    string propKey = item.Key;

                    var typeNameIndex = propKey.LastIndexOf("_");
                    var typeName = propKey.Substring(typeNameIndex + 1);
                    //int typeProp = 0;
                    //int typeProp = 0;

                    switch (typeName)
                    {
                        case "byte":
                            {
                                try
                                {
                                    System.Byte Prop = Convert.ToByte(propValue);
                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    error.Value = item.Value;
                                    error.Description = "число не соотвествует типу byte";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу byte");
                                }
                                break;

                            }
                        case "word":
                            {
                                try
                                {
                                    System.UInt16 Prop = Convert.ToUInt16(propValue);
                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    error.Value = item.Value;
                                    error.Description = "число не соотвествует типу word";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу word");
                                }
                                break;

                            }
                        case "dword":
                            {
                                try
                                {
                                    System.UInt32 Prop = Convert.ToUInt32(propValue);
                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    error.Value = item.Value;
                                    error.Description = "число не соотвествует типу dword";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу dword");
                                }
                                break;

                            }
                        case "shortInt":
                            {
                                try
                                {
                                    System.SByte Prop = Convert.ToSByte(propValue);
                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    error.Value = item.Value;
                                    error.Description = "число не соотвествует типу shortInt";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу shortInt");
                                }
                                break;

                            }
                        case "smallInt":
                            {
                                try
                                {
                                    System.Int16 Prop = Convert.ToInt16(propValue);
                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    error.Value = item.Value;
                                    error.Description = "число не соотвествует типу smallInt";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу smallInt");
                                }
                                break;

                            }
                        case "longInt":
                            {
                                try
                                {
                                    System.Int32 Prop = Convert.ToInt32(propValue);
                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    error.Value = item.Value;
                                    error.Description = "число не соотвествует типу longInt";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу longInt");
                                }
                                break;

                            }
                        case "float":
                            {
                                try
                                {
                                    System.Single Prop = Convert.ToSingle(propValue);

                                    if (Prop.ToString() != propValue.ToString())
                                    {
                                        float dd = propValue;
                                    }
                                    //Type type=propValue.GetType();
                                    ////if (type == typeof(float))
                                    ////{
                                    ////    float dd = propValue;
                                    ////}
                                    //double maxVal = 3.40282347E+38;
                                    //double minVal = -3.40282347E+38;
                                    //if (propValue < maxVal && propValue > minVal)
                                    //{
                                    //    float dd = propValue;
                                    //}

                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    var intermediateVal = item.Value.ToString().Replace(".", ",");
                                    error.Value = intermediateVal;
                                    error.Description = "число не соотвествует типу float";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу float");
                                }
                                break;

                            }
                        case "double":
                            {
                                try
                                {
                                    System.Double Prop = Convert.ToDouble(propValue);
                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    error.Value = item.Value;
                                    error.Description = "число не соотвествует типу double";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу double");
                                }
                                break;

                            }
                        case "bool":
                            {
                                try
                                {
                                    System.Boolean Prop = Convert.ToBoolean(propValue);
                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    error.Value = item.Value;
                                    error.Description = "число не соотвествует типу bool";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу bool");
                                }
                                break;

                            }
                        case "string":
                            {
                                try
                                {
                                    System.String Prop = Convert.ToString(propValue);
                                }
                                catch
                                {
                                    var error = new ErrorUserProp();
                                    error.Name = item.Key;
                                    error.Value = item.Value;
                                    error.Description = "число не соотвествует типу string";
                                    test2.Add(error);
                                    test.Add(item.Key, "число не соотвествует типу string");
                                }
                                break;

                            }

                    }

                }

            }

            return test2;
        }
        //public Dictionary<string, string> getCommonProps(int id)
        //{
        //    var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
        //    var commonProps = new Dictionary<string, string>();
        //    //var commonProps1 = new Dictionary<string, dynamic>();
        //    string JSONprop = sel1.Value;
        //    var props = json.Deserialize(JSONprop, commonProps.GetType());
        //    //var props1 = json.Deserialize(JSONprop, commonProps1.GetType());
        //    commonProps = (Dictionary<string, string>)props;
        //    //commonProps1 = (Dictionary<string, dynamic>)props1;
        //    return commonProps;
        //}

        public Dictionary<string, dynamic> getUserPropsAfterValidate(string jsonStr)
        {

            string userPropsStrHelp ="{"+ jsonStr.Remove(0, 1)+"}";
            var commonProps1 = new Dictionary<string, dynamic>();

            var props1 = json.Deserialize(userPropsStrHelp, commonProps1.GetType());
            commonProps1 = (Dictionary<string, dynamic>)props1;

            return commonProps1;
        }

        public Dictionary<string, dynamic> getUserProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var commonProps1 = new Dictionary<string, dynamic>();
            string JSONprop = sel1.Value;
            var props1 = json.Deserialize(JSONprop, commonProps1.GetType());
            commonProps1 = (Dictionary<string, dynamic>)props1;
            List<string> propsNames = new List<string>{
            
            "special","Id","Name","Opc","Connection", "Alarm_IsPermit","HiHiText","HiText","NormalText","LoText", "LoLoText", "HiHiSeverity", "HiSeverity","LoSeverity",
            "LoLoSeverity", "ControllerType","RealType", "RealType","Register","AccessType", "Order","InMin", "InMax", "OutMin", "OutMax",
            "History_IsPermit","RegPeriod",  "Deadbend", "IsSpecialTag", "ChannelType", "InterPollPause", "MaxErrorsToSwitchChannel",
            "MaxErrorsToBadQuality", "TimeTryGoBackToPrimary","IpAddress", "Port", "ReadTimeout", "WriteTimeout","PortName", "BaudRate","Parity",
            "StopBits", "Address","Driver", "RetrCount", "ParentGroup","PrimaryChannel", "SecondaryChannel","Start","Count", "Function", "Type","Connect"
            };
            var commonProps2 = new Dictionary<string, dynamic>();
            foreach (var item in commonProps1)
            {
                if (propsNames.IndexOf(item.Key) == -1)
                {
                    commonProps2.Add(item.Key, item.Value);
                }
            }
            return commonProps2;
        }

        public Dictionary<string, dynamic> getStandartProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var commonProps1 = new Dictionary<string, dynamic>();
            string JSONprop = sel1.Value;
            var props1 = json.Deserialize(JSONprop, commonProps1.GetType());
            commonProps1 = (Dictionary<string, dynamic>)props1;
            List<string> propsNames = new List<string>{
            
            "special","Id","Name","Opc","Connection", "Alarm_IsPermit","HiHiText","HiText","NormalText","LoText", "LoLoText", "HiHiSeverity", "HiSeverity","LoSeverity",
            "LoLoSeverity", "ControllerType","RealType", "RealType","Register","AccessType", "Order","InMin", "InMax", "OutMin", "OutMax",
            "History_IsPermit","RegPeriod",  "Deadbend", "IsSpecialTag", "ChannelType", "InterPollPause", "MaxErrorsToSwitchChannel",
            "MaxErrorsToBadQuality", "TimeTryGoBackToPrimary","IpAddress", "Port", "ReadTimeout", "WriteTimeout","PortName", "BaudRate","Parity",
            "StopBits", "Address","Driver", "RetrCount", "ParentGroup","PrimaryChannel", "SecondaryChannel","Start","Count", "Function", "Type","Connect"
            };
            var commonProps2 = new Dictionary<string, dynamic>();
            foreach (var item in commonProps1)
            {
                if (propsNames.IndexOf(item.Key) != -1)
                {
                    commonProps2.Add(item.Key, item.Value);
                }
            }
            return commonProps2;
        }

        public Dictionary<string, string> getCommonProps1(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var commonProps = new Dictionary<string, string>();
            string JSONprop = sel1.Value;
            var props = json.Deserialize(JSONprop, commonProps.GetType());
            commonProps = (Dictionary<string, string>)props;
            return commonProps;
        }

        public NoTypeNodeHelp getNoTypeNodeProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var noTypesProps = new NoTypeNodeHelp();
            string JSONprop = sel1.Value;
            var props = json.Deserialize(JSONprop, noTypesProps.GetType());
            noTypesProps = (NoTypeNodeHelp)props;
            return noTypesProps;

            //var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            //var noTypesProps = new NoTypeNodeHelp();
            //string JSONprop = sel1.Value;
            //var props = json.Deserialize(JSONprop, noTypesProps.GetType());
            //noTypesProps = (NoTypeNodeHelp)props;
            //return noTypesProps;
        }
        public NoTypesProps getNoTypeNodePropsIdName(int id)
        {
            var sel = context.Objects.Where(m => m.Id == id).Select(m => m.Name).FirstOrDefault();
            var noTypesNodeIdNane = new NoTypesProps();
            noTypesNodeIdNane.Id = id;
            noTypesNodeIdNane.Name = sel;
            return noTypesNodeIdNane;

        }
        public TagProps getTagProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var tagProps = new TagProps();
            string JSONprop = sel1.Value;
            var props = json.Deserialize(JSONprop, tagProps.GetType());
            tagProps = (TagProps)props;
            return tagProps;
        }

        public RadioChannelProps getRadioChannelProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var Props = new RadioChannelProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (RadioChannelProps)propsDes;
            return Props;
        }

        public GPRSChannelProps getGPRSChannelProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var Props = new GPRSChannelProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (GPRSChannelProps)propsDes;
            return Props;
        }
        public ObjectProps getObjectProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var Props = new ObjectProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (ObjectProps)propsDes;
            return Props;
        }
        public PollingGroupProps getPollingGroupProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var Props = new PollingGroupProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (PollingGroupProps)propsDes;
            return Props;
        }
        public OPCProps getOPCProps(int id)
        {
            var sel1 = context.Properties.Where(c => c.ObjectId == id && c.PropId == 0).FirstOrDefault();
            var Props = new OPCProps();
            string JSONprop = sel1.Value;
            var propsDes = json.Deserialize(JSONprop, Props.GetType());
            Props = (OPCProps)propsDes;
            return Props;
        }
        public List<moduleCondition> GetConnectedModules()
        {
            var moduleConnectedList = new List<moduleCondition>();
            var sel1 = from o1 in context.Properties
                       where o1.PropId == 100 && o1.Value == "1"
                       select o1.ObjectId;
            var sel2 = from o1 in context.Properties
                       join o2 in context.Objects on o1.ObjectId equals o2.Id
                       where o1.PropId == 101 && sel1.Contains(o1.ObjectId)
                       select new
                       {

                           modId = o1.ObjectId,
                           modRun = o1.Value,
                           modName = o2.Name
                       };
            var a = sel2.Any();
            if (a)
            {
                foreach (var item in sel2)
                {
                    var result = new moduleCondition();
                    result.idModule = item.modId;
                    result.nameModule = item.modName;
                    result.isRun = item.modRun;
                    moduleConnectedList.Add(result);
                }
            }
            else
            {
                var result = new moduleCondition();
                moduleConnectedList.Add(result);
            }
            return moduleConnectedList;
        }

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
            var modForDel = context.Properties.Where(c => c.ObjectId == id && c.PropId == 100).FirstOrDefault();
            modForDel.Value = "0";
            var modForDelRun = context.Properties.Where(c => c.ObjectId == id && c.PropId == 101).FirstOrDefault();
            modForDel.Value = "0";
            context.SaveChanges();
        }

        public void changeStatus(int id, string moduleStatus)
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

        public void AddModule(List<string> idModList)
        {
            List<int> idModList1 = new List<int>() { 1, 3 };
            int[] idModList2 = idModList[0].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => int.Parse(x)).ToArray();

            var moduleForAdd = context.Properties.Where(c => idModList2.Contains(c.ObjectId) && c.PropId == 100);
            foreach (var item in moduleForAdd)
            {
                item.Value = "1";
            }
            //moduleForAdd.Value = "1";
            context.SaveChanges();
        }
        int nodePaste;
        public void pasteNode(int idPasteParentElem, int idCopyParentElem, string nameOPC)
        {

            int nodeCopy;
            nodePaste = idPasteParentElem;
            nodeCopy = idCopyParentElem;
            int maxID1;
            var OPCName = nameOPC;
            using (var context = new EFDbContext())
            {
                var root1 = context.Objects.Where(c => c.Id == nodeCopy).Select(c => c).FirstOrDefault();
                //Извлечем корневой элемент для копирования вместе с его свойствами
                var nodePasteName = from o1 in context.Objects
                                    where o1.Id == nodePaste
                                    select o1.Name;
                var root = from o1 in context.Objects
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
                var rootList = root.ToList();
                var pasteNode = nodePasteName.ToList();
                //подготовим сущность для вставки в таблицу Objects
                Objects obj = new Objects
                {
                    //id = maxID1++,
                    ParentId = Convert.ToInt32(idPasteParentElem),
                    Type = rootList[0].type,
                    Name = rootList[0].name
                };
                //parIDCopy = Convert.ToInt32(idPasteParentElem);
                context.Objects.Add(obj);
                context.SaveChanges();
                //найдем ID только что вставленного корневого элемента
                maxID1 = context.Objects.Select(c => c.Id).Max();
                var getNewPropArr = rootList[0].propValue.Split(',');

                //var getNewID = getNewIDArr[0].Split('\"');
                var oldID = getNewPropArr[0];
                var newID = "{\"ID\":\"" + maxID1 + "\"";
                var newPropJson = rootList[0].propValue.Replace(oldID, newID);

                var getNewParentName = pasteNode[0];

                var oldParentName = getNewPropArr[2];
                var newParentName = oldParentName.Split(':');
                var name = "\"" + getNewParentName + "\"";
                var pasteParentName = newParentName[0] + ':' + name;
                //var newID = "{\"ID\":\"" + maxID1 + "\"";
                var newPropJsonChange1 = newPropJson.Replace(oldParentName, pasteParentName);

                if (getNewPropArr.Length > 10)
                {
                    var oldOPCName = getNewPropArr[11];
                    var newOPCName = oldOPCName.Split(':');
                    OPCName = newOPCName[0] + ':' + "\"" + nameOPC + "\"";
                    newPropJsonChange1 = newPropJsonChange1.Replace(oldOPCName, OPCName);
                }


                //подготовим сущность для вставки в таблицу Properties
                Property objProp = new Property
                {
                    ObjectId = maxID1,
                    PropId = 0,
                    Value = newPropJsonChange1
                };
                context.Properties.Add(objProp);
                context.SaveChanges();

            }
            pasteJsTreeNodes(nodeCopy, maxID1, OPCName);
        }

        int maxIdForProp2;
        public void pasteJsTreeNodes(int parID, int parIDCopy, string OPCName)
        {
            using (var context = new EFDbContext())
            {
                //Родительский копируемый узел
                var root = context.Objects.Where(c => c.Id == parID).Select(c => c).FirstOrDefault();

                var childs1 = context.Objects.Where(c => c.ParentId == parID).Select(c => c);
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
                    int maxID;
                    int maxID2;
                    using (var context1 = new EFDbContext())
                    {
                        if (child.parentID == Convert.ToInt32(nodePaste))
                            return;
                        Objects obj1 = new Objects
                        {
                            //id = maxID1++,
                            ParentId = parIDCopy,
                            Type = child.type,
                            Name = child.name
                        };
                        context1.Objects.Add(obj1);
                        context1.SaveChanges();

                        //ID только что вставленного элемента
                        using (var db4 = new EFDbContext())
                        {
                            var maxIdForProp = db4.Objects.Select(c => c.Id).Max();
                            maxIdForProp2 = Convert.ToInt32(maxIdForProp);
                        }

                        //поправим значение ID в строке свойств 
                        var getNewIDArr = child.propValue.Split(',');
                        //var getNewID = getNewIDArr[0].Split('\"');
                        var oldID = getNewIDArr[0];
                        var newID = "{\"ID\":\"" + maxIdForProp2 + "\"";
                        var newPropJson = child.propValue.Replace(oldID, newID);

                        if (getNewIDArr.Length > 10)
                        {
                            var oldOPCName = getNewIDArr[11];
                            var newOPCName = oldOPCName.Split(':');
                            var newNameOPC = newOPCName[0] + ':' + "\"" + OPCName + "\"";
                            newPropJson = newPropJson.Replace(oldOPCName, newNameOPC);
                        }
                        //вставка строки со свойствами в таблицу свойств
                        Property objProp = new Property
                        {
                            ObjectId = maxIdForProp2,
                            PropId = 0,
                            Value = newPropJson
                        };
                        context1.Properties.Add(objProp);
                        context1.SaveChanges();
                        //Сделала новый Using, потому что возникала ошибка. Не знаю верное ли решение
                        //{System.Data.Entity.Core.EntityException: An error occurred while starting a transaction on the provider connection. See the inner exception for details. --->
                        //System.InvalidOperationException:
                        //Существует назначенный этой команде Command открытый DataReader, который требуется предварительно закрыть.
                        using (var db2 = new EFDbContext())
                        {
                            maxID = db2.Objects.Select(c => c.Id).Max();
                            maxID2 = Convert.ToInt32(maxID);
                        }
                    }
                    pasteJsTreeNodes(child.id, maxID2, OPCName);
                    counter++;
                }
            }
        }

    }





    public class moduleCondition //in use
    {
        //public int idTag;
        public int idModule;
        public string nameModule;
        public string isRun;
        public string descrModule;
        public moduleCondition()
        {
            //idTag = -1;
            idModule = -1;
            nameModule = "";
            isRun = "";
            descrModule = "";

        }
    }

    public class moduleInfo
    {

        public int idModule;
        public string active;
        public string nameModule;
        public string descrModule;


        public moduleInfo()
        {

            idModule = -1;
            active = "";
            nameModule = "";
            descrModule = "";
        }
    }

    public class tagProps
    {
        public int idTag;
        public string propValue;
        public int idProp;

        public tagProps()
        {
            idTag = -1;
            propValue = "";
            idProp = -1;
        }

    }
}

