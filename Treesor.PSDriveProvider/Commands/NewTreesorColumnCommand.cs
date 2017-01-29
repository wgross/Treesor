using System;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Commands
{
    [Cmdlet(VerbsCommon.New, "TreesorColumn", DefaultParameterSetName = currentDrive)]
    public class NewTreesorColumnCommand : TreesorNamedColumnCommandBase
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public object ColumnType { get; set; }
        
        protected override void ProcessRecord()
        {
            string typeName = null;
            if (this.ColumnType is string)
                typeName = (string)this.ColumnType;
            else if (this.ColumnType is Type)
                typeName = this.ColumnType.ToString();
            else
                typeName = this.ColumnType.ToString();
            
            this.WriteObject(this.GetTreesorDriveProvider(this.GetDriveName())?
                .Service.CreateColumn(this.Name, Type.GetType(typeName, throwOnError: true, ignoreCase: true)));
        }
    }
}