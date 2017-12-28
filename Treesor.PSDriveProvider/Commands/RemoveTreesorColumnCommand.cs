using System.Management.Automation;

namespace Treesor.PSDriveProvider.Commands
{
    [Cmdlet(VerbsCommon.Remove, "TreesorColumn", DefaultParameterSetName = currentDrive)]
    public class RemoveTreesorColumnCommand : TreesorNamedColumnCommandBase
    {
        protected override void ProcessRecord()
        {
            this.GetTreesorDriveProvider(this.GetDriveName())?.Model.RemoveColumn(this.Name);
        }
    }
}