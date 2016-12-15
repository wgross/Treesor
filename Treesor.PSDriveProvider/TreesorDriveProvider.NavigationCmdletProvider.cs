using Elementary.Hierarchy;
using NLog.Fluent;
using System.Linq;

namespace Treesor.PSDriveProvider
{
    public partial class TreesorDriveProvider
    {
        #region Override NavigationCmdletProvider methods

        protected override void MoveItem(string path, string destination)
        {
            log.Trace().Message($"{nameof(MoveItem)}({path}, {destination})").Write();

            this.GetTreesorDriveInfo().MoveItem(path: TreesorNodePath.Parse(path), destination: TreesorNodePath.Parse(destination));
        }

        protected override string GetChildName(string path)
        {
            log.Trace().Message($"{nameof(GetChildName)}({nameof(path)}='{path}')").Write();

            var childName = TreesorNodePath.Parse(path).HierarchyPath.Leaf().ToString();

            log.Debug().Message($"{nameof(GetChildName)}({nameof(path)}='{path}')->'{childName}'").Write();

            return childName;
        }

        protected override string MakePath(string parent, string child)
        {
            log.Trace().Message($"{nameof(MakePath)}({nameof(parent)}='{parent}',{nameof(child)}='{child}')").Write();

            var result = TreesorNodePath.Create(
                TreesorNodePath.Parse(parent).HierarchyPath.Join(TreesorNodePath.Parse(child).HierarchyPath).Items.ToArray()).ToString();

            log.Trace().Message($"{nameof(MakePath)}({nameof(parent)}='{parent}',{nameof(child)}='{child}')->'{result}'").Write();

            return result;
        }

        protected override string GetParentPath(string path, string root)
        {
            log.Trace().Message($"{nameof(GetParentPath)}({nameof(path)}='{path}',{nameof(root)}='{root}')").Write();

            var parsedPath = TreesorNodePath.Parse(path).HierarchyPath;
            string result;
            if (parsedPath.HasParentNode)
                result = parsedPath.Parent().ToString();
            else
                result = root;

            log.Debug().Message($"{nameof(GetParentPath)}({nameof(path)}='{path}',{nameof(root)}='{root}')->'{result}'").Write();
            return result;
        }

        protected override bool IsItemContainer(string path)
        {
            log.Trace().Message($"{nameof(IsItemContainer)}({nameof(path)}={path})").Write();

            return (this.GetTreesorDriveInfo().GetItem(TreesorNodePath.Parse(path))?.IsContainer ?? false);
        }

        #endregion Override NavigationCmdletProvider methods
    }
}