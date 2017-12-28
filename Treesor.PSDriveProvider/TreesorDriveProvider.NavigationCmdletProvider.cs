using Elementary.Hierarchy;
using NLog.Fluent;
using System.Linq;
using Treesor.Model;
using static Treesor.Model.TreesorItemPath;

namespace Treesor.PSDriveProvider
{
    public partial class TreesorDriveProvider
    {
        #region Override NavigationCmdletProvider methods

        protected override void MoveItem(string path, string destination)
        {
            log.Trace().Message($"{nameof(MoveItem)}({nameof(path)}={path}, {nameof(destination)}={destination})").Write();

            this.DriveInfo.Model.MoveItem(path: ParsePath(path), destination: ParsePath(destination));
        }

        protected override string GetChildName(string path)
        {
            log.Trace().Message($"{nameof(GetChildName)}({nameof(path)}='{path}')").Write();

            var childName = ParsePath(path).HierarchyPath.Leaf().ToString();

            log.Debug().Message($"{nameof(GetChildName)}({nameof(path)}='{path}')->'{childName}'").Write();

            return childName;
        }

        protected override string MakePath(string parent, string child)
        {
            log.Trace().Message($"{nameof(MakePath)}({nameof(parent)}='{parent}',{nameof(child)}='{child}')").Write();

            var result = CreatePath(
                ParsePath(parent).HierarchyPath.Join(ParsePath(child).HierarchyPath).Items.ToArray()).ToString();

            log.Trace().Message($"{nameof(MakePath)}({nameof(parent)}='{parent}',{nameof(child)}='{child}')->'{result}'").Write();

            return result;
        }

        protected override string GetParentPath(string path, string root)
        {
            log.Trace().Message($"{nameof(GetParentPath)}({nameof(path)}='{path}',{nameof(root)}='{root}')").Write();

            var parsedPath = ParsePath(path).HierarchyPath;
            string result;
            if (parsedPath.HasParentNode)
                result = parsedPath.Parent().ToString();
            else
                result = root;

            log.Trace().Message($"{nameof(GetParentPath)}({nameof(path)}='{path}',{nameof(root)}='{root}')->'{result}'").Write();
            return result;
        }

        protected override bool IsItemContainer(string path)
        {
            log.Trace().Message($"{nameof(IsItemContainer)}({nameof(path)}={path})").Write();

            try
            {
                return (this.DriveInfo.Model.Items.Get(ParsePath(path)).IsContainer);
            }
            catch (TreesorModelException ex) when (TreesorModelErrorCodes.MissingItem.Equals(ex.ErrorCode))
            {
                return false;
            }
        }

        #endregion Override NavigationCmdletProvider methods
    }
}