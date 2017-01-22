using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Commands
{
    [Cmdlet(VerbsCommon.Remove, "TreesorColumn", DefaultParameterSetName = "pathSet")]
    public class RemoveTreesorColumnCommand : PSCmdlet
    {
        // ACT
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(TreesorDriveNameCompleter))]
        public string DriveName { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            var driveToUse = this.SessionState.Provider
                .GetOne(TreesorDriveProvider.Id)
                .Drives
                .OfType<TreesorDriveInfo>()
                .FirstOrDefault(p => p.Name.Equals(this.DriveName, System.StringComparison.OrdinalIgnoreCase));

            driveToUse.Service.RemoveColumn(this.Name);
        }
    }
}