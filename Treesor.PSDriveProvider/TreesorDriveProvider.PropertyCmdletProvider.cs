using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using System.Threading.Tasks;

namespace Treesor.PSDriveProvider
{
    public partial class TreesorDriveProvider : IPropertyCmdletProvider
    {
        public void GetProperty(string path, Collection<string> providerSpecificPickList)
        {
            throw new NotImplementedException();
        }

        public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
        {
            throw new NotImplementedException();
        }
    }
}
