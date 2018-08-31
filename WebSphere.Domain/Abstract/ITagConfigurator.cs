using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Abstract
{
    public interface ITagConfigurator
    {
        int GetNodeType(int id);
        //List<moduleCondition> GetConnectedModules();
        List<moduleCondition> modulesToConnect();
        void deleteModule(int id);
        void ChangeModuleStatus(int id, string moduleStatus);
        List<moduleCondition> AddModuleDialog();
        void AddModule(List<int> idModList);
        TagProps getTagProps(int id);
        RadioChannelProps getRadioChannelProps(int id);
        GPRSChannelProps getGPRSChannelProps(int id);
        ObjectProps getObjectProps(int id);
        PollingGroupProps getPollingGroupProps(int id);
        OPCProps getOPCProps(int id);
        void saveObjectProps(ObjectProps objectProps, int id);
        void saveTagProps(TagProps tagProps, int id);
        void saveOPCProps(OPCProps opcProps, int id);
        void savePollingGroupProps(PollingGroupProps pollingGroupProps, int id);
        void saveRadioChannelProps(RadioChannelProps radioChannelProps, int id);
        void saveGPRSChannelProps(GPRSChannelProps gprsChannelProps, int id);
        Dictionary<int, string> getOpcServersName();
        //int? getParentGroup(int id);
        Dictionary<int, string> getChannels(int idCallingNode);
        List<int> getChannelsInFolder(int folderId);
        List<moduleCondition> GetModules();
    }
}
