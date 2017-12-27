using NLog.Fluent;
using System;
using System.Management.Automation;
using Treesor.Model;

namespace Treesor.PSDriveProvider
{
    public partial class TreesorDriveProvider
    {
        #region Override ContainerCmdletProvider methods

        protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
        {
            log.Trace().Message($"{nameof(ConvertPath)}({nameof(path)}={path},{nameof(filter)}={filter})").Write();

            updatedPath = path; // TreesorNodePath.Parse(path).ToString();
            updatedFilter = filter;

            return true;
        }

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            log.Trace().Message($"{nameof(CopyItem)}({nameof(path)}={path},{nameof(copyPath)}={copyPath},{nameof(recurse)}={recurse})").Write();

            this.DriveInfo.Service.CopyItem(path: TreesorNodePath.Parse(path), destinationPath: TreesorNodePath.Parse(copyPath), recurse: recurse);
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            log.Trace().Message($"{nameof(GetChildItems)}({nameof(path)}={path},{nameof(recurse)}={recurse})").Write();

            foreach (var childItem in this.DriveInfo.GetChildItems(TreesorNodePath.Parse(path), recurse))
            {
                log.Trace()
                    .Message($"{nameof(GetChildItems)}:Sending to pipe:{nameof(this.WriteItemObject)}({nameof(childItem)}.GetHashCode={childItem?.GetHashCode()},{nameof(childItem.Path)}={childItem.Path},isContainer={childItem.IsContainer})")
                    .Write();

                this.WriteItemObject(childItem, childItem.Path.ToString(), childItem.IsContainer);
            }
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            log.Trace().Message($"{nameof(GetChildNames)}({nameof(path)}={path},{nameof(returnContainers)}={returnContainers})").Write();

            foreach (var childName in this.DriveInfo.GetChildNames(TreesorNodePath.Parse(path), returnContainers))
            {
                log.Trace()
                    .Message($"{nameof(GetChildNames)}:Sending to pipe:{nameof(this.WriteItemObject)}({nameof(childName)}={childName},{nameof(path)}={path},isContainer={true})")
                    .Write();

                this.WriteItemObject(childName, path, true);
            }
        }

        protected override bool HasChildItems(string path)
        {
            log.Trace().Message($"{nameof(HasChildItems)}({nameof(path)}={path})").Write();

            var hasChildItems = this.DriveInfo.Service.HasChildItems(TreesorNodePath.Parse(path));

            log.Trace().Message($"{nameof(HasChildItems)}({nameof(path)}={path})->{hasChildItems}").Write();

            return hasChildItems;
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            log.Trace()
                .Message($"{nameof(NewItem)}({nameof(path)}='{path}',{nameof(itemTypeName)}='{itemTypeName}',{nameof(newItemValue)}='{newItemValue}')")
                .Write();
            try
            {
                //if (this.DynamicParameters != null && ((TreesorNewItemDynamicParameters)this.DynamicParameters).NullValue.IsPresent)
                //    newItem = this.TreesorDriveInfo.NewItem(TreesorNodePath.Parse(path), itemTypeName, TreesorDriveInfo.NullValue, out isContainer);
                //else

                var newItem = this.DriveInfo.Service.NewItem(TreesorNodePath.Parse(path), newItemValue);

                log.Trace()
                    .Message($"{nameof(NewItem)}:Sending to pipe:{nameof(this.WriteItemObject)}({nameof(newItem)}.GetHashCode='{newItem?.GetHashCode()}',{nameof(path)}='{newItem.Path}',isContainer='{newItem.IsContainer}'")
                    .Write();

                this.WriteItemObject(newItem, path, newItem.IsContainer);
            }
            catch (TreesorModelException ex) when (ex.ErrorCode == TreesorModelErrorCodes.DuplicateItem)
            {
                this.WriteError(new ErrorRecord(ex, "newitem.1", ErrorCategory.ResourceExists, path));
            }
            catch (TreesorModelException ex) when (ex.ErrorCode == TreesorModelErrorCodes.NotImplemented)
            {
                this.WriteError(new ErrorRecord(ex, "newitem.2", ErrorCategory.NotImplemented, path));
            }
        }

        //protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
        //{
        //    log.Trace()
        //       .Message($"{nameof(NewItemDynamicParameters)}({nameof(path)}='{path}',{nameof(itemTypeName)}='{itemTypeName}',{nameof(newItemValue)}='{newItemValue}')")
        //       .Write();

        //    return this.TreesorDriveInfo.NewItemDynamicParameters(TreesorNodePath.Parse(path), itemTypeName, newItemValue);
        //}

        protected override void RemoveItem(string path, bool recurse)
        {
            log.Trace().Message($"{nameof(RemoveItem)}({nameof(path)}={path},{nameof(recurse)}={recurse})").Write();

            this.DriveInfo.Service.RemoveItem(TreesorNodePath.Parse(path), recurse);
        }

        protected override void RenameItem(string path, string newName)
        {
            log.Debug($"{nameof(RenameItem)}({nameof(path)}='{path}', {nameof(newName)}='{newName}'s)");

            this.DriveInfo.Service.RenameItem(TreesorNodePath.Parse(path), newName);
        }

        #endregion Override ContainerCmdletProvider methods

        //#region IPropertyCmdletProvider Members

        //public void ClearProperty(string path, Collection<string> propertyToClear)
        //{
        //    this.TreesorDriveInfo.ClearProperty(TreesorNodePath.Parse(path), propertyToClear);
        //}

        //public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
        //{
        //    log.Debug("Processing ClearPropertyDynamicParameters({0}, {1})", path ?? "null", string.Join(",", propertyToClear));

        //    return this.TreesorDriveInfo.ClearPropertyDynamicParameters(TreesorNodePath.Parse(path), propertyToClear);
        //}

        //public void GetProperty(string path, Collection<string> providerSpecificPickList)
        //{
        //    log.Debug("Processing: GetProperty({0},{1})", path ?? "null", string.Join(",", providerSpecificPickList));

        //    var value = this.TreesorDriveInfo.GetProperty(TreesorNodePath.Parse(path), providerSpecificPickList);
        //    this.WritePropertyObject(value, path);
        //}

        //public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
        //{
        //    log.Debug("Processing GetPropertyDynamicParameters({0}, {1})", path ?? "null", string.Join(",", providerSpecificPickList));

        //    return this.TreesorDriveInfo.GetPropertyDynamicParameters(TreesorNodePath.Parse(path), providerSpecificPickList);
        //}

        //public void SetProperty(string path, PSObject propertyValue)
        //{
        //    log.Debug("Processing SetProperty({0}, {1})", path ?? "null", propertyValue.ToString());

        //    this.TreesorDriveInfo.SetProperty(TreesorNodePath.Parse(path), propertyValue);
        //}

        //public object SetPropertyDynamicParameters(string path, PSObject propertyValue)
        //{
        //    log.Debug("Processing SetPropertyDynamicParameters({0}, {1})", path ?? "null", propertyValue);

        //    return this.TreesorDriveInfo.SetPropertyDynamicParameters(TreesorNodePath.Parse(path), propertyValue);
        //}

        //#endregion IPropertyCmdletProvider Members

        //#region IDynamicPropertyCmdletProvider Members

        //public void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        //{
        //    log.Debug("Processing: CopyProperty({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

        //    this.TreesorDriveInfo.CopyProperty(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        //}

        //public object CopyPropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        //{
        //    log.Debug("Processing: CopyPropertyDynamicParameters({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

        //    return this.TreesorDriveInfo.CopyPropertyDynamicParameters(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        //}

        //public void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        //{
        //    log.Debug("Processing: MoveProperty({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

        //    this.TreesorDriveInfo.MoveProperty(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        //}

        //public object MovePropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        //{
        //    log.Debug("Processing: MovePropertyDynamicParameters({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

        //    return this.TreesorDriveInfo.MovePropertyDynamicParameters(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        //}

        //public void NewProperty(string path, string propertyName, string propertyTypeName, object value)
        //{
        //    log.Debug("Processing: NewProperty({0}, {1}, {2}, {3}", path ?? "null", propertyTypeName ?? "null", propertyTypeName ?? "null", value ?? (object)"null");

        //    this.TreesorDriveInfo.NewProperty(TreesorNodePath.Parse(path), propertyName, propertyTypeName, value);
        //}

        //public object NewPropertyDynamicParameters(string path, string propertyName, string propertyTypeName, object value)
        //{
        //    log.Debug("Processing: NewPropertyDynamicParameters({0}, {1}, {2}, {3}", path ?? "null", propertyTypeName ?? "null", propertyTypeName ?? "null", value ?? (object)"null");

        //    return this.TreesorDriveInfo.NewPropertyDynamicParameters(TreesorNodePath.Parse(path), propertyName, propertyTypeName, value);
        //}

        //public void RemoveProperty(string path, string propertyName)
        //{
        //    throw new NotImplementedException();
        //}

        //public object RemovePropertyDynamicParameters(string path, string propertyName)
        //{
        //    throw new NotImplementedException();
        //}

        //public void RenameProperty(string path, string sourceProperty, string destinationProperty)
        //{
        //    throw new NotImplementedException();
        //}

        //public object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty)
        //{
        //    throw new NotImplementedException();
        //}

        // #endregion IDynamicPropertyCmdletProvider Members
    }
}