using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Commands
{
    public class TreesorColumnCommandBase : PSCmdlet
    {
        public const string currentDrive = "currentDrive";
        public const string otherDrive = "otherDrive";

        [Parameter(ParameterSetName = currentDrive, Mandatory = false, Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(TreesorDriveNameCompleter))]
        public string DriveName { get; set; } = null;

        protected string GetDriveName()
        {
            // if a drive isn't specified explicitely, splt the current path
            return this.DriveName ?? this.GetVariableValue("PWD").ToString().Split(":".ToCharArray()).FirstOrDefault();
        }

        protected TreesorDriveInfo GetTreesorDriveProvider(string treesorDriveName)
        {
            return this.SessionState.Provider
                .GetOne(TreesorDriveProvider.Id)
                .Drives
                .OfType<TreesorDriveInfo>()
                .FirstOrDefault(p => p.Name.Equals(treesorDriveName, System.StringComparison.OrdinalIgnoreCase));
        }
    }

    public class TreesorNamedColumnCommandBase : TreesorColumnCommandBase
    {
        [Parameter(ParameterSetName = currentDrive, Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = otherDrive, Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }
    }
}