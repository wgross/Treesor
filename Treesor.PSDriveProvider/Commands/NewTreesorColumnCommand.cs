using System;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Commands
{
    [Cmdlet(VerbsCommon.New, "TreesorColumn", DefaultParameterSetName = TreesorColumnCommandBase.currentDrive)]
    public class NewTreesorColumnCommand : TreesorColumnCommandBase
    {
        [Parameter(ParameterSetName = currentDrive, Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = otherDrive, Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string TypeName { get; set; }

        protected override void ProcessRecord()
        {
            this.GetTreesorDriveProvider(this.GetDriveName())?.Service
                .CreateColumn(this.Name, Type.GetType(this.TypeName, throwOnError: true, ignoreCase: true));
        }
    }
}