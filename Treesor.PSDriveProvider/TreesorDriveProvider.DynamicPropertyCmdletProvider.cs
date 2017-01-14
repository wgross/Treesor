using NLog.Fluent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Treesor.PSDriveProvider
{
    public partial class TreesorDriveProvider : IDynamicPropertyCmdletProvider
    {
        #region IPropertyCmdletProvider Members

        public void GetProperty(string path, Collection<string> providerSpecificPickList)
        {
            log.Trace().Message($"{nameof(GetProperty)}({nameof(path)}='{path}',{nameof(providerSpecificPickList)}='{string.Join(",", providerSpecificPickList)}')").Write();

            this.WritePropertyObject(
                this.DriveInfo.Service.GetPropertyValue(TreesorNodePath.Parse(path), providerSpecificPickList.First()),
                path);
        }

        public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
        {
            log.Trace().Message($"{nameof(GetPropertyDynamicParameters)}({nameof(path)}='{path}',{nameof(providerSpecificPickList)}='{string.Join(",", providerSpecificPickList)}')").Write();

            return null;
        }

        public void SetProperty(string path, PSObject propertyValue)
        {
            log.Trace().Message($"{nameof(SetProperty)}({nameof(path)}='{path}',{nameof(propertyValue)}='{propertyValue}')").Write();
            var property = propertyValue.Properties.First();
            this.DriveInfo.Service.SetPropertyValue(TreesorNodePath.Create(path), property.Name, property.Value);
        }

        public object SetPropertyDynamicParameters(string path, PSObject propertyValue)
        {
            log.Trace().Message($"{nameof(SetPropertyDynamicParameters)}({nameof(path)}='{path}',{nameof(propertyValue)}='{propertyValue}')").Write();

            return null;
        }

        public void ClearProperty(string path, Collection<string> propertyToClear)
        {
            log.Trace().Message($"{nameof(ClearProperty)}({nameof(path)}='{path}',{nameof(propertyToClear)}='{propertyToClear}')").Write();

            this.DriveInfo.Service.ClearPropertyValue(TreesorNodePath.Parse(path), propertyToClear.First());
        }

        public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
        {
            log.Trace().Message($"{nameof(ClearPropertyDynamicParameters)}({nameof(path)}='{path}',{nameof(propertyToClear)}='{propertyToClear}')").Write();

            return null;
        }

        #endregion IPropertyCmdletProvider Members

        #region IDynamicPropertyCmdletProvider Members

        public void NewProperty(string path, string propertyName, string propertyTypeName, object value)
        {
            log.Trace().Message($"{nameof(NewProperty)}({nameof(path)}='{path}',{nameof(propertyName)}='{propertyName}',{nameof(propertyTypeName)}='{propertyTypeName}')").Write();

            this.WriteError(new ErrorRecord(
                exception: new PSNotSupportedException("Treesor provider doesn't support property creation"),
                errorId: "newItemPropertyNotSupported",
                errorCategory: ErrorCategory.NotImplemented,
                targetObject: path)
            {
                ErrorDetails = new ErrorDetails("To create properties use New-TreesorColumn to allocate a new property for all items.")
            });
        }

        public object NewPropertyDynamicParameters(string path, string propertyName, string propertyTypeName, object value)
        {
            log.Trace().Message($"{nameof(NewPropertyDynamicParameters)}({nameof(path)}='{path}',{nameof(propertyName)}='{propertyName}',{nameof(propertyTypeName)}='{propertyTypeName}')").Write();

            return null;
        }

        public void RemoveProperty(string path, string propertyName)
        {
            log.Trace().Message($"{nameof(RemoveProperty)}({nameof(path)}='{path}',{nameof(propertyName)}='{propertyName}')").Write();

            this.WriteError(new ErrorRecord(
              exception: new PSNotSupportedException("Treesor provider doesn't support property removal"),
              errorId: "removeItemPropertyNotSupported",
              errorCategory: ErrorCategory.NotImplemented,
              targetObject: path)
            {
                ErrorDetails = new ErrorDetails("To remove properties use Remove-TreesorColumn to deallocate a property from all items or clear the property value at a specific item.")
            });
        }

        public object RemovePropertyDynamicParameters(string path, string propertyName)
        {
            log.Trace().Message($"{nameof(RemoveProperty)}({nameof(path)}='{path}',{nameof(propertyName)}='{propertyName}')").Write();

            return null;
        }

        public void RenameProperty(string path, string sourceProperty, string destinationProperty)
        {
            log.Trace().Message($"{nameof(RenameProperty)}({nameof(path)}='{path}',{nameof(sourceProperty)}='{sourceProperty}',{nameof(destinationProperty)}='{destinationProperty}')").Write();

            this.WriteError(new ErrorRecord(
              exception: new PSNotSupportedException("Treesor provider doesn't support property renaming"),
              errorId: "renameItemPropertyNotSupported",
              errorCategory: ErrorCategory.NotImplemented,
              targetObject: path)
            {
                ErrorDetails = new ErrorDetails("To rename properties use Rename-TreesorColumn whichs changes ths name of this property at all items.")
            });
        }

        public object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty)
        {
            log.Trace().Message($"{nameof(RenamePropertyDynamicParameters)}({nameof(path)}='{path}',{nameof(sourceProperty)}='{sourceProperty}',{nameof(destinationProperty)}='{destinationProperty}')").Write();

            return null;
        }

        public void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            log.Trace().Message($"{nameof(CopyProperty)}({nameof(sourcePath)}='{sourcePath}',{nameof(sourceProperty)}='{sourceProperty}',{nameof(destinationPath)}='{destinationPath}',{nameof(destinationProperty)}='{destinationProperty}')").Write();

            this.DriveInfo.Service.CopyPropertyValue(TreesorNodePath.Create(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        }

        public object CopyPropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            log.Trace().Message($"{nameof(CopyPropertyDynamicParameters)}({nameof(sourcePath)}='{sourcePath}',{nameof(sourceProperty)}='{sourceProperty}',{nameof(destinationPath)}='{destinationPath}',{nameof(destinationProperty)}='{destinationProperty}')").Write();

            return null;
        }

        public void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            log.Trace().Message($"{nameof(MoveProperty)}({nameof(sourcePath)}='{sourcePath}',{nameof(sourceProperty)}='{sourceProperty}',{nameof(destinationPath)}='{destinationPath}',{nameof(destinationProperty)}='{destinationProperty}')").Write();

            this.DriveInfo.Service.MovePropertyValue(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Create(destinationPath), destinationProperty);
        }

        public object MovePropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            log.Trace().Message($"{nameof(MovePropertyDynamicParameters)}({nameof(sourcePath)}='{sourcePath}',{nameof(sourceProperty)}='{sourceProperty}',{nameof(destinationPath)}='{destinationPath}',{nameof(destinationProperty)}='{destinationProperty}')").Write();

            return null;
        }

        #endregion IDynamicPropertyCmdletProvider Members
    }
}