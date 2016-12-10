using Elementary.Hierarchy;
using NLog.Fluent;
using System;
using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider
{
    public partial class TreesorDriveProvider
    {
        #region Override ItemCmdletProvider methods

        protected override bool IsValidPath(string path)
        {
            log.Trace().Message($"{nameof(IsValidPath)}({nameof(path)}={path})").Write();

            // the path is considered valid if it can be parsed.
            TreesorNodePath parsedPath;
            return TreesorNodePath.TryParse(path, out parsedPath);
        }

        protected override void ClearItem(string path)
        {
            log.Trace().Message($"{nameof(ClearItem)}({nameof(path)}={path})").Write();

            this.GetTreesorDriveInfo().ClearItem(TreesorNodePath.Parse(path));
        }

        protected override string[] ExpandPath(string path)
        {
            log.Trace().Message($"{nameof(ExpandPath)}({nameof(path)}={path})").Write();

            return this.GetTreesorDriveInfo()
                .GetChildNames(TreesorNodePath.Create(path), ReturnContainers.ReturnMatchingContainers)
                .ToArray();
        }

        protected override void GetItem(string path)
        {
            log.Trace().Message($"{nameof(GetItem)}({nameof(path)}={path})").Write();

            var treesorNodePath = TreesorNodePath.Parse(path);

            var item = this.GetTreesorDriveInfo().GetItem(treesorNodePath);

            log.Debug()
                .Message($"{nameof(GetItem)}:Sending to pipe:{nameof(this.WriteItemObject)}({nameof(item)}.GetHashCode={item?.GetHashCode()},{nameof(item.Path)}={item.Path},isContainer={item.IsContainer})")
                .Write();

            this.WriteItemObject(item, path, isContainer: item.IsContainer);
        }

        protected override bool ItemExists(string path)
        {
            log.Trace().Message($"{nameof(ItemExists)}({nameof(path)}={path})").Write();

            return this.GetTreesorDriveInfo().ItemExists(TreesorNodePath.Parse(path));
        }

        protected override void SetItem(string path, object value)
        {
            log.Trace().Message($"{nameof(SetItem)}({nameof(path)}={path},{nameof(value)}.GetHashCode={value?.GetHashCode()})").Write();

            this.GetTreesorDriveInfo().SetItem(TreesorNodePath.Parse(path), value);
        }

        #endregion Override ItemCmdletProvider methods
    }
}