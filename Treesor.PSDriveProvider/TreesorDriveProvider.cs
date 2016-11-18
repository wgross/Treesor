namespace Treesor.PSDriveProvider
{
    using NLog;
    using NLog.Fluent;
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Provider;

    [CmdletProvider("Treesor", ProviderCapabilities.None)]
    public partial class TreesorDriveProvider : NavigationCmdletProvider // ContainerCmdletProvider
    //  NavigationCmdletProvider //, IPropertyCmdletProvider, IDynamicPropertyCmdletProvider
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TreesorDriveInfo GetTreesorDriveInfo()
        {
            return (TreesorDriveInfo)this.PSDriveInfo;
        }

        #region Override DriveCmdletProvider methods

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            log.Trace().Message($"{nameof(NewDrive)}(drive.Root='{drive.Root}',drive.Name='{drive.Name}')").Write();

            // Check if the drive object is null.
            if (drive == null)
            {
                this.WriteError(new ErrorRecord(new ArgumentNullException("drive"), "NullDrive", ErrorCategory.InvalidArgument, targetObject: null));

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

            TreesorDriveInfo treesorDriveInfo = drive as TreesorDriveInfo;

            if (treesorDriveInfo == null)
            {
                treesorDriveInfo = new TreesorDriveInfo(drive);
            }

            return treesorDriveInfo;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            log.Trace().Message($"{nameof(InitializeDefaultDrives)}()").Write();

            return new Collection<PSDriveInfo> { TreesorDriveInfo.CreateDefault(this.ProviderInfo) };
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            TreesorDriveInfo treesorDriveInfo = drive as TreesorDriveInfo;

            if (treesorDriveInfo == null)
            {
                this.WriteError(new ErrorRecord(new ArgumentNullException(nameof(drive)), "NullDrive", ErrorCategory.InvalidArgument, null));
                return null;
            }

            treesorDriveInfo.RemovingDrive();

            return treesorDriveInfo;
        }

        #endregion Override DriveCmdletProvider methods
    }
}