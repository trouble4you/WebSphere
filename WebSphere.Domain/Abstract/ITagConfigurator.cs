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
        int GetPropType(int id);
        List<moduleCondition> GetConnectedModules();
        List<moduleCondition> modulesToConnect();
        void deleteModule(int id);
        void changeStatus(int id, string moduleStatus);
        List<moduleCondition> AddModuleDialog();
        void AddModule(List<string> idModList);
        TagProps getTagProps(int id);
        RadioChannelProps getRadioChannelProps(int id);
        GPRSChannelProps getGPRSChannelProps(int id);
        ObjectProps getObjectProps(int id);
        PollingGroupProps getPollingGroupProps(int id);
        OPCProps getOPCProps(int id);
        NoTypeNodeHelp getNoTypeNodeProps(int id);
        Dictionary<string, string> getCommonProps(int id);
        void saveObjectProps(ObjectProps objectProps, int id);
        void saveTagProps(TagProps tagProps, int id);
        void saveOPCProps(OPCProps opcProps, int id);
        void savePollingGroupProps(PollingGroupProps pollingGroupProps, int id);
        void saveRadioChannelProps(RadioChannelProps radioChannelProps, int id);
        void saveGPRSChannelProps(GPRSChannelProps gprsChannelProps, int id);
        void saveNoTypesProps(NoTypesProps noTypesProps, int id);
        void addProp(string propName, string propValue, int propType, int nodeId);
        void deleteProp(int id, string propName);
        Dictionary<string, dynamic> getUserProps(int id);
        Dictionary<string, dynamic> getStandartProps(int id);
        bool checkExistingNodeName(string Name);
        //bool checkExistingPropName(string Name);
        void addProp(string newProp, int nodeId);
        Dictionary<int, string> getOpcServersName();
        int? getParentGroup(int id);
        Dictionary<int, string> getChannels();
        NoTypesProps getNoTypeNodePropsIdName(int id);
        void saveNoTypesProps1(NoTypesPropsHelp model);
        List<ErrorUserProp> checkUserPropsValidity(string propsString);
        void deleteProps(int id, List<string> deletePropsArr);
        Dictionary<string, dynamic> getUserPropsAfterValidate(string jsonStr);
        bool checkUserStrProp(string checkStr);
    }
}
