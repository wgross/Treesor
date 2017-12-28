using NLog.Fluent;
using System.Linq;
using System.Management.Automation;
using Treesor.Model;

namespace Treesor.PSDriveProvider
{
    public partial class TreesorDriveProvider
    {
        #region Override ItemCmdletProvider methods

        protected override bool IsValidPath(string path)
        {
            log.Trace().Message($"{nameof(IsValidPath)}({nameof(path)}={path})").Write();

            // the path is considered valid if it can be parsed.
            TreesorItemPath parsedPath;
            return TreesorItemPath.TryParse(path, out parsedPath);
        }

        protected override void ClearItem(string path)
        {
            log.Trace().Message($"{nameof(ClearItem)}({nameof(path)}={path})").Write();

            this.DriveInfo.Model.ClearItem(TreesorItemPath.ParsePath(path));
        }

        protected override string[] ExpandPath(string path)
        {
            log.Trace().Message($"{nameof(ExpandPath)}({nameof(path)}={path})").Write();

            var filter = new WildcardPattern(path);

            var tmp = from i in this.DriveInfo.Model.GetDescendants(TreesorItemPath.RootPath)
                      let itemPath = i.Path.ToString()
                      where filter.IsMatch(itemPath)
                      select itemPath;
            var tmp2 = tmp.ToArray();
            return tmp2;
        }

        protected override void GetItem(string path)
        {
            log.Trace().Message($"{nameof(GetItem)}({nameof(path)}={path})").Write();

            var treesorNodePath = TreesorItemPath.ParsePath(path);

            try
            {
                var item = this.DriveInfo.Model.Items.Get(treesorNodePath);

                log.Debug()
                    .Message($"{nameof(GetItem)}:Sending to pipe:{nameof(this.WriteItemObject)}({nameof(item.GetHashCode)}={item?.GetHashCode()},{nameof(item.Path)}={item?.Path},{nameof(item.IsContainer)}={item?.IsContainer})")
                    .Write();

                this.WriteItemObject(item, path, isContainer: item.IsContainer);
            }
            catch (TreesorModelException ex) when (TreesorModelErrorCodes.MissingItem.Equals(ex.ErrorCode))
            {
                this.WriteError(new ErrorRecord(ex, "getitem.1", ErrorCategory.ObjectNotFound, path));
            }
        }

        protected override bool ItemExists(string path)
        {
            log.Trace().Message($"{nameof(ItemExists)}({nameof(path)}={path})").Write();

            return this.DriveInfo.Model.Items.Exists(TreesorItemPath.ParsePath(path));
        }

        protected override void SetItem(string path, object value)
        {
            log.Trace().Message($"{nameof(SetItem)}({nameof(path)}={path},{nameof(value)}.GetHashCode={value?.GetHashCode()})").Write();

            try
            {
                this.DriveInfo.Model.SetItem(TreesorItemPath.ParsePath(path), value);
            }
            catch (TreesorModelException ex) when (TreesorModelErrorCodes.NotImplemented.Equals(ex.ErrorCode))
            {
                this.WriteError(new ErrorRecord(ex, "setitem.1", ErrorCategory.NotImplemented, path));
            }
        }

        #endregion Override ItemCmdletProvider methods
    }
}