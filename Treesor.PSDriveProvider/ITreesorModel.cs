using System;
using System.Collections.Generic;
using Treesor.Model;

namespace Treesor.PSDriveProvider
{
    /// <summary>
    /// Interface contract for data servcies
    /// </summary>
    public interface ITreesorModel : IDisposable
    {
        bool ItemExists(TreesorNodePath treesorNodePath);

        /// <summary>
        /// Retrieves the item specified by te given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        TreesorItem GetItem(TreesorNodePath path);

        void SetItem(TreesorNodePath rootPath, object value);

        void ClearItem(TreesorNodePath rootPath);

        TreesorItem NewItem(TreesorNodePath treesorNodePath, object newItemValue);

        void RemoveItem(TreesorNodePath treesorNodePath, bool recurse);

        /// <summary>
        /// Returns true if the item specifoed by the given path has children.
        /// </summary>
        /// <param name="treesorNodePath"></param>
        /// <returns></returns>
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

        TreesorColumn CreateColumn(string name, Type type);

        bool RemoveColumn(string propertyName);

        void RenameColumn(string name, string newName);

        IEnumerable<TreesorColumn> GetColumns();

        void CopyPropertyValue(TreesorNodePath sourcePath, string sourceProperty, TreesorNodePath destinationPath, string destinationProperty);

        void MovePropertyValue(TreesorNodePath sourcePath, string sourceProperty, TreesorNodePath destinationPath, string destinationProperty);
    }
}