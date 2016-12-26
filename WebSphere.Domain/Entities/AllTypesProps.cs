using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WebSphere.Domain.Entities
{
    public class NoTypeNodeHelp
    {


        //public int Id { get; set; }
        //public string Name { get; set; }
        //public string special { get; set; }
        //public int Id { get; set; }

        public string Connection { get; set; }

        public int? Opc { get; set; }
        //public bool Alarm_IsPermitHelp { get; set; }
        //[RequiredIfTrue("Alarm_IsPermitHelp")]
        //[Required(ErrorMessage = "Поле обязательно для ввода")]
        public bool? Alarm_IsPermit { get; set; }

        public string HiHiText { get; set; }

        public string HiText { get; set; }

        public string NormalText { get; set; }

        public string LoText { get; set; }

        public string LoLoText { get; set; }

        public double? HiHiSeverity { get; set; }

        public double? HiSeverity { get; set; }

        public double? LoSeverity { get; set; }

        public double? LoLoSeverity { get; set; }

        public int? ControllerType { get; set; }

        public int? RealType { get; set; }

        [RegularExpression("(H[34]{1}[0-9]{5}\\.[0-9]{1}(\\.[0-9])?)|([34]{1}\\.[0-9]{2}((\\.[0-9]{1})|(\\.[0-9]{1}\\.[0-9]{1}))?)", ErrorMessage = "Выражение не соотвествует шаблону")]
        public string Register { get; set; }

        public int? AccessType { get; set; }

        public int? Order { get; set; }

        public float? InMin { get; set; }

        public float? InMax { get; set; }

        public float? OutMin { get; set; }

        public float? OutMax { get; set; }
        public bool? IsSpecialTag { get; set; }
        public bool? History_IsPermit { get; set; }

        public int? RegPeriod { get; set; }

        public double? Deadbend { get; set; }

        public string ChannelType { get; set; }

        public int? InterPollPause { get; set; }

        public int? MaxErrorsToSwitchChannel { get; set; }

        public int? MaxErrorsToBadQuality { get; set; }

        public int? TimeTryGoBackToPrimary { get; set; }

        public string PortName { get; set; }

        public int? BaudRate { get; set; }

        public int? Parity { get; set; }

        public int? StopBits { get; set; }

        public int? WriteTimeout { get; set; }

        public int? ReadTimeout { get; set; }

        [RegularExpression("^(25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}|[0-9]{2}|[0-9])(\\.(25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}|[0-9]{2}|[0-9])){3}$", ErrorMessage = "Выражение не соотвествует шаблону X.X.X.X")]
        public string IpAddress { get; set; }

        public int? Port { get; set; }

        public int? Address { get; set; }

        public string Driver { get; set; }

        public int? RetrCount { get; set; }
        public int? PrimaryChannel { get; set; }
        public int? SecondaryChannel { get; set; }
        public int? Start { get; set; }
        public int? Count { get; set; }
        public int? Function { get; set; }
        public string Type { get; set; }
        public bool? Connect { get; set; }
        public int? ParentGroup { get; set; }
    }
    public class NoTypesPropsHelp
    {
        //public  static MvcHtmlString CustomTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        //{
        //    var name = ExpressionHelper.GetExpressionText(expression);
        //    var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
        //    TagBuilder tb = new TagBuilder("input");
        //    tb.Attributes.Add("type", "checkbox");
        //    tb.Attributes.Add("name", name);
        //    if (metadata.Model == "true")
        //    {
        //        tb.Attributes.Add("value", "true");
        //    }
        //    else
        //    {
        //        tb.Attributes.Add("value", "false");
        //    }


        //    tb.Attributes.Add("style", "color:red");
        //    return new MvcHtmlString(tb.ToString());
        //}  
        public NoTypesProps notypesforSave { get; set; }
        public NoTypeNodeHelp notypesModel { get; set; }

        //public string getWriteStr (string str, int index)
        //{

        //int index=propValue.IndexOf("\\");
        //if(index=!-1)
        //{
        //    return 
        //}
        //}


    }
    public class NoTypesProps
    {
        public int Id { get; set; }
        public string Name { get; set; }

        //public bool? Alarm_IsPermit { get; set; }

        //public string HiHiText { get; set; }

        //public string HiText { get; set; }

        //public string NormalText { get; set; }

        //public double? HiSeverity { get; set; }

        public string special { get; set; }

        //public string ChannelType { get; set; }
        //public int InterPollPause { get; set; }
        //public int MaxErrorsToSwitchChannel { get; set; }
        //public int MaxErrorsToBadQuality { get; set; }
        //public int TimeTryGoBackToPrimary { get; set; }
        //public string IpAddress { get; set; }
        //public int? Port { get; set; }
        //public int WriteTimeout { get; set; }
        //public int ReadTimeout { get; set; }

        //занести сюда все свойства

        //public List<NewProp> newProps { get; set; }
        //public TagProps tagProp { get; set; }
    }
    public class TagProps
    {
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Opc { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Connection { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public bool Alarm_IsPermit { get; set; }
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        //[RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Могут быть введены только текстовые значения")]
        public string HiHiText { get; set; }
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        //[RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Могут быть введены только текстовые значения")]
        public string HiText { get; set; }
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        //[RegularExpression(@"^[0-9a-zA-Zа-яА-Я]+$", ErrorMessage = "Могут быть введены только текстовые значения")]
        public string NormalText { get; set; }

        //[Required(ErrorMessage = "Поле должно быть установлено")]
        //[RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Могут быть введены только текстовые значения")]
        public string LoText { get; set; }
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        //[RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Могут быть введены только текстовые значения")]
        public string LoLoText { get; set; }
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        public double? HiHiSeverity { get; set; }
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        public double? HiSeverity { get; set; }
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        public double? LoSeverity { get; set; }
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        public double? LoLoSeverity { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        //public IEnumerable<SelectListItem> ControllerType {get; set;}
        public int ControllerType { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        //public IEnumerable<SelectListItem> RealType { get; set; }
        public int RealType { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Register { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int AccessType { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Order { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public float InMin { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public float InMax { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public float OutMin { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public float OutMax { get; set; }
        [Required(ErrorMessage = "Выберите true или false")]
        public bool IsSpecialTag { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public bool History_IsPermit { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int RegPeriod { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public double Deadbend { get; set; }
        //public List<string> AdditionalProps { get; set; }
        //public Dictionary<Object, object>;
        public string special { get; set; }

    }
    public class RadioChannelProps
    {
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string ChannelType { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int InterPollPause { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int MaxErrorsToSwitchChannel { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int MaxErrorsToBadQuality { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int TimeTryGoBackToPrimary { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string PortName { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int BaudRate { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Parity { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int StopBits { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int WriteTimeout { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int ReadTimeout { get; set; }
        public string special { get; set; }
    }

    public class GPRSChannelProps
    {
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string ChannelType { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int InterPollPause { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int MaxErrorsToSwitchChannel { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int MaxErrorsToBadQuality { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int TimeTryGoBackToPrimary { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string IpAddress { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int? Port { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int WriteTimeout { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int ReadTimeout { get; set; }
        public string special { get; set; }
    }

    public class ObjectProps
    {
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Id { get; set; }
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        //[RegularExpression(@"^[0-9a-zA-Zа-яА-Я]+$", ErrorMessage = "Некорректное название")]
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        public string Name { get; set; }
        //[DataType(DataType.Custom)] Как ЭТИМ ПОЛЬЗОВАТЬСЯ???
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Address { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Driver { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int RetrCount { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int ParentGroup { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int PrimaryChannel { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int SecondaryChannel { get; set; }
        public string special { get; set; }

    }
    public class PollingGroupProps
    {
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Id { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Start { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Count { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Function { get; set; }
        public string special { get; set; }

    }
    public class ErrorUserProp
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public dynamic Value { get; set; }
    }

    public class NewProp
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public string Value { get; set; }
    }

    public class NewPropList
    {
        public List<NewProp> newProps { get; set; }
    }
    public class OPCProps
    {
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Type { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Connection { get; set; }
        [Required(ErrorMessage = "Выберите true или false")]
        public bool Connect { get; set; }
        public string special { get; set; }

    }
    public class CommonProps
    {
        public List<TagProps> tagProp { get; set; }
        public List<RadioChannelProps> radioChannelProp { get; set; }
        public List<GPRSChannelProps> GPRSChannelProp { get; set; }
        public List<ObjectProps> objectProp { get; set; }
        public List<PollingGroupProps> pollingGroupProp { get; set; }
        public List<OPCProps> OPCProp { get; set; }
    }
}

