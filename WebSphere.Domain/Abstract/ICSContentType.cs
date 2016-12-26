using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Abstract
{
    public interface ICSContentType
    {
        // список типов контента (можно запросить полный список / по Id / по Name типа контента)
        List<ContentType> ContentTypeList(int? id, string name = "");

        // список групп типов контента
        List<ContentGroup> GetContentTypeGroups(int? id);

        // cписок групп типов контена (именно для групп)
        List<ContentGroup> ContentGroupList(string obj = "");

        // создание типа контента
        bool CreateContentType(ContentType ct);

        // обновление типа контента
        bool ChangeContentType(ContentType ct);

        // удаление типа контента
        bool DeleteContentType(List<ContentType> ct);

        // создание группы типа контента
        bool CreateContentTypeGroup(string name);

        // изменение группы типа контента
        bool ChangeContentTypeGroup(int id, string name);

        // удаление группы типа контента
        bool DeleteContentTypeGroup(List<ContentGroup> groups);
    }
}
