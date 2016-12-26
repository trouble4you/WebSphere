using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Abstract
{
    public interface IAccount
    {
        // аутентификация
        bool Authenticate(string username, string password);

        // авторизация
        bool HasAccess(string controller, string action, string username, List<Role> roles);

        // список пользователей
        List<User> UsersList(string username);

        // регистрация пользователя 
        bool CreateUser(User user);

        // изменение пользователя
        bool ChangeUser(User user);

        // удаление пользователя
        bool DeleteUser(List<User> users);

        // список всех ролей
        //List<Role> GetAllRoles();

        // список всех ролей пользователя
        List<Role> GetUserRoles(string username);

        // создание роли
        bool CreateRole(string name, List<ContentType> ct);

        // изменеие роли
        bool ChangeRole(int id, string name, List<ContentType> ct);

        // удаление роли
        bool DeleteRole(List<Role> roles);

        // данные роли
        Role GetRoleData(string name, List<Role> roles);

        // добавить роль пользователю
        bool AddUserToRole(string username, List<Role> roles);

        // удалить роли пользователя
        bool RemoveRolesFromUser(string username);
    }
}
