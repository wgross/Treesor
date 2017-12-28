using System.Management.Automation;

namespace Treesor.PSDriveProvider.Commands
{
    [Cmdlet(VerbsCommon.Get, "TreesorColumn", DefaultParameterSetName = currentDrive)]
    public class GetTreesorColumnCommand : TreesorColumnCommandBase
    {
        protected override void ProcessRecord()
        {
            foreach (var column in this.GetTreesorDriveProvider(this.GetDriveName())?.Model.GetColumns())
                this.WriteObject(column);
        }
    }
}