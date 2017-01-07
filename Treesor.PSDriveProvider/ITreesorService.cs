using System;
using System.Collections.Generic;

namespace Treesor.PSDriveProvider
{
    /// <summary>
    /// Interface contract for data servcies
    /// </summary>
    public interface ITreesorService : IDisposable
    {
        bool ItemExists(TreesorNodePath treesorNodePath);

        TreesorItem GetItem(TreesorNodePath rootPath);

        void SetItem(TreesorNodePath rootPath, object value);

        void ClearItem(TreesorNodePath rootPath);

        TreesorItem NewItem(TreesorNodePath treesorNodePath, object newItemValue);

        void RemoveItem(TreesorNodePath treesorNodePath, bool recurse);

        bool HasChildItems(TreesorNodePath treesorNodePath);

        IEnumerable<TreesorItem> GetChildItemsByWildcard(TreesorNodePath treesorNodePath);

        IEnumerable<TreesorItem> GetChildItems(TreesorNodePath treesorNodePath);

        IEnumerable<TreesorItem> GetDescendants(TreesorNodePath treesorNodePath);

        void CopyItem(TreesorNodePath path, TreesorNodePath destinationPath, bool recurse);

        void RenameItem(TreesorNodePath treesorNodePath, string newName);

        void MoveItem(TreesorNodePath path, TreesorNodePath destination);

        void SetPropertyValue(TreesorNodePath rootPath, string name, object value);

        object GetPropertyValue(TreesorNodePath rootPath, string name);

        void ClearPropertyValue(TreesorNodePath treesorNodePath, string name);

        void NewProperty(TreesorNodePath treesorNodePath, string propertyName, string propertyTypeName, object value);

        void RemoveProperty(TreesorNodePath treesorNodePath, string propertyName);

        void CopyPropertyValue(TreesorNodePath sourcePath, string sourceProperty, TreesorNodePath destinationPath, string destinationProperty);

        void MovePropertyValue(TreesorNodePath sourcePath, string sourceProperty, TreesorNodePath destinationPath, string destinationProperty);

        void RenameProperty(TreesorNodePath path, string sourceProperty, string destinationProperty);
    }
}