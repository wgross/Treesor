﻿using Elementary.Hierarchy;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using Treesor.Abstractions;
using Treesor.Model;

namespace Treesor.Persistence.LiteDb
{
    public partial class LiteDbTreesorModel : ITreesorModel
    {
        private readonly LiteDbTreesorItemRepository items;

        public ITreesorItemRepository Items => items;

        private readonly IDictionary<string, TreesorColumn> columns;

        #region Construction and initialization of this instance

        // isn't readonly because is 'nulled' in Dispse
        private IHierarchy<string, Reference<Guid>> hierarchy;

        private LiteDatabase database;

        public LiteDbTreesorModel(IHierarchy<string, Reference<Guid>> hierarchy, LiteDatabase database)
        {
            this.hierarchy = hierarchy;
            this.database = database;
            this.items = new LiteDbTreesorItemRepository(this.database);

            this.database.GetCollection<ColumnEntity>(column_collection).EnsureIndex(ce => ce.Name, unique: true);
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
                    this.database.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.hierarchy = null;

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

        #region Retrieve node by path

        private bool TryGetItem(TreesorItemPath path, out TreesorItem item)
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

        #endregion Retrieve node by path

        public void SetItem(TreesorItemPath treesorNodePath, object value)
        {
            throw new NotSupportedException($"A value for node {treesorNodePath} is not allowed");
        }

        public void ClearItem(TreesorItemPath rootPath)
        {
            // currently function isn't implmented.
            return;
        }

        public TreesorItem NewItem(TreesorItemPath treesorItemPath, object treesorItemValue)
        {
            // currently a value is not accepted.
            // a data model of the nodes is not yet decided

            if (treesorItemValue != null)
                throw new NotSupportedException($"Creating treesorItem(path='{treesorItemPath}' failed: a value isn't allowed.");

            return this.items.New(treesorItemPath);
        }

        public void RemoveItem(TreesorItemPath treesorNodePath, bool recurse)
        {
            this.hierarchy.RemoveNode(treesorNodePath.HierarchyPath, recurse);
        }

        public bool HasChildItems(TreesorItemPath treesorNodePath)
        {
            // Result of Travesr is never null.
            // hierachy throws KeyNotFindException in this case.
            return this.hierarchy.Traverse(treesorNodePath.HierarchyPath).HasChildNodes;
        }

        public IEnumerable<TreesorItem> GetChildItemsByWildcard(TreesorItemPath treesorNodePath)
        {
            throw new InvalidOperationException();
        }

        public IEnumerable<TreesorItem> GetChildItems(TreesorItemPath treesorNodePath)
        {
            return this.hierarchy
                .Traverse(treesorNodePath.HierarchyPath)
                .Children()
                .Select(n => new TreesorItem(TreesorItemPath.CreatePath(n.Path), n.Value));
        }

        public IEnumerable<TreesorItem> GetDescendants(TreesorItemPath treesorNodePath)
        {
            return this.hierarchy
                .Traverse(treesorNodePath.HierarchyPath)
                .Descendants()
                .Select(n => new TreesorItem(TreesorItemPath.CreatePath(n.Path), n.Value));
        }

        public void CopyItem(TreesorItemPath path, TreesorItemPath destinationPath, bool recurse)
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

        public void RenameItem(TreesorItemPath treesorNodePath, string newName)
        {
            this.items.Rename(treesorNodePath, newName);
        }

        public void MoveItem(TreesorItemPath path, TreesorItemPath destinationPath)
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

        #region Manage Item Property Values

        private TreesorColumn GetColumnOrThrow(string name)
        {
            var columnEntity = this.GetColumnEntityOrThrow(name);

            return new TreesorColumn(name, Type.GetType(columnEntity.TypeName, throwOnError: true, ignoreCase: true));
        }

        private ColumnEntity GetColumnEntityOrThrow(string name)
        {
            var columnEntity = this.database
                .GetCollection<ColumnEntity>(column_collection)
                .FindOne(c => c.Name.Equals(name));

            if (columnEntity == null)
                throw new InvalidOperationException($"Property '{name}' doesn't exist");

            return columnEntity;
        }

        public void SetPropertyValue(TreesorItemPath path, string name, object value)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            TreesorItem item;
            if (!this.TryGetItem(path, out item))
                throw new InvalidOperationException($"Node '{path}' doesn't exist");

            var column = this.GetColumnEntityOrThrow(name);
            if (!column.GetColumnType().Equals(value.GetType()))
                throw new InvalidOperationException($"Couldn't assign value '{value}'(type='{value.GetType()}') to property '{column.Name}' at node '{item.Id}': value.GetType() must be '{column.TypeName}'");

            this.UpsertColumnValue(item.Id, column.Id, value);
        }

