using System.Management.Automation;

namespace Treesor.PSDriveProvider.Commands
{
    [Cmdlet(VerbsCommon.Remove, "TreesorColumn", DefaultParameterSetName = TreesorColumnCommandBase.currentDrive)]
    public class RemoveTreesorColumnCommand : TreesorColumnCommandBase
    {
        protected override void ProcessRecord()
        {
            this.GetTreesorDriveProvider(this.GetDriveName())?.Service.RemoveColumn(this.Name);
        }
    }
}