using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Commands
{
    [Cmdlet(VerbsCommon.Rename, "TreesorColumn", DefaultParameterSetName = "pathSet")]
    public class RenameTreesorColumnCommand : PSCmdlet
    {
        // ACT
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(TreesorDriveNameCompleter))]
        public string DriveName { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string NewName { get; set; }

        protected override void ProcessRecord()
        {
            var driveToUse = this.SessionState.Provider
                .GetOne(TreesorDriveProvider.Id)
                .Drives
                .OfType<TreesorDriveInfo>()
                .FirstOrDefault(p => p.Name.Equals(this.DriveName, System.StringComparison.OrdinalIgnoreCase));

            driveToUse.Service.RenameColumn(this.Name, this.NewName);
        }
    }
}