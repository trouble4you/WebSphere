using System.Collections.Specialized;
using Foolproof;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.WebUI.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Логин должен быть от 2 до 50 символов")]
        [Display(Name = "Логин ")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль ")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 20 символов")]
        public string Password { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, укажите логин")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Логин должен быть от 2 до 50 символов")]
        [RegularExpression(@"^[а-яА-ЯЁёa-zA-Z0-9]+(([_\.\-]+[а-яА-ЯЁёa-zA-Z0-9]{1,})?)+$", ErrorMessage = "Логин указан неверно")]
        [Display(Name = "Логин: ")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите пароль")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 20 символов")]
        [Display(Name = "Пароль: ")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Пожалуйста, повторите пароль")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 20 символов")]
        [Display(Name = "Подтвердите пароль: ")]
        [Compare("Password", ErrorMessage = "Введенное значение не совпадает с паролем")]
        public string ConfirmPassword { get; set; }

        [RegularExpression(@"(^[а-яА-ЯЁёa-zA-Z]{2,})+(([\.\-\s]+[а-яА-ЯЁёa-zA-Z]{2,})?)+$", ErrorMessage = "Имя указано неверно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 50 символов")]
        [Display(Name = "Имя: ")]
        public string Name { get; set; }

        [RegularExpression(@"(^[а-яА-ЯЁёa-zA-Z]{2,})+(([\.\-\s]+[а-яА-ЯЁёa-zA-Z]{2,})?)+$", ErrorMessage = "Фамилия указана неверно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна быть от 2 до 50 символов")]
        [Display(Name = "Фамилия: ")]
        public string LastName { get; set; }

        [RegularExpression(@"(^[а-яА-ЯЁёa-zA-Z]{2,})+(([\.\-\s]+[а-яА-ЯЁёa-zA-Z]{2,})?)+$", ErrorMessage = "Отчество указано неверно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Отчество должно быть от 2 до 50 символов")]
        [Display(Name = "Отчество: ")]
        public string MiddleName { get; set; }

        //[DataType(DataType.EmailAddress)]
        //[EmailAddress (ErrorMessage = "Недопустимый адрес электронной почты")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Email должен быть от 2 до 50 символов")]
        [RegularExpression(@"^[a-zA-Z0-9]+(([_\.\-]+[a-zA-Z0-9]{1,})?)+(@[a-zA-Z0-9]{1,})(([\.\-]+[a-zA-Z0-9]{1,})?)+\.[a-zA-Z]{2,255}$", ErrorMessage = "Недопустимый адрес электронной почты")]
        [Display(Name = "E-mail адрес: ")]
        public string Email { get; set; }

        [Display(Name = "Активный: ")]
        public bool IsActive { get; set; }

        [Display(Name = "Суперпользователь: ")]
        public bool Superuser { get; set; }

        [Display(Name = "Группа: ")]
        public List<Role> Roles { get; set; }
    }

    public class ChangeViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите логин")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Логин должен быть от 2 до 50 символов")]
        [RegularExpression(@"^[а-яА-ЯЁёa-zA-Z0-9]+(([_\.\-]+[а-яА-ЯЁёa-zA-Z0-9]{1,})?)+$", ErrorMessage = "Логин указан неверно")]
        [Display(Name = "Логин: ")]
        public string UserName { get; set; }

        [Display(Name = "Сброс пароля")]
        public bool ResetPassword { get; set; }

        [RequiredIfTrue("ResetPassword", ErrorMessage = "Пожалуйста, укажите пароль")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 20 символов")]
        [Display(Name = "Пароль: ")]
        public string Password { get; set; }

        [RequiredIfTrue("ResetPassword", ErrorMessage = "Пожалуйста, повторите пароль")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 20 символов")]
        [Display(Name = "Подтвердите пароль: ")]
        [Compare("Password", ErrorMessage = "Введенное значение не совпадает с паролем")]
        public string ConfirmPassword { get; set; }

        [RegularExpression(@"(^[а-яА-ЯЁёa-zA-Z]{2,})+(([\.\-\s]+[а-яА-ЯЁёa-zA-Z]{2,})?)+$", ErrorMessage = "Имя указано неверно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 50 символов")]
        [Display(Name = "Имя: ")]
        public string Name { get; set; }

        [RegularExpression(@"(^[а-яА-ЯЁёa-zA-Z]{2,})+(([\.\-\s]+[а-яА-ЯЁёa-zA-Z]{2,})?)+$", ErrorMessage = "Фамилия указана неверно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна быть от 2 до 50 символов")]
        [Display(Name = "Фамилия: ")]
        public string LastName { get; set; }

        [RegularExpression(@"(^[а-яА-ЯЁёa-zA-Z]{2,})+(([\.\-\s]+[а-яА-ЯЁёa-zA-Z]{2,})?)+$", ErrorMessage = "Отчество указано неверно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Отчество должно быть от 2 до 50 символов")]
        [Display(Name = "Отчество: ")]
        public string MiddleName { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Email должен быть от 2 до 50 символов")]
        [RegularExpression(@"^[a-zA-Z0-9]+(([_\.\-]+[a-zA-Z0-9]{1,})?)+(@[a-zA-Z0-9]{1,})(([\.\-]+[a-zA-Z0-9]{1,})?)+\.[a-zA-Z]{2,255}$", ErrorMessage = "Недопустимый адрес электронной почты")]
        [Display(Name = "E-mail адрес: ")]
        public string Email { get; set; }

        [Display(Name = "Активный: ")]
        public bool IsActive { get; set; }

        [Display(Name = "Суперпользователь: ")]
        public bool Superuser { get; set; }

        [Display(Name = "Группа: ")]
        public List<Role> Roles { get; set; }
    }

    public class DeleteViewModel
    {
        [Required(ErrorMessage = "Не выбраны объекты для обработки")]
        public List<User> users { get; set; }
    }

    public class CreateChangeRoleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите имя группы")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Группа должна быть от 2 до 50 символов")]
        [RegularExpression(@"^[а-яА-ЯЁёa-zA-Z0-9]+(([_\.\-\s]+[а-яА-ЯЁёa-zA-Z0-9]{1,})?)+$", ErrorMessage = "Имя указано неверно")]
        [Display(Name = "Имя: ")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите разрешения для группы")]
        [Display(Name = "Разрешения: ")]
        public List<ContentType> ContentTypes { get; set; }
    }

    public class DeleteRoleViewModel
    {
        [Required(ErrorMessage = "Не выбраны объекты для обработки")]
        public List<Role> roles { get; set; }
    }

    public class MyTrendPage
    {
        public string object_type_str = "";
        public string object_name = "";

        public int object_id = 0;
        public UInt64 start_date = 0;
        public UInt64 end_date = 0;

        public List<OrderedDictionary> signals = new List<OrderedDictionary>();
    }
}