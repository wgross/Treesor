namespace Treesor.PSDriveProvider
{
    using NLog;
    using NLog.Fluent;
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Provider;

    [CmdletProvider(TreesorDriveProvider.Id, ProviderCapabilities.ExpandWildcards)]
    public partial class TreesorDriveProvider : NavigationCmdletProvider
    //, IPropertyCmdletProvider, IDynamicPropertyCmdletProvider
    {
        public const string Id = "Treesor";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TreesorDriveInfo DriveInfo => (TreesorDriveInfo)this.PSDriveInfo;

        #region Override DriveCmdletProvider methods

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            log.Trace().Message($"{nameof(NewDrive)}({nameof(drive)}.Root='{drive.Root}',{nameof(drive)}.Name='{drive.Name}')").Write();

            // Check if the drive object is null.
            if (drive == null)
            {
                this.WriteError(new ErrorRecord(new ArgumentNullException(nameof(drive)), "NullDrive", ErrorCategory.InvalidArgument, targetObject: null));
                return null;
            }

            // Check if the drive root is not null or empty
            // and if it is an existing file.
            //if (String.IsNullOrEmpty(drive.Root) || (File.Exists(drive.Root) == false))
            //{
            //    WriteError(new ErrorRecord(
            //               new ArgumentException("drive.Root"),
            //               "Nof",
            //               ErrorCategory.InvalidArgument,
            //               drive));

            //    return null;
            //}

            return drive as TreesorDriveInfo ?? new TreesorDriveInfo(drive);
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            log.Trace().Message($"{nameof(InitializeDefaultDrives)}()").Write();

            return new Collection<PSDriveInfo>
            {
                TreesorDriveInfo.CreateDefault(this.ProviderInfo)
            };
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            log.Trace().Message($"{nameof(InitializeDefaultDrives)}:Removing drive").Write();

            TreesorDriveInfo treesorDriveInfo = drive as TreesorDriveInfo;

            if (treesorDriveInfo == null)
            {
                this.WriteError(new ErrorRecord(new ArgumentNullException(nameof(drive)), "NullDrive", ErrorCategory.InvalidArgument, null));
                return null;
            }

            DriveInfo.Model.Dispose();

            return treesorDriveInfo;
        }

        #endregion Override DriveCmdletProvider methods
    }
}