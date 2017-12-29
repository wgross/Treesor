﻿namespace Treesor.PSDriveProvider
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

        internal ITreesorModel Model => this.treesorModel;

        #region Creation and initialization of this instance

        public TreesorDriveInfo(PSDriveInfo driveInfo)
            : base(driveInfo)
        {
            this.treesorModel = TreesorModelFactory(driveInfo.Root);
        }

        internal IEnumerable<TreesorItem> GetChildItems(TreesorItemPath treesorNodePath, bool recurse)
        {
            if (recurse)
                return this.treesorModel.Items.GetDescendants(treesorNodePath);
            else
                return this.treesorModel.Items.GetChildItems(treesorNodePath);
        }

        private readonly ITreesorModel treesorModel;

        #endregion Creation and initialization of this instance

        #region Get notified of end of life

        public void RemovingDrive()
        {
            this.treesorModel.Dispose();
        }

        internal IEnumerable<string> GetChildNames(TreesorItemPath treesorNodePath, ReturnContainers returnContainers)
        {
            return this.treesorModel.Items
                .GetChildItems(treesorNodePath)
                .Select(ci => ci.Path.HierarchyPath.Leaf().Items.Last());
        }

        #endregion Get notified of end of life
    }
}