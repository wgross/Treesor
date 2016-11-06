namespace Treesor.PSDriveProvider
{
    using NLog;
    using NLog.Fluent;
    using System.Management.Automation;
    using System;

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

        internal void ClearItem(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        #region Creation and initialization of this instance

        public TreesorDriveInfo(PSDriveInfo driveInfo)
            : base(driveInfo)
        {
            this.treesorService = TreesorService.Factory(driveInfo.Root);
        }

        internal object GetItem(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
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

        #endregion Get notified of end of life

        #region Implement ItemCmdletProvider methods

        internal bool ItemExists(TreesorNodePath treesorNodePath)
        {
            return this.treesorService.ItemExists(treesorNodePath);
        }

        internal void SetItem(TreesorNodePath treesorNodePath, object value)
        {
            throw new NotImplementedException();
        }

        #endregion Implement ItemCmdletProvider methods

    }
}