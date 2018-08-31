using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Abstract
{
    public interface IJSTree
    {
        string CreateJsTree(int parID);
        string CreateJsTreeHelp(int parID);
        string pasteNodeRoot(int idPasteParentElem, int idCopyParentElem, string newContName);
        void pasteNode(int copyNode, int newNode);
        List<int> deleteJsTreeNodeRoot(int parID);
        List<int> deleteJsTreeNode(int parID);
        void deleteJsTreeNodeBulk(List<int> IdList);
        List<int> findObjectNodes();
        int addNode(string newNodeName, int newNodeType, int idNodeToAdd);
        void renameNode(int idRenameNode, string newNodeName, int typeProp);
        string getConnectionProp(int? idElemToPaste, out int OPCId);
        //int getOPCID();
        int getRootNodeId();
        MetaObject CreateMetaObject(int parentId, ref Dictionary<int, object> Nodes);
        bool CheckNodeExists(int Id);

    }
}