        private void UpsertColumnValue(Guid nodeId, int columnId, object value)
        {
            var collection = this.database.GetCollection(value_collection);
            var itemValuesDocument = collection.FindById(new BsonValue(nodeId));
            if (itemValuesDocument == null)
            {
                itemValuesDocument = new BsonDocument(new Dictionary<string, BsonValue>
                {
                    { "_id", new BsonValue(nodeId) },
                });
            }
            //?//collection.Upsert(itemValuesDocument.Set(columnId.ToString(), new BsonValue(value)));
        }

        public object GetPropertyValue(TreesorItemPath path, string name)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var column = this.GetColumnEntityOrThrow(name);

            TreesorItem item;
            if (!this.TryGetItem(path, out item))
                throw new InvalidOperationException($"Node '{path}' doesn't exist");

            return this.GetColumnValue(item.Id, column.Id);
        }

        private object GetColumnValue(Guid itemId, int columnId)
        {
            return this.database
                .GetCollection(value_collection)
                .FindById(new BsonValue(itemId))
                ?.Get(columnId.ToString())
                ?.SingleOrDefault()
                ?.RawValue;
        }

        public void ClearPropertyValue(TreesorItemPath path, string name)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var column = this.GetColumnEntityOrThrow(name);

            TreesorItem item;
            if (!this.TryGetItem(path, out item))
                throw new InvalidOperationException($"Node '{path}' doesn't exist");

            this.ClearPropertyValue(item.Id, column.Id);
        }

        private void ClearPropertyValue(Guid itemId, int columnId)
        {
            var collection = this.database.GetCollection(value_collection);
            var bsonDocument = collection.FindById(new BsonValue(itemId));
            bsonDocument.Remove(columnId.ToString());
            collection.Update(bsonDocument);
        }

        public void CopyPropertyValue(TreesorItemPath sourcePath, string sourceProperty, TreesorItemPath destinationPath, string destinationProperty)
        {
            this.SetPropertyValue(destinationPath, destinationProperty, this.GetPropertyValue(sourcePath, sourceProperty));
        }

        public void MovePropertyValue(TreesorItemPath sourcePath, string sourceProperty, TreesorItemPath destinationPath, string destinationProperty)
        {
            this.SetPropertyValue(destinationPath, destinationProperty, this.GetPropertyValue(sourcePath, sourceProperty));
            this.ClearPropertyValue(sourcePath, sourceProperty);
        }

        #endregion Manage Item Property Values

        #region Manage Item Columns

        public TreesorColumn CreateColumn(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var existingColumn = this.database
                .GetCollection<ColumnEntity>(column_collection)
                .FindOne(ce => ce.Name.Equals(name));

            // accept same name with same type

            if (existingColumn != null && existingColumn.TypeName.Equals(type.ToString()))
                return new TreesorColumn(existingColumn.Name, existingColumn.GetColumnType());

            // throw on duplcate

            if (existingColumn != null)
                throw new InvalidOperationException($"Column: '{name}' already defined with type: '{existingColumn.TypeName}'");

            // write to Db and the in memory.
            var column = new TreesorColumn(name, type);

            var documentId = this.database
                .GetCollection<ColumnEntity>(column_collection)
                .Insert(new ColumnEntity
                {
                    Name = column.Name,
                    TypeName = column.Type.ToString()
                });

            return column;
        }

        public void RenameColumn(string name, string newName)
        {
            var column = this.GetColumnEntityOrThrow(name);

            var existingColumnEntity = this.database
               .GetCollection<ColumnEntity>(column_collection)
               .FindOne(c => c.Name.Equals(newName));

            if (existingColumnEntity != null)
                throw new ArgumentException("An item with the same key has already been added.");

            column.Name = newName;

            this.database
                .GetCollection<ColumnEntity>(column_collection)
                .Update(column);
        }

        public bool RemoveColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException(nameof(columnName));

            var tmp = this.database
                .GetCollection<ColumnEntity>(column_collection)
                .Delete(c => c.Name == columnName);

            return tmp > 0;
        }

        public IEnumerable<TreesorColumn> GetColumns()
        {
            return this.database
                .GetCollection<ColumnEntity>(column_collection)
                .FindAll()
                .Select(ce => new TreesorColumn(ce.Name, ce.GetColumnType()));
        }

        #endregion Manage Item Columns
    }
}