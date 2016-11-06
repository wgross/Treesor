namespace Treesor.PSDriveProvider
{
    using Elementary.Hierarchy;
    using NLog;
    using NLog.Fluent;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Provider;

    [CmdletProvider("Treesor", ProviderCapabilities.None)]
    public class TreesorDriveProvider : ItemCmdletProvider
        //  NavigationCmdletProvider //, IPropertyCmdletProvider, IDynamicPropertyCmdletProvider
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TreesorDriveInfo GetTreesorDriveInfo()
        {
            return (TreesorDriveInfo)this.PSDriveInfo;
        }

        #region Override DriveCmdletProvider methods

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            log.Trace().Message($"{nameof(NewDrive)}(drive.Root='{drive.Root}',drive.Name='{drive.Name}')").Write();

            // Check if the drive object is null.
            if (drive == null)
            {
                this.WriteError(new ErrorRecord(new ArgumentNullException("drive"), "NullDrive", ErrorCategory.InvalidArgument, targetObject: null));

                return null;
            }

            // Check if the drive root is not null or empty
            // and if it is an existing file.
            //if (String.IsNullOrEmpty(drive.Root) || (File.Exists(drive.Root) == false))
            //{
            //    WriteError(new ErrorRecord(
            //               new ArgumentException("drive.Root"),
            //               "Nof",
            //               ErrorCategory.InvalidArgument,
            //               drive));

            //    return null;
            //}

            TreesorDriveInfo treesorDriveInfo = drive as TreesorDriveInfo;

            if (treesorDriveInfo == null)
            {
                treesorDriveInfo = new TreesorDriveInfo(drive);
            }

            return treesorDriveInfo;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            log.Trace().Message($"{nameof(InitializeDefaultDrives)}()").Write();

            return new Collection<PSDriveInfo> { TreesorDriveInfo.CreateDefault(this.ProviderInfo) };
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            TreesorDriveInfo treesorDriveInfo = drive as TreesorDriveInfo;

            if (treesorDriveInfo == null)
            {
                this.WriteError(new ErrorRecord(new ArgumentNullException(nameof(drive)), "NullDrive", ErrorCategory.InvalidArgument, null));
                return null;
            }

            treesorDriveInfo.RemovingDrive();

            return treesorDriveInfo;
        }

        #endregion Override DriveCmdletProvider methods

        #region Override ItemCmdletProvider methods

        protected override bool IsValidPath(string path)
        {
            log.Trace().Message($"{nameof(IsValidPath)}({nameof(path)}={path})").Write();

            TreesorNodePath parsedPath;
            return TreesorNodePath.TryParse(path, out parsedPath);
        }

        //protected override void ClearItem(string path)
        //{
        //    log.Trace().Message($"{nameof(ClearItem)}({nameof(path)}={path})").Write();

        //    this.GetTreesorDriveInfo().ClearItem(TreesorNodePath.Parse(path));
        //}

        //protected override string[] ExpandPath(string path)
        //{
        //    log.Trace().Message($"{nameof(ExpandPath)}({nameof(path)}={path})").Write();

        //    throw new NotImplementedException("ExpandPath");
        //}

        //protected override void GetItem(string path)
        //{
        //    log.Trace().Message($"{nameof(GetItem)}({nameof(path)}={path})").Write();

        //    var treesorNodePath = TreesorNodePath.Parse(path);

        //    var item = this.GetTreesorDriveInfo().GetItem(treesorNodePath);

        //    log.Debug()
        //        .Message($"{nameof(GetItem)}:Sending to pipe:{nameof(this.WriteItemObject)}({nameof(item)}.GetHashCode={item?.GetHashCode()},{nameof(item.Path)}={item.Path},isContainer={item is TreesorContainerItem})")
        //        .Write();

        //    this.WriteItemObject(item, path, isContainer: item is TreesorContainerItem);
        //}


        //protected override bool ItemExists(string path)
        //{
        //    log.Trace().Message($"{nameof(ItemExists)}({nameof(path)}={path})").Write();

        //    return this.GetTreesorDriveInfo().ItemExists(TreesorNodePath.Parse(path));
        //}

        //protected override void SetItem(string path, object value)
        //{
        //    log.Trace().Message($"{nameof(SetItem)}({nameof(path)}={path},{nameof(value)}.GetHashCode={value?.GetHashCode()})").Write();

        //    this.GetTreesorDriveInfo().SetItem(TreesorNodePath.Parse(path), value);
        //}

        #endregion Override ItemCmdletProvider methods

        //#region Override ContainerCmdletProvider methods

        //protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
        //{
        //    log.Trace().Message($"{nameof(ConvertPath)}({nameof(path)}={path},{nameof(filter)}={filter})").Write();

        //    updatedPath = path; // TreesorNodePath.Parse(path).ToString();
        //    updatedFilter = filter;

        //    return true;
        //}

        //protected override void CopyItem(string path, string copyPath, bool recurse)
        //{
        //    log.Trace().Message($"{nameof(CopyItem)}({nameof(path)}={path},{nameof(copyPath)}={copyPath},{nameof(recurse)}={recurse})").Write();

        //    this.GetTreesorDriveInfo().CopyItem(sourcePath: TreesorNodePath.Parse(path), destinationPath: TreesorNodePath.Parse(copyPath), recurse: recurse);
        //}

        //protected override void GetChildItems(string path, bool recurse)
        //{
        //    log.Trace().Message($"{nameof(GetChildItems)}({nameof(path)}={path},{nameof(recurse)}={recurse})").Write();

        //    foreach (var childItem in this.GetTreesorDriveInfo().GetChildItem(TreesorNodePath.Parse(path), recurse))
        //    {
        //        log.Trace()
        //            .Message($"{nameof(GetChildItems)}:Sending to pipe:{nameof(this.WriteItemObject)}({nameof(childItem)}.GetHashCode={childItem?.GetHashCode()},{nameof(childItem.Path)}={childItem.Path},isContainer={childItem is TreesorContainerItem})")
        //            .Write();

        //        this.WriteItemObject(childItem, childItem.Path.ToString(), childItem is TreesorContainerItem);
        //    }
        //}

        //protected override void GetChildNames(string path, ReturnContainers returnContainers)
        //{
        //    log.Trace().Message($"{nameof(GetChildNames)}({nameof(path)}={path},{nameof(returnContainers)}={returnContainers})").Write();

        //    foreach (var childName in this.GetTreesorDriveInfo().GetChildNames(TreesorNodePath.Parse(path), returnContainers))
        //    {
        //        log.Trace()
        //            .Message($"{nameof(GetChildNames)}:Sending to pipe:{nameof(this.WriteItemObject)}({nameof(childName)}={childName},{nameof(path)}={path},isContainer={true})")
        //            .Write();

        //        this.WriteItemObject(childName, path, true);
        //    }
        //}

        //protected override bool HasChildItems(string path)
        //{
        //    log.Trace().Message($"{nameof(HasChildItems)}({nameof(path)}={path})").Write();

        //    var hasChildItems = this.GetTreesorDriveInfo().HasChildItems(TreesorNodePath.Parse(path));

        //    log.Trace().Message($"{nameof(HasChildItems)}({nameof(path)}={path})->{hasChildItems}").Write();

        //    return hasChildItems;
        //}

        //protected override void NewItem(string path, string itemTypeName, object newItemValue)
        //{
        //    log.Trace()
        //        .Message($"{nameof(NewItem)}({nameof(path)}='{path}',{nameof(itemTypeName)}='{itemTypeName}',{nameof(newItemValue)}='{newItemValue}')")
        //        .Write();

        //    bool? isContainer;
        //    TreesorNode newItem;

        //    if (this.DynamicParameters != null && ((TreesorNewItemDynamicParameters)this.DynamicParameters).NullValue.IsPresent)
        //        newItem = this.GetTreesorDriveInfo().NewItem(TreesorNodePath.Parse(path), itemTypeName, TreesorDriveInfo.NullValue, out isContainer);
        //    else
        //        newItem = this.GetTreesorDriveInfo().NewItem(TreesorNodePath.Parse(path), itemTypeName, newItemValue, out isContainer);

        //    log.Trace()
        //           .Message($"{nameof(NewItem)}:Sending to pipe:{nameof(this.WriteItemObject)}({nameof(newItem)}.GetHashCode='{newItem?.GetHashCode()}',{nameof(path)}='{path}',isContainer='{isContainer.GetValueOrDefault(false)}'")
        //           .Write();

        //    this.WriteItemObject(newItem, path, isContainer.GetValueOrDefault(false));
        //}

        //protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
        //{
        //    log.Trace()
        //       .Message($"{nameof(NewItemDynamicParameters)}({nameof(path)}='{path}',{nameof(itemTypeName)}='{itemTypeName}',{nameof(newItemValue)}='{newItemValue}')")
        //       .Write();

        //    return this.GetTreesorDriveInfo().NewItemDynamicParameters(TreesorNodePath.Parse(path), itemTypeName, newItemValue);
        //}

        //protected override void RemoveItem(string path, bool recurse)
        //{
        //    log.Trace().Message($"{nameof(RemoveItem)}({nameof(path)}={path},{nameof(recurse)}={recurse})").Write();

        //    this.GetTreesorDriveInfo().RemoveItem(TreesorNodePath.Parse(path), recurse);
        //}

        //protected override void RenameItem(string path, string newName)
        //{
        //    log.Debug("Processing RenameItem({0}, {1})", path ?? "null", newName ?? "null");

        //    var renamedItem = this.GetTreesorDriveInfo().RenameItem(TreesorNodePath.Parse(path), newName);
        //}

        //#endregion Override ContainerCmdletProvider methods

        //#region Override NavigationCmdletProvider methods

        //protected override void MoveItem(string path, string destination)
        //{
        //    log.Debug("Processing MoveItem({0}, {1})", path ?? "null", destination ?? "null");

        //    this.GetTreesorDriveInfo().MoveItem(path: TreesorNodePath.Parse(path), destination: TreesorNodePath.Parse(destination));
        //}

        //protected override string GetChildName(string path)
        //{
        //    log.Trace().Message($"{nameof(GetChildName)}({nameof(path)}='{path}')").Write();

        //    var childName = TreesorNodePath.Parse(path).HierarchyPath.Leaf().ToString();

        //    log.Debug().Message($"{nameof(GetChildName)}({nameof(path)}='{path}')->'{childName}'").Write();

        //    return childName;
        //}

        //protected override string MakePath(string parent, string child)
        //{
        //    log.Trace().Message($"{nameof(MakePath)}({nameof(parent)}='{parent}',{nameof(child)}='{child}')").Write();

        //    var result = TreesorNodePath.Create(TreesorNodePath.Parse(parent).HierarchyPath.Join(TreesorNodePath.Parse(child).HierarchyPath).Items.ToArray()).HierarchyPath.ToString();

        //    log.Trace().Message($"{nameof(MakePath)}({nameof(parent)}='{parent}',{nameof(child)}='{child}')->'{result}'").Write();

        //    return result;
        //}

        //protected override string GetParentPath(string path, string root)
        //{
        //    log.Trace().Message($"{nameof(GetParentPath)}({nameof(path)}='{path}',{nameof(root)}='{root}')").Write();

        //    var parsedPath = TreesorNodePath.Parse(path).HierarchyPath;
        //    string result;
        //    if (parsedPath.HasParentNode)
        //        result = parsedPath.Parent().ToString();
        //    else
        //        result = root;

        //    log.Debug().Message($"{nameof(GetParentPath)}({nameof(path)}='{path}',{nameof(root)}='{root}')->'{result}'").Write();
        //    return result;
        //}

        //protected override bool IsItemContainer(string path)
        //{
        //    log.Trace().Message($"{nameof(IsItemContainer)}({nameof(path)}={path})").Write();

        //    return (this.GetTreesorDriveInfo().GetItem(TreesorNodePath.Parse(path)) != null);
        //}

        //#endregion Override NavigationCmdletProvider methods

        //#region IPropertyCmdletProvider Members

        //public void ClearProperty(string path, Collection<string> propertyToClear)
        //{
        //    this.GetTreesorDriveInfo().ClearProperty(TreesorNodePath.Parse(path), propertyToClear);
        //}

        //public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
        //{
        //    log.Debug("Processing ClearPropertyDynamicParameters({0}, {1})", path ?? "null", string.Join(",", propertyToClear));

        //    return this.GetTreesorDriveInfo().ClearPropertyDynamicParameters(TreesorNodePath.Parse(path), propertyToClear);
        //}

        //public void GetProperty(string path, Collection<string> providerSpecificPickList)
        //{
        //    log.Debug("Processing: GetProperty({0},{1})", path ?? "null", string.Join(",", providerSpecificPickList));

        //    var value = this.GetTreesorDriveInfo().GetProperty(TreesorNodePath.Parse(path), providerSpecificPickList);
        //    this.WritePropertyObject(value, path);
        //}

        //public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
        //{
        //    log.Debug("Processing GetPropertyDynamicParameters({0}, {1})", path ?? "null", string.Join(",", providerSpecificPickList));

        //    return this.GetTreesorDriveInfo().GetPropertyDynamicParameters(TreesorNodePath.Parse(path), providerSpecificPickList);
        //}

        //public void SetProperty(string path, PSObject propertyValue)
        //{
        //    log.Debug("Processing SetProperty({0}, {1})", path ?? "null", propertyValue.ToString());

        //    this.GetTreesorDriveInfo().SetProperty(TreesorNodePath.Parse(path), propertyValue);
        //}

        //public object SetPropertyDynamicParameters(string path, PSObject propertyValue)
        //{
        //    log.Debug("Processing SetPropertyDynamicParameters({0}, {1})", path ?? "null", propertyValue);

        //    return this.GetTreesorDriveInfo().SetPropertyDynamicParameters(TreesorNodePath.Parse(path), propertyValue);
        //}

        //#endregion IPropertyCmdletProvider Members

        //#region IDynamicPropertyCmdletProvider Members

        //public void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        //{
        //    log.Debug("Processing: CopyProperty({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

        //    this.GetTreesorDriveInfo().CopyProperty(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        //}

        //public object CopyPropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        //{
        //    log.Debug("Processing: CopyPropertyDynamicParameters({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

        //    return this.GetTreesorDriveInfo().CopyPropertyDynamicParameters(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        //}

        //public void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        //{
        //    log.Debug("Processing: MoveProperty({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

        //    this.GetTreesorDriveInfo().MoveProperty(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        //}

        //public object MovePropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        //{
        //    log.Debug("Processing: MovePropertyDynamicParameters({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

        //    return this.GetTreesorDriveInfo().MovePropertyDynamicParameters(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        //}

        //public void NewProperty(string path, string propertyName, string propertyTypeName, object value)
        //{
        //    log.Debug("Processing: NewProperty({0}, {1}, {2}, {3}", path ?? "null", propertyTypeName ?? "null", propertyTypeName ?? "null", value ?? (object)"null");

        //    this.GetTreesorDriveInfo().NewProperty(TreesorNodePath.Parse(path), propertyName, propertyTypeName, value);
        //}

        //public object NewPropertyDynamicParameters(string path, string propertyName, string propertyTypeName, object value)
        //{
        //    log.Debug("Processing: NewPropertyDynamicParameters({0}, {1}, {2}, {3}", path ?? "null", propertyTypeName ?? "null", propertyTypeName ?? "null", value ?? (object)"null");

        //    return this.GetTreesorDriveInfo().NewPropertyDynamicParameters(TreesorNodePath.Parse(path), propertyName, propertyTypeName, value);
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