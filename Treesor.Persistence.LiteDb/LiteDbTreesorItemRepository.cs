using Elementary.Hierarchy.Generic;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using Treesor.Model;
using static Treesor.Model.TreesorItemPath;

namespace Treesor.Persistence.LiteDb
{
    public class LiteDbTreesorItemRepository : ITreesorItemRepository
    {
        public static readonly string node_collection = nameof(node_collection);

        /// <summary>
        /// Represents the Lite db node structure
        /// </summary>

        public class LiteDbTreesorItem : TreesorItem
        {
            public LiteDbTreesorItem(BsonDocument node)
                : this(RootPath)
            {
                this.BsonDocument = node;
            }

            internal BsonDocument BsonDocument { get; private set; }

            public LiteDbTreesorItem(TreesorItemPath path)
                : base(path, new Reference<Guid>(Guid.NewGuid()))

            {
                //this.Key = path.HierarchyPath.Leaf().ToString();
            }

            public LiteDbTreesorItem(TreesorItemPath path, BsonDocument bsonDocumentNode) : this(path)
            {
                this.BsonDocument = bsonDocumentNode;
            }

            public void Add(LiteDbTreesorItem child)
            {
                this.BsonDocument.TryAddChild(child.BsonDocument);
            }

            public string Key => this.BsonDocument.Key();

            public (bool, BsonValue) TryGetChild(string childKey)
            {
                throw new NotImplementedException();
            }

            public (string, BsonValue) Children => (null, null);
        }

        #region Construction and initialization of this instance

        private readonly LiteDatabase database;
        private readonly LiteCollection<BsonDocument> nodes;
        private readonly Lazy<LiteDbTreesorItem> lazyRootNode;

        //static LiteDbTreesorItemRepository()
        //{
        //    BsonMapper.Global.Entity<LiteDbTreesorItem>()
        //        .Id(n => n.Id)
        //        .Ignore(n => n.IdRef).Ignore(n => n.IsContainer).Ignore(n => n.Path);
        //}

        public LiteDbTreesorItemRepository(LiteDatabase database)
        {
            this.database = database;
            this.nodes = this.database.GetCollection<BsonDocument>(node_collection);
            this.lazyRootNode = new Lazy<LiteDbTreesorItem>(() => this.GetOrCreateRootNode());
        }

        #endregion Construction and initialization of this instance

        public bool Exists(TreesorItemPath treesorItemPath)
        {
            if (treesorItemPath == null)
                throw new ArgumentNullException(nameof(treesorItemPath));

            return this.lazyRootNode.Value.TryGetDescendantAt(this.TryGetChildNodeByKey, treesorItemPath.HierarchyPath).Item1;
        }

        public TreesorItem Get(TreesorItemPath treesorItemPath)
        {
            if (treesorItemPath == null)
                throw new ArgumentNullException(nameof(treesorItemPath));

            if (RootPath.Equals(treesorItemPath))
                return this.lazyRootNode.Value;

            var (exists, node) = this.lazyRootNode.Value.TryGetDescendantAt(this.TryGetChildNodeByKey, treesorItemPath.HierarchyPath);
            if (exists)
                return node;

            return null;
        }

        private LiteDbTreesorItem GetOrCreateRootNode()
        {
            var existingNode = this.nodes.FindOne(Query.EQ("key", BsonValue.Null));
            if (existingNode != null)
                return new LiteDbTreesorItem(RootPath, existingNode);

            var newRoot = new LiteDbTreesorItem(RootPath, new BsonDocument().Key(BsonValue.Null));
            if (this.nodes.Upsert(newRoot.BsonDocument))
                return newRoot;

            throw new InvalidOperationException("A root not was't found and couldn't be created");
        }

        public IEnumerable<TreesorItem> GetChildItems(TreesorItemPath treesorNodePath)
        {
            return this.lazyRootNode.Value.Children(this.GetChildNode);
        }

        public IEnumerable<TreesorItem> GetDescendants(TreesorItemPath treesorNodePath)
        {
            return this.lazyRootNode.Value.Descendants(this.GetChildNode);
        }

        internal LiteDbTreesorItem New(TreesorItemPath treesorItemPath)
        {
            if (treesorItemPath == null)
                throw new ArgumentNullException(nameof(treesorItemPath));

            // fail if node is already there
            if (Exists(treesorItemPath))
                throw new InvalidOperationException($"Creating TreesorItem(path='{treesorItemPath.HierarchyPath.ToString()}') failed: It already exists.");

            // create node and return it
            var (_, node) = this.lazyRootNode.Value.TryGetDescendantAt(GetOrCreateChildNodeByKey, treesorItemPath.HierarchyPath);
            return node;
        }

        private (bool Success, LiteDbTreesorItem Node) GetOrCreateChildNodeByKey(LiteDbTreesorItem parent, string childKey)
        {
            var child = parent.BsonDocument.TryGetChild(childKey);
            if (child.Exists)
                return (true, new LiteDbTreesorItem(this.nodes.FindById(child.Id)));

            var childNode = new BsonDocument().Key(childKey);
            if (this.nodes.Upsert(childNode))
                if (parent.BsonDocument.TryAddChild(childNode))
                    if (this.nodes.Update(parent.BsonDocument))
                        return (true, new LiteDbTreesorItem(childNode));

            return (false, null);
        }

        private (bool Success, LiteDbTreesorItem Node) TryGetChildNodeByKey(LiteDbTreesorItem parent, string childKey)
        {
            var child = parent.BsonDocument.TryGetChild(childKey);
            if (child.Exists)
            {
                var doc = this.nodes.FindById(child.Id);
                return (true, new LiteDbTreesorItem(CreatePath(parent.Path.HierarchyPath.Join(doc.Key())), doc));
            }
            return (false, null);
        }

        private IEnumerable<LiteDbTreesorItem> GetChildNode(LiteDbTreesorItem parent)
        {
            LiteDbTreesorItem fromId(ObjectId id)
            {
                var tmp = this.nodes.FindById(id);
                return new LiteDbTreesorItem(TreesorItemPath.CreatePath(parent.Path.HierarchyPath.Join(tmp.Key())), tmp);
            }

            return parent.BsonDocument.Children().Select(kv => fromId(kv.Value));
        }
    }
}