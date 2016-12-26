using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Abstract
{
    public interface IJSTree
    {
        //string GetTreeAsRawHTML(int root);
        string CreateJsTree(int parID);
        void pasteNode(int idPasteParentElem, int idCopyParentElem);
        void pasteJsTreeNodes(int parID, int parIDCopy);
        void deleteJsTreeNode(int parID);
        void addNode(string newNodeName, int newNodeType, int idNodeToAdd, int defNodeObjType);
        void renameNode(int idRenameNode, string newNodeName, int typeProp);
        Dictionary<int, string> findObjForDefaultNode();
        string getConnectionProp(int idElemToPaste);
        int getOPCID();

    }
}
