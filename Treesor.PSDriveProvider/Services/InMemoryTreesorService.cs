using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treesor.PSDriveProvider
{
    public class InMemoryTreesorService : ITreesorService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static Func<string, ITreesorService> Factory { get; set; } = DefaultFactoryDelegate;

        private static ITreesorService DefaultFactoryDelegate(string type)
        {
            var hierarchy = new MutableHierarchy<string, Reference<Guid>>();
            Reference<Guid> id;
            if (!hierarchy.TryGetValue(HierarchyPath.Create<string>(), out id))
                hierarchy.Add(HierarchyPath.Create<string>(), new Reference<Guid>(Guid.NewGuid()));
            return new InMemoryTreesorService(hierarchy);
        }

        #region Construction and initialization of this instance

        private readonly IHierarchy<string, Reference<Guid>> hierarchy;

        private readonly IDictionary<string, TreesorColumn> columns;

        public InMemoryTreesorService(IHierarchy<string, Reference<Guid>> hierarchy)
        {
            this.hierarchy = hierarchy;
            this.columns = new Dictionary<string, TreesorColumn>();
        }

        #endregion Construction and initialization of this instance

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TreesorService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        public bool ItemExists(TreesorNodePath treesorNodePath)
        {
            Reference<Guid> id;
            return this.hierarchy.TryGetValue(treesorNodePath.HierarchyPath, out id);
        }

        public TreesorItem GetItem(TreesorNodePath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            Reference<Guid> id;
            if (this.hierarchy.TryGetValue(path.HierarchyPath, out id))
                return new TreesorItem(path, id);
            else return null;
        }

        private bool TryGetItem(TreesorNodePath path, out TreesorItem item)
        {
            Reference<Guid> id;
            if (this.hierarchy.TryGetValue(path.HierarchyPath, out id))
            {
                item = new TreesorItem(path, id);
                return true;
            }

            item = null;
            return false;
        }

        public void SetItem(TreesorNodePath treesorNodePath, object value)
        {
            throw new NotSupportedException($"A value for node {treesorNodePath} is not allowed");
        }

        public void ClearItem(TreesorNodePath rootPath)
        {
            // currently function isn't implmented.
            return;
        }

        public TreesorItem NewItem(TreesorNodePath treesorNodePath, object newItemValue)
        {
            // currently a value is not accepted.
            // a data model of the nodes is not yet decided

            if (newItemValue != null)
                throw new NotSupportedException($"A value for node {treesorNodePath} is not allowed");

            Reference<Guid> id;
            this.hierarchy.Add(treesorNodePath.HierarchyPath, id = new Reference<Guid>(Guid.NewGuid()));

            return new TreesorItem(treesorNodePath, id);
        }

        public void RemoveItem(TreesorNodePath treesorNodePath, bool recurse)
        {
            this.hierarchy.RemoveNode(treesorNodePath.HierarchyPath, recurse);
        }

        public bool HasChildItems(TreesorNodePath treesorNodePath)
        {
            // Result of Travesr is never null.
            // hierachy throws KeyNotFindException in this case.
            return this.hierarchy.Traverse(treesorNodePath.HierarchyPath).HasChildNodes;
        }

        public IEnumerable<TreesorItem> GetChildItemsByWildcard(TreesorNodePath treesorNodePath)
        {
            throw new InvalidOperationException();
        }

        public IEnumerable<TreesorItem> GetChildItems(TreesorNodePath treesorNodePath)
        {
            return this.hierarchy
                .Traverse(treesorNodePath.HierarchyPath)
                .Children()
                .Select(n => new TreesorItem(TreesorNodePath.Create(n.Path), n.Value));
        }

        public IEnumerable<TreesorItem> GetDescendants(TreesorNodePath treesorNodePath)
        {
            return this.hierarchy
                .Traverse(treesorNodePath.HierarchyPath)
                .Descendants()
                .Select(n => new TreesorItem(TreesorNodePath.Create(n.Path), n.Value));
        }

        public void CopyItem(TreesorNodePath path, TreesorNodePath destinationPath, bool recurse)
        {
            Reference<Guid> id;
            if (this.hierarchy.TryGetValue(path.HierarchyPath, out id))
            {
                Reference<Guid> destinationId;
                if (this.hierarchy.TryGetValue(destinationPath.HierarchyPath, out destinationId))
                {
                    // try create new item under existing destination
                    if (!this.hierarchy.TryGetValue(destinationPath.HierarchyPath.Join(path.HierarchyPath.Leaf()), out destinationId))
                        this.hierarchy.Add(destinationPath.HierarchyPath.Join(path.HierarchyPath.Leaf()), destinationId = new Reference<Guid>(Guid.NewGuid()));
                }
                else
                {
                    // create new item at destiontionPath
                    this.hierarchy.Add(destinationPath.HierarchyPath, destinationId = new Reference<Guid>(Guid.NewGuid()));
                    if (recurse)
                    {
                        // copy child nodes of source to destination
                        foreach (var source in this.hierarchy.Traverse(path.HierarchyPath).Descendants(depthFirst: false))
                            this.hierarchy.Add(destinationPath.HierarchyPath.Join(source.Path.RelativeToAncestor(path.HierarchyPath)), new Reference<Guid>(Guid.NewGuid()));
                    }
                }
            }
        }

        public void RenameItem(TreesorNodePath treesorNodePath, string newName)
        {
            Reference<Guid> id;
            if (this.hierarchy.TryGetValue(treesorNodePath.HierarchyPath, out id))
                if (!this.hierarchy.TryGetValue(treesorNodePath.HierarchyPath.Parent().Join(newName), out id))
                    if (this.hierarchy.Remove(treesorNodePath.HierarchyPath))
                        this.hierarchy.Add(treesorNodePath.HierarchyPath.Parent().Join(newName), id);
        }

        public void MoveItem(TreesorNodePath path, TreesorNodePath destinationPath)
        {
            Reference<Guid> id, destinationId;
            if (this.hierarchy.TryGetValue(path.HierarchyPath, out id))
                if (this.hierarchy.TryGetValue(destinationPath.HierarchyPath, out destinationId))
                {
                    // try create item under existing destination item
                    if (!this.hierarchy.TryGetValue(destinationPath.HierarchyPath.Join(path.HierarchyPath.Leaf()), out destinationId))
                    {
                        this.hierarchy.Add(destinationPath.HierarchyPath.Join(path.HierarchyPath.Leaf()), id);

                        // finally remove the source
                        this.hierarchy.Remove(path.HierarchyPath);
                    }
                }
                else
                {
                    this.hierarchy.Add(destinationPath.HierarchyPath, id);
                    // move item and its descendants to the destination
                    foreach (var source in this.hierarchy.Traverse(path.HierarchyPath).Descendants(depthFirst: false))
                        this.hierarchy.Add(destinationPath.HierarchyPath.Join(source.Path.RelativeToAncestor(path.HierarchyPath)), source.Value);

                    // finally remove the source
                    this.hierarchy.Remove(path.HierarchyPath);
                }
        }

        public void SetPropertyValue(TreesorNodePath path, string name, object value)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            TreesorItem item;
            if (!this.TryGetItem(path, out item))
                throw new InvalidOperationException($"Node '{path}' doesn't exist");

            TreesorColumn column = null;
            if (!this.columns.TryGetValue(name, out column))
                throw new InvalidOperationException($"Property '{name}' doesn't exist");

            column.SetValue(this.GetItem(path).IdRef, value);
        }

        public object GetPropertyValue(TreesorNodePath path, string name)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            TreesorColumn column = null;
            if (!this.columns.TryGetValue(name, out column))
                throw new InvalidOperationException($"Property '{name}' doesn't exist");

            TreesorItem item;
            if (!this.TryGetItem(path, out item))
                throw new InvalidOperationException($"Node '{path}' doesn't exist");

            return column.GetValue(item.IdRef);
        }

        public void ClearPropertyValue(TreesorNodePath path, string name)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            TreesorColumn column = null;
            if (!this.columns.TryGetValue(name, out column))
                throw new InvalidOperationException($"Property '{name}' doesn't exist");

            TreesorItem item;
            if (!this.TryGetItem(path, out item))
                throw new InvalidOperationException($"Node '{path}' doesn't exist");

            column.UnsetValue(item);
        }

        #region Create a new columns in treesor data model

        public TreesorColumn CreateColumn(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            TreesorColumn column;
            if (this.columns.TryGetValue(name, out column))
            {
                if (type.Equals(column.Type))
                    return column;

                throw new InvalidOperationException($"Column: '{name}' already defined with type: '{column.Type}'");
            }

            this.columns.Add(name, column = new TreesorColumn(name, type));
            return column;
        }

        #endregion Create a new columns in treesor data model

        public void CopyPropertyValue(TreesorNodePath sourcePath, string sourceProperty, TreesorNodePath destinationPath, string destinationProperty)
        {
            this.SetPropertyValue(destinationPath, destinationProperty, this.GetPropertyValue(sourcePath, sourceProperty));
        }

        public void MovePropertyValue(TreesorNodePath sourcePath, string sourceProperty, TreesorNodePath destinationPath, string destinationProperty)
        {
            this.SetPropertyValue(destinationPath, destinationProperty, this.GetPropertyValue(sourcePath, sourceProperty));
            this.ClearPropertyValue(sourcePath, sourceProperty);
        }

        public void RenameColumn(string name, string newName)
        {
            TreesorColumn column;
            if (!this.columns.TryGetValue(name, out column))
                return;

            this.columns.Remove(name);
            this.columns.Add(newName, new TreesorColumn(newName, column.Type));
        }

        public bool RemoveColumn(string propertyName)
        {
            return this.columns.Remove(propertyName);
        }

        public IEnumerable<TreesorColumn> GetColumns()
        {
            return this.columns.Select(kv => kv.Value);
        }
    }
}