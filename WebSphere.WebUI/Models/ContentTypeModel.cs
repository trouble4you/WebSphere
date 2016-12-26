using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebSphere.Domain.Entities;
using WebSphere.WebUI.Helpers;

namespace WebSphere.WebUI.Models
{
    // создание типа контента
    public class CreateContentTypeViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, укажите тип контента")]
        [RegularExpression(@"^[а-яА-Яa-zЁёA-Z0-9]+(([_\-\s]+[а-яА-ЯЁёa-zA-Z0-9]{1,})?)+$", ErrorMessage = "Тип контента указан неверно")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Длина должна быть от 2 до 30 символов")]
        [Display(Name = "Тип контента: ")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите контроллер")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Длина должна быть от 2 до 30 символов")]
        [Display(Name = "Контроллер: ")]
        public string Controller { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите экшены")]
        [Display(Name = "Экшены: ")]
        public List<Permission> Actions { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите имя группы")]
        [Display(Name = "Имя группы: ")]
        public ContentGroup contentGroup { get; set; }

        // cписок контроллеров и их экшенов json - строка
        public string caList { get; set; }
    }

    // изменение типа контента
    public class ChangeContentTypeViewModel
    {
        [Required(ErrorMessage = "Отсутствует ID типа контента")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите тип контента")]
        [RegularExpression(@"^[а-яА-Яa-zЁёA-Z0-9]+(([_\-\s]+[а-яА-ЯЁёa-zA-Z0-9]{1,})?)+$", ErrorMessage = "Тип контента указан неверно")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Длина должна быть от 2 до 30 символов")]
        [Display(Name = "Тип контента: ")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите контроллер")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Длина должна быть от 2 до 30 символов")]
        [Display(Name = "Контроллер: ")]
        public string Controller { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите экшены")]
        [Display(Name = "Экшены: ")]
        public List<Permission> Actions { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите имя группы")]
        [Display(Name = "Имя группы: ")]
        public ContentGroup contentGroup { get; set; }

        // cписок контроллеров и их экшенов json - строка
        public string caList { get; set; }
    }

    // удаление типа контента
    public class DeleteContentTypeViewModel
    {
        [Required(ErrorMessage = "Не выбраны объекты для обработки")]
        public List<ContentType> contenttypes { get; set; }
    }

    // модель для подгрузки списка экшенов при смене контроллера
    public class ActionsListViewModel
    {
        public List<Permission> Actions { get; set; }
    }

    // создание группы типа контента
    public class CreateContentTypeGroupViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, укажите имя группы")]
        [RegularExpression(@"^[а-яА-Яa-zЁёA-Z0-9]+(([_\-\s]+[а-яА-ЯЁёa-zA-Z0-9]{1,})?)+$", ErrorMessage = "Группа указана неверно")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Длина должна быть от 2 до 30 символов")]
        [Display(Name = "Имя: ")]
        public string Name { get; set; }
    }

    // изменение группы типа контента
    public class ChangeContentTypeGroupViewModel
    {
        [Required(ErrorMessage = "Отсутствует ID типа контента")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите имя группы")]
        [RegularExpression(@"^[а-яА-Яa-zЁёA-Z0-9]+(([_\-\s]+[а-яА-ЯЁёa-zA-Z0-9]{1,})?)+$", ErrorMessage = "Группа указана неверно")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Длина должна быть от 2 до 30 символов")]
        [Display(Name = "Имя: ")]
        public string Name { get; set; }
    }

    // удаление группы типа контента
    public class DeleteContentTypeGroupViewModel
    {
        [Required(ErrorMessage = "Не выбраны объекты для обработки")]
        public List<ContentGroup> contentgroups { get; set; }
    }
}