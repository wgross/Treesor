namespace Treesor.PSDriveProvider
{
    using NLog;
    using NLog.Fluent;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Treesor.Model;

    public class TreesorDriveInfo : PSDriveInfo
    {
        /// <summary>
        /// By default a treesore model is created as an in memory Tresor service. For testing purpos this can be overwritten
        /// </summary>
        public static Func<string, ITreesorModel> TreesorModelFactory { get; set; } = s => InMemoryTreesorService.Factory(s);

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

        internal ITreesorModel Service => this.treesorService;

        #region Creation and initialization of this instance

        public TreesorDriveInfo(PSDriveInfo driveInfo)
            : base(driveInfo)
        {
            this.treesorService = TreesorModelFactory(driveInfo.Root);
        }

        internal IEnumerable<TreesorItem> GetChildItems(TreesorItemPath treesorNodePath, bool recurse)
        {
            if (recurse)
                return this.treesorService.GetDescendants(treesorNodePath);
            else
                return this.treesorService.GetChildItems(treesorNodePath);
        }

        private readonly ITreesorModel treesorService;

        #endregion Creation and initialization of this instance

        #region Get notified of end of life

        public void RemovingDrive()
        {
            this.treesorService.Dispose();
        }

        internal IEnumerable<string> GetChildNames(TreesorItemPath treesorNodePath, ReturnContainers returnContainers)
        {
            return this.treesorService
                .GetChildItems(treesorNodePath)
                .Select(ci => ci.Path.HierarchyPath.Leaf().Items.Last());
        }

        #endregion Get notified of end of life
    }
}