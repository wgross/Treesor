using System;
using System.Collections.Generic;
using Treesor.Model;

namespace Treesor.Model
{
    /// <summary>
    /// Interface contract for data servcies
    /// </summary>
    public interface ITreesorModel : IDisposable
    {
        bool ItemExists(TreesorItemPath treesorNodePath);

        /// <summary>
        /// Retrieves the item specified by te given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        TreesorItem GetItem(TreesorItemPath path);

        void SetItem(TreesorItemPath rootPath, object value);

        void ClearItem(TreesorItemPath rootPath);

        TreesorItem NewItem(TreesorItemPath treesorNodePath, object newItemValue);

        void RemoveItem(TreesorItemPath treesorNodePath, bool recurse);

        /// <summary>
        /// Returns true if the item specifoed by the given path has children.
        /// </summary>
        /// <param name="treesorNodePath"></param>
        /// <returns></returns>
        bool HasChildItems(TreesorItemPath treesorNodePath);

        IEnumerable<TreesorItem> GetChildItemsByWildcard(TreesorItemPath treesorNodePath);

        IEnumerable<TreesorItem> GetChildItems(TreesorItemPath treesorNodePath);

        IEnumerable<TreesorItem> GetDescendants(TreesorItemPath treesorNodePath);

        void CopyItem(TreesorItemPath path, TreesorItemPath destinationPath, bool recurse);

        void RenameItem(TreesorItemPath treesorNodePath, string newName);

        void MoveItem(TreesorItemPath path, TreesorItemPath destination);

        void SetPropertyValue(TreesorItemPath rootPath, string name, object value);

        object GetPropertyValue(TreesorItemPath rootPath, string name);

        void ClearPropertyValue(TreesorItemPath treesorNodePath, string name);

        TreesorColumn CreateColumn(string name, Type type);

        bool RemoveColumn(string propertyName);

        void RenameColumn(string name, string newName);

        IEnumerable<TreesorColumn> GetColumns();

        void CopyPropertyValue(TreesorItemPath sourcePath, string sourceProperty, TreesorItemPath destinationPath, string destinationProperty);

        void MovePropertyValue(TreesorItemPath sourcePath, string sourceProperty, TreesorItemPath destinationPath, string destinationProperty);
    }
}