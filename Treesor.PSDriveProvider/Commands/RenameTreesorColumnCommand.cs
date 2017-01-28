using System.Management.Automation;

namespace Treesor.PSDriveProvider.Commands
{
    [Cmdlet(VerbsCommon.Rename, "TreesorColumn", DefaultParameterSetName = TreesorColumnCommandBase.currentDrive)]
    public class RenameTreesorColumnCommand : TreesorColumnCommandBase
    {
        [Parameter(ParameterSetName = currentDrive, Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = otherDrive, Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string NewName { get; set; }

        protected override void ProcessRecord()
        {
            this.GetTreesorDriveProvider(this.GetDriveName())?.Service.RenameColumn(this.Name, this.NewName);
        }
    }
}