using NLog.Fluent;
using System;
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

        #region IDynamicPropertyCmdltProvider Members

        public void NewProperty(string path, string propertyName, string propertyTypeName, object value)
        {
            throw new NotImplementedException();
        }

        public object NewPropertyDynamicParameters(string path, string propertyName, string propertyTypeName, object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveProperty(string path, string propertyName)
        {
            throw new NotImplementedException();
        }

        public object RemovePropertyDynamicParameters(string path, string propertyName)
        {
            throw new NotImplementedException();
        }

        public void RenameProperty(string path, string sourceProperty, string destinationProperty)
        {
            throw new NotImplementedException();
        }

        public object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty)
        {
            throw new NotImplementedException();
        }

        public void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            throw new NotImplementedException();
        }

        public object CopyPropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            throw new NotImplementedException();
        }

        public void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            throw new NotImplementedException();
        }

        public object MovePropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            throw new NotImplementedException();
        }

        #endregion IDynamicPropertyCmdltProvider Members

    }
}