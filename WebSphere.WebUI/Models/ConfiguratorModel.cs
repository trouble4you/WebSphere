using Foolproof;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WebSphere.WebUI.Models
{
    public class AddPropViewModel
    {
        public int nodeType { get; set; }
        public AddUserPropModel userPropModel { get; set; }
        public AddStandartPropModelHelp standartPropModel { get; set; }
    }

    public class AddStandartPropModelHelp
    {
        //!!!При добавлении свойств свойство ParentGroup должно быть в самом низу, т.к. в контроллере при добавлении свойства 
        //берется значение по индексу 2. См==> var propName = ModelState.Keys.ElementAt(2);
        public int selectValueStd { get; set; }
        public int Id { get; set; }
        public string CurrentPropName { get; set; }
        public string Connection { get; set; }
        public int Opc { get; set; }
        public bool Alarm_IsPermit { get; set; }
        public string HiHiText { get; set; }
        public string HiText { get; set; }
        public string NormalText { get; set; }
        public string LoText { get; set; }
        public string LoLoText { get; set; }
        public double HiHiSeverity { get; set; }
        public double HiSeverity { get; set; }
        public double LoSeverity { get; set; }
        public double LoLoSeverity { get; set; }
        public int ControllerType { get; set; }
        public int RealType { get; set; }
        //[RegularExpression("H[34][0-9{5}\\.[0-9]{1}(\\.[0-9])?]", ErrorMessage = "Выражение не соотвествует шаблону X.X.X.X")]
        [RegularExpression("(H[34]{1}[0-9]{5}\\.[0-9]{1}(\\.[0-9])?)|([34]{1}\\.[0-9]{2}((\\.[0-9]{1})|(\\.[0-9]{1}\\.[0-9]{1}))?)", ErrorMessage = "Выражение не соотвествует шаблону")]
        public string Register { get; set; }
        public int AccessType { get; set; }
        public int Order { get; set; }
        public float InMin { get; set; }
        public float InMax { get; set; }
        public float OutMin { get; set; }
        public float OutMax { get; set; }
        public bool IsSpecialTag { get; set; }
        public bool History_IsPermit { get; set; }
        public int RegPeriod { get; set; }
        public double Deadbend { get; set; }
        public string ChannelType { get; set; }
        public int InterPollPause { get; set; }
        public int MaxErrorsToSwitchChannel { get; set; }
        public int MaxErrorsToBadQuality { get; set; }
        public int TimeTryGoBackToPrimary { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int Parity { get; set; }
        public int StopBits { get; set; }
        public int WriteTimeout { get; set; }
        public int ReadTimeout { get; set; }
        [RegularExpression("^(25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}|[0-9]{2}|[0-9])(\\.(25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}|[0-9]{2}|[0-9])){3}$", ErrorMessage = "Выражение не соотвествует шаблону X.X.X.X")]
        public string IpAddress { get; set; }
        public int? Port { get; set; }
        public int Address { get; set; }
        public string Driver { get; set; }
        public int RetrCount { get; set; }

        public int PrimaryChannel { get; set; }
        public int SecondaryChannel { get; set; }
        public int Start { get; set; }
        public int Count { get; set; }
        public int Function { get; set; }
        public string Type { get; set; }
        public bool Connect { get; set; }
        public int? ParentGroup { get; set; }
        //!!!При добавлении свойств свойство ParentGroup должно быть в самом низу, т.к. в контроллере при добавлении свойства 
        //берется значение по индексу 2. См==> var propName = ModelState.Keys.ElementAt(2);
    }


    public class AddStandartPropModelTest
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Range { get; set; }
        public string RegExp { get; set; }
    }

    public class AddNodeModel
    {
        [Required (ErrorMessage="Поле должно быть заполнено")]
        public string Name{get; set;}
        public int nodeType {get; set;}
        public int idNodeToAdd {get;set;}
        public int userNodeObjType { get; set; }
    }

    public class AddUserPropModel
    {

        [Required(ErrorMessage = "Поле обязательно для ввода")]
        //[RegularExpression(@"^[a-zA-Zа-яА-яёЁ]{3,20}[0-9-_\.]{0,3}$", ErrorMessage = "Может быть введен только текстовые значения и последующие цифры")]
        //[Remote("CheckNameProp", "Configurator", ErrorMessage = "Данное название уже существует")]
        public string Name { get; set; }
        public int Id { get; set; }
        public int selectValue { get; set; }

        public bool ByteType { get; set; }
        public bool WordType { get; set; }
        public bool DWordType { get; set; }
        public bool ShortIntType { get; set; }
        public bool SmallIntType { get; set; }
        public bool LongIntType { get; set; }
        public bool FloatType { get; set; }
        public bool DoubleType { get; set; }
        public bool BoolenType { get; set; }
        [RequiredIfTrue("ByteType")]
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        public byte ByteValue { get; set; }

        [RequiredIfTrue("WordType")]
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        public ushort WordValue { get; set; }

        [RequiredIfTrue("DWordType")]
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        public uint DWordValue { get; set; }

        [RequiredIfTrue("ShortIntType")]
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        public sbyte ShortIntValue { get; set; }

        [RequiredIfTrue("SmallIntType")]
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        public short SmallIntValue { get; set; }

        [RequiredIfTrue("LongIntType")]
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        public int LongIntValue { get; set; }

        [RequiredIfTrue("FloatType")]
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        //[DisplayFormat(DataFormatString = "{0:#,##0.000000#}")]
        public float FloatValue { get; set; }

        [RequiredIfTrue("DoubleType")]
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        public double DoubleValue { get; set; }

        [RequiredIfTrue("BoolenType")]
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        public bool BoolenValue { get; set; }
        public bool StringType { get; set; }
        [RequiredIfTrue("StringType")]
        //[Required(ErrorMessage = "Поле обязательно для ввода")]
        public string StringValue { get; set; }

    }

}
