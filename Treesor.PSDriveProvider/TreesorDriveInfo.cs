namespace Treesor.PSDriveProvider
{
    using NLog;
    using NLog.Fluent;
    using System.Collections.Generic;
    using System.Linq;
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

        internal ITreesorService Service => this.treesorService;

        #region Creation and initialization of this instance

        public TreesorDriveInfo(PSDriveInfo driveInfo)
            : base(driveInfo)
        {
            this.treesorService = InMemoryTreesorService.Factory(driveInfo.Root);
        }

        internal IEnumerable<TreesorItem> GetChildItems(TreesorNodePath treesorNodePath, bool recurse)
        {
            if (recurse)
                return this.treesorService.GetDescendants(treesorNodePath);
            else
                return this.treesorService.GetChildItems(treesorNodePath);
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

        internal IEnumerable<string> GetChildNames(TreesorNodePath treesorNodePath, ReturnContainers returnContainers)
        {
            return this.treesorService
                .GetChildItems(treesorNodePath)
                .Select(ci => ci.Path.HierarchyPath.Leaf().Items.Last());
        }

        #endregion Get notified of end of life
    }
}