namespace Treesor.PSDriveProvider
{
    using NLog;
    using NLog.Fluent;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    public class TreesorDriveInfo : PSDriveInfo
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        internal static readonly object NullValue = new object();

        public static TreesorDriveInfo CreateDefault(ProviderInfo provider)
        {
            log.Debug()
                .Message("Creating default drive provider")
                .Property(nameof(provider.Name), provider.Name)
                .Write();

            return new TreesorDriveInfo(new PSDriveInfo(
               name: "treesor",
               provider: provider,
               root: @"\",
               description: "Treesor data store provider",
               credential: null
           ));
        }

        internal void MoveItem(TreesorNodePath path, TreesorNodePath destination)
        {
            this.treesorService.MoveItem(path, destination);
        }

        internal void CopyItem(TreesorNodePath path, TreesorNodePath destinationPath, bool recurse)
        {
            this.treesorService.CopyItem(path, destinationPath, recurse);
        }

        internal void ClearItem(TreesorNodePath treesorNodePath)
        {
            this.treesorService.ClearItem(treesorNodePath);
        }

        #region Creation and initialization of this instance

        public TreesorDriveInfo(PSDriveInfo driveInfo)
            : base(driveInfo)
        {
            this.treesorService = TreesorService.Factory(driveInfo.Root);
        }

        internal IEnumerable<TreesorItem> GetChildItems(TreesorNodePath treesorNodePath, bool recurse)
        {
            if (recurse)
                return this.treesorService.GetDescendants(treesorNodePath);
            else
                return this.treesorService.GetChildItems(treesorNodePath);
        }

        internal TreesorItem GetItem(TreesorNodePath treesorNodePath)
        {
            return this.treesorService.GetItem(treesorNodePath);
        }

        //public TreesorDriveInfo(string name, ProviderInfo provider, string root, string description, PSCredential credential) : base(name, provider, root, description, credential)
        //{
        //}

        private readonly ITreesorService treesorService;

        #endregion Creation and initialization of this instance

        #region Get notified of end of life

        public void RemovingDrive()
        {
            this.treesorService.Dispose();
        }

        internal IEnumerable<object> GetChildNames(TreesorNodePath treesorNodePath, ReturnContainers returnContainers)
        {
            throw new NotImplementedException();
        }

        #endregion Get notified of end of life

        #region Implement ItemCmdletProvider methods

        internal bool ItemExists(TreesorNodePath treesorNodePath)
        {
            return this.treesorService.ItemExists(treesorNodePath);
        }

        internal void SetItem(TreesorNodePath treesorNodePath, object value)
        {
            this.treesorService.SetItem(treesorNodePath, value);
        }

        internal TreesorItem NewItem(TreesorNodePath treesorNodePath, string itemTypeName, object newItemValue)
        {
            return this.treesorService.NewItem(treesorNodePath, newItemValue);
        }

        internal void RemoveItem(TreesorNodePath treesorNodePath, bool recurse)
        {
            this.treesorService.RemoveItem(treesorNodePath, recurse);
        }

        internal bool HasChildItems(TreesorNodePath treesorNodePath)
        {
            return this.treesorService.HasChildItems(treesorNodePath);
        }

        internal void RenameItem(TreesorNodePath treesorNodePath, string newName)
        {
            this.treesorService.RenameItem(treesorNodePath, newName);
        }

        #endregion Implement ItemCmdletProvider methods
    }
}