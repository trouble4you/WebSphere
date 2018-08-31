using Foolproof;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WebSphere.WebUI.Models
{


    public class AddNodeModel
    {
        [Required(ErrorMessage = "Поле должно быть заполнено")]
        public string Name { get; set; }
        public int nodeType2 { get; set; }
        public int idNodeToAdd { get; set; }
        //public int userNodeObjType { get; set; }
        public int typeParentNode { get; set; }
    }


}
