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
    public class MetaObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int? ParentId { get; set; }
        public List<MetaObject> Children { get; set; }
    }
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
        public bool? Sound { get; set; }
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

        [RegularExpression("(H[0-9]{1,2}[0-9a-fA-F]{4})|([0-9]{1,2}\\.[0-9a-fA-F]{4}\\.[0-9]{1,2}\\.[0-9]{1,2})|([0-9]{1,2}\\.[0-9a-fA-F]{4})", ErrorMessage = "Выражение не соотвествует шаблону")]
        public string Register { get; set; }

        public int? AccessType { get; set; }

        public int? Order { get; set; }

        public float? InMin { get; set; }

        public float? InMax { get; set; }

        public float? OutMin { get; set; }

        public float? OutMax { get; set; }
        public string IsSpecialTag { get; set; }
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
        public NoTypesProps notypesforSave { get; set; }
        public NoTypeNodeHelp notypesModel { get; set; }

    }
    public class NoTypesProps
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string special { get; set; }
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
        public string Description { get; set; }

        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int ControllerType { get; set; }

        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int RealType { get; set; }

        [Required(ErrorMessage = "Поле должно быть установлено")]
        [RegularExpression("(H[0-9]{1,2}[0-9a-fA-F]{4})|([0-9]{1,2}\\.[0-9a-fA-F]{4}\\.[0-9]{1,2}\\.[0-9]{1,2})|([0-9]{1,2}\\.[0-9a-fA-F]{4})", ErrorMessage = "Выражение не соотвествует шаблону")]
        public string Register { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int AccessType { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int Order { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        //[RegularExpression("[0-9,]{0,15}", ErrorMessage = "Введено более 15 символов")]
        public float InMin { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public float InMax { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public float OutMin { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public float OutMax { get; set; }
        public string IsSpecialTag { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public bool History_IsPermit { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int RegPeriod { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public double Deadbend { get; set; }
        public bool UpdateAnyway { get; set; }//поменяет метку времени 
        public Alarms Alarms { get; set; }
        public Event Events { get; set; }

    }
    public class Alarms
    {
        //[Required(ErrorMessage = "Поле должно быть установлено")]
        [Display(Name = "Включить")]
        public bool Permit { get; set; }
        [Display(Name = "Следить")]
        public bool Enabled { get; set; }
        [Display(Name = "Звук")]
        public bool Sound { get; set; }
        [StringLength(50, ErrorMessage = "Превышена длина сообщения")]
        [RegularExpression("[^'<>]*", ErrorMessage = "Символы ' , < и > не допустимы")]
        public string HiHiText { get; set; }
        [StringLength(50, ErrorMessage = "Превышена длина сообщения")]
        [RegularExpression("[^'<>]*", ErrorMessage = "Символы ' , < и > не допустимы")]
        public string HiText { get; set; }
        [StringLength(50, ErrorMessage = "Превышена длина сообщения")]
        [RegularExpression("[^'<>]*", ErrorMessage = "Символы ' , < и > не допустимы")]
        public string NormalText { get; set; }
        [StringLength(50, ErrorMessage = "Превышена длина сообщения")]
        [RegularExpression("[^'<>]*", ErrorMessage = "Символы ' , < и > не допустимы")]
        public string LoText { get; set; }
        [StringLength(50, ErrorMessage = "Превышена длина сообщения")]
        [RegularExpression("[^'<>]*", ErrorMessage = "Символы ' , < и > не допустимы")]
        public string LoLoText { get; set; }
        //[RegularExpression("[\\w\\d,]{0,15}", ErrorMessage = "Введено более 15 символов")]
        //[GreaterThan("HiSeverity")]
        public double? HiHiSeverity { get; set; }
        //[RegularExpression("[0-9,]{0,15}", ErrorMessage = "Введено более 15 символов")]
        //[GreaterThan("LoSeverity")]
        public double? HiSeverity { get; set; }
        //[RegularExpression("[0-9,]{0,15}", ErrorMessage = "Введено более 15 символов")]
        //[GreaterThan("LoLoSeverity")]
        public double? LoSeverity { get; set; }
        //[RegularExpression("[0-9,]{0,15}", ErrorMessage = "Введено более 15 символов")]
        //[LessThan("LoSeverity")]
        public double? LoLoSeverity { get; set; }
    }

    public class Event
    {
        [Display(Name = "Следить")]
        public bool Enabled { get; set; }
        public List<EventValMessage> EventMessages { get; set; }
    }

    public class EventValMessage
    {
        [RegularExpression("[0-9,]{0,15}", ErrorMessage = "Введено более 15 символов")]
        public float Value { get; set; }
        [StringLength(50, ErrorMessage = "Превышена длина сообщения")]
        public string Message { get; set; }
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
        [RegularExpression(@"COM\d{1,3}", ErrorMessage = "Не соотвествует шаблону COMxxx")]
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
    }

    public class GPRSChannelProps
    {
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

    }

    public class ObjectProps
    {
        public int Id { get; set; }
        //[RegularExpression(@"^[0-9a-zA-Zа-яА-Я]+$", ErrorMessage = "Некорректное название")]
        public string Name { get; set; }
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
        public int PgPause { get; set; } //паузы между группами опроса ++++

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
        public string UserData { get; set; } //вспомогательная штука
    }
    public class ErrorUserProp
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public dynamic Value { get; set; }
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
    }
}

