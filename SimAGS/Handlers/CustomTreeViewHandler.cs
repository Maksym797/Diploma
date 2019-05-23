using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cern.colt.matrix;
using javax.swing.tree;
using TreeNode = System.Windows.Forms.TreeNode;

namespace SimAGS.Handlers
{
    public class CustomTreeViewHandler
    {
        private static TreeView _treeView { get; set; }
        private static TreeNodeCollection Cases => _treeView.Nodes["Case"].Nodes;
        private Dictionary<TreeNode, DoubleMatrix2D> storeDictionary = new Dictionary<TreeNode, DoubleMatrix2D>();

        private CustomTreeViewHandler() { }

        public static TreeNode GetCurrentNode()
        {
            return _treeView.SelectedNode;
        }

        public static CustomTreeViewHandler Config(TreeView treeView)
        {
            _treeView = treeView;
            return new CustomTreeViewHandler();
        }

        public static CustomTreeViewHandler loadModelTree()
        {
            //TreeNode root = (TreeNode)modelTree.getModel().getRoot();
            //TreeNode pfNode = new TreeNode("power flow");                   // parent node for power flow 
            //TreeNode dynNode = new TreeNode("dynamic models");              // parent node for dynamic model
            //TreeNode AGCNode = new TreeNode("AGC model");                   // parent node for AGC model 
            //TreeNode lastParentNode = null;

            if (CustomGlobalFormsStore.powerFlowCaseFile != null)
            {
                if (CustomGlobalFormsStore.pfProc.nBus != 0) Cases.Add(new TreeNode("BUS"));
                if (CustomGlobalFormsStore.pfProc.nGen != 0) Cases.Add(new TreeNode("GENERATOR"));
                if (CustomGlobalFormsStore.pfProc.nLoad != 0) Cases.Add(new TreeNode("LOAD"));
                if (CustomGlobalFormsStore.pfProc.nBranch != 0) Cases.Add(new TreeNode("BRANCH"));
                if (CustomGlobalFormsStore.pfProc.nTwoWindXfrm != 0) Cases.Add(new TreeNode("2W TRANSFORMER"));
                if (CustomGlobalFormsStore.pfProc.nThrWindXfrm != 0) Cases.Add(new TreeNode("3W TRANSFORMER"));
                if (CustomGlobalFormsStore.pfProc.nSWShunt != 0) Cases.Add(new TreeNode("SW SHUNT"));
                if (CustomGlobalFormsStore.pfProc.nArea != 0) Cases.Add(new TreeNode("AREA"));
                if (CustomGlobalFormsStore.pfProc.nZone != 0) Cases.Add(new TreeNode("ZONE"));
                if (CustomGlobalFormsStore.pfProc.nOwner != 0) Cases.Add(new TreeNode("OWNER"));
            }

            //if (dynDataCaseFile != null)
            //{
            //    root.add((lastParentNode = dynNode));
            //    if (dynProc.nGENCLS != 0) dynNode.add(new TreeNode("GENCLS"));
            //    if (dynProc.nGENROU != 0) dynNode.add(new TreeNode("GENROU"));
            //    if (dynProc.nESDC1A != 0) dynNode.add(new TreeNode("ESDC1A"));
            //    if (dynProc.nESAC4A != 0) dynNode.add(new TreeNode("ESAC4A"));
            //    if (dynProc.nTGOV1 != 0) dynNode.add(new TreeNode("TGOV1"));
            //    if (dynProc.nHYGOV != 0) dynNode.add(new TreeNode("HYGOV"));
            //    if (dynProc.nIEEEG2 != 0) dynNode.add(new TreeNode("IEEEG2"));
            //    if (dynProc.nWindDyn != 0) dynNode.add(new TreeNode("WINDDYN"));
            //    if (dynProc.nDynLoadUDM != 0) dynNode.add(new TreeNode("DYNLOADUDM"));
            //}

            //if (AGCDataCaseFile != null)
            //{
            //    root.add((lastParentNode = AGCNode));
            //}
            //modelTree.scrollPathToVisible(new TreePath(lastParentNode.getPath()));
            return new CustomTreeViewHandler();
        }
    }
}
