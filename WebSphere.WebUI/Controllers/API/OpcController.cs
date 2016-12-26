using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Management;
using System.Web.Script.Serialization;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.WebUI.Controllers.API  
{
    [Authorize]
    public class OpcController : ApiController
    {
        private static Logging logger = new Logging();

         
        IJSON _json=new JSON();
 
        private EFDbContext context = new EFDbContext();

        [HttpPost]
        public void SetOpcTagsValues(FormDataCollection data)
        {
            var result = data.Get("Tag"); 
            
                 var jss = new JavaScriptSerializer();
            var dict = jss.Serialize(result);
            Console.WriteLine(dict);
        }
        [HttpPost]
        public List< TagValueContainer> GetOpcTagsValues(FormDataCollection data)
        {
             
     var result = new List< TagValueContainer>();
             if (data == null) return (result);
                
                 var strTagCount = data.Get("TagsCount");
                 var tagCount = Convert.ToInt32(strTagCount);
                 var tags = new List<string>();
            List<string> z;
            for (var tagIndex = 0; tagIndex < tagCount; tagIndex++)
            {
                var tag = data.Get("Tags[" + tagIndex + "]"); 
                var custs = from Object in context.Objects.Where(x => x.Name == tag)
                    join to in context.Properties.Where(x => x.PropId == 0) on Object.Id equals to.ObjectId
                    select to.Value;
                if (custs.FirstOrDefault() != null)
                {
                    var dict = _json.Deserialize(custs.FirstOrDefault());

                    if (dict != null)
                    {
                        try
                        { 
                           // var tvd = 
                               // MvcApplication.OpcPoller.ReadTag(new TagId( new TagId{PollerId =dict["Opc"],TagName =dict["Connection"] })); 
                          //  if (tvd != null) result.Add(tvd);
                        }
                        catch (Exception ex)
                        {
                            result.Add(new TagValueContainer
                            {
                                Tag = new TagId {PollerId = 0, TagName = tag},
                                LastValue = ex.Message
                            });
                            logger.Logged("Error", "No key in Json " + tag, "OpcController", "Opc");
                            break;
                        }
                    }

                }

                else
                {
                    result.Add(new TagValueContainer
                    {
                        Tag = new TagId {PollerId = 0, TagName = tag},
                        LastValue = "No properties"
                    });
                logger.Logged("Error", "No   Json " + tag, "OpcController", "Opc");
            }
        }

            return result ; 
        }

        [HttpPost]
        public IEnumerable<TagValueDto> GetOpcTagsVals(FormDataCollection data)
        {

            var jss = new JavaScriptSerializer();
            var result = new List<TagValueDto>();
            if (data == null) return (result); 
            var strTagCount = Convert.ToInt32(data.Get("TagsCount"));

            for (var tagIndex = 0; tagIndex < strTagCount; tagIndex++)
            {
                var tag = data.Get("Tags[" + tagIndex + "]");
                var json = from Object in context.Objects.Where(x => x.Name == tag)
                    join to in context.Properties.Where(x => x.PropId == 0) on Object.Id equals to.ObjectId
                    select to.Value;
                result.Add(new TagValueDto {Tag = tag, Json = json.FirstOrDefault()});
            }

            foreach (var tag in result)
            { 
            if (tag.Json != null)
                {
                    var dict = jss.Deserialize<Dictionary<string, dynamic>>(tag.Json); 
                    if (dict != null)
                    { try
                        {
                            var tvd =
                               MvcApplication.OpcPoller.ReadTag(new TagId { PollerId = dict["Opc"], TagName = dict["Connection"] }); 
                            if (tvd != null) 
                                tag.OpcVals=tvd;
                        }
                        catch (Exception ex)
                        {
                            tag.OpcVals=(new TagValueContainer
                            {
                                Tag = new TagId { PollerId = 0, TagName = tag.Tag },
                                LastValue = "No 'Opc', 'Connection' key in Json "
                            });
                            logger.Logged("Error", "No 'Opc', 'Connection' key in Json " + tag, "OpcController", "Opc");
                            break;
                        }
                    }

                }

                else
                {
                    tag.OpcVals = (new TagValueContainer
                    {
                        Tag = new TagId { PollerId = 0, TagName = tag.Tag },
                        LastValue = "No Json properties in DataBase"
                    });
                    logger.Logged("Error", "No   Json " + tag.Tag, "OpcController", "Opc");
                }
            } 
            return result;
        }
 
        [HttpPost]
        public bool WriteOpcTagValue(FormDataCollection data)
        {

            var jss = new JavaScriptSerializer();
            var tag = data.Get("tag");
            var value = data.Get("value");

            var custs = from Object in context.Objects.Where(x => x.Name == tag)
                join to in context.Properties.Where(x => x.PropId == 0) on Object.Id equals to.ObjectId
                select to.Value;
            if (custs.FirstOrDefault() != null)
            {
                var dict = jss.Deserialize<Dictionary<string, dynamic>>(custs.FirstOrDefault());

                if (dict != null)
                {
                    try
                    {
                        return MvcApplication.OpcPoller.WriteTag(new TagId { PollerId = dict["Opc"], TagName = dict["Connection"] },
                               value);
                        
                    }
                    catch (Exception ex)
                    {
                        logger.Logged("Error", "Cant Write " + tag + "bacause :"+ex.Message, "OpcController", "WriteTag");
                        return false;
                    }
                } logger.Logged("Error", "No Json " + tag +" in DB", "OpcController", "WriteTag"); return false;
            }  logger.Logged("Error", "No  " + tag +" in DB", "OpcController", "WriteTag"); return false;
                
        }


        [HttpPost]
        public  List<TagValueContainer> GetAllOpcTagsValues(FormDataCollection data)
        {

            return MvcApplication.OpcPoller.ReadTags();
        }
        [HttpPost]
        public string GetOpcTag(FormDataCollection data)
        {
            var result = data.Get("Tag");
            return MvcApplication.OpcPoller.OnReadOpcTag(result);
        }  

            
        [HttpPost]
        public List<Dictionary<string, dynamic>> GetOpcServersInfo(FormDataCollection data)
        {
            var z = MvcApplication.OpcPoller.GetOpcInfo();

            return z;
        }
        [HttpPost]
        public bool ReConnectServer(FormDataCollection data)
        {
            int pollerId = Convert.ToInt32(data.Get("pollerId"));
            var z = MvcApplication.OpcPoller.Reinicialize(pollerId);

            return z;
        } 
        /*
        [HttpPost]
        public string GetOpcTagsValues1(FormDataCollection data)
        {
            
            string result = "";
            if (data == null) return (result);

            var strTagCount = data.Get("TagsCount");
            var tagCount = Convert.ToInt32(strTagCount);
            var sender = data.Get("Sender");

            var filter = ""; 
                var tag = data.Get("Tags[0]");
                var tags = (from ti in context.Objects
                            join to in context.Properties on ti.Id equals to.ObjectId
                            where ti.Type == 2 && ti.ObjectName== tag && to.PropId == 0
                            select to.Value);
                
                 var z = tags.ToList().First();  




                  return z;
        }
        */
    }
}
