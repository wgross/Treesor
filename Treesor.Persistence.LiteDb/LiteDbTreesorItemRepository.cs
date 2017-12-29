using Elementary.Hierarchy.Generic;
using LiteDB;
using System;
using System.Collections.Generic;
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

        public class Node : TreesorItem
        {
            public Node()
                : this(RootPath, new Reference<Guid>(Guid.NewGuid()))
            { }

            public Node(TreesorItemPath path, Reference<Guid> idRef)
                : base(path, idRef)
            {
                this.Key = path.HierarchyPath.Leaf().ToString();
            }

            public string Key { get; set; }
        }

        #region Construction and initialeization of this instance

        private readonly LiteDatabase database;
        private readonly LiteCollection<Node> nodes;
        private readonly Lazy<Node> rootNode;

        static LiteDbTreesorItemRepository()
        {
            BsonMapper.Global.Entity<Node>()
                .Id(n => n.Id)
                .Ignore(n => n.IdRef).Ignore(n => n.IsContainer).Ignore(n => n.Path);
        }

        public LiteDbTreesorItemRepository(LiteDatabase database)
        {
            this.database = database;
            this.nodes = this.database.GetCollection<Node>(node_collection);
            this.nodes.EnsureIndex(ce => ce.Key, unique: false);
            this.rootNode = new Lazy<Node>(() => this.GetOrCreateRootNode());
        }

        #endregion Construction and initialeization of this instance

        public bool Exists(TreesorItemPath treesorNodePath)
        {
            throw new System.NotImplementedException();
        }

        public TreesorItem Get(TreesorItemPath path)
        {
            if (RootPath.Equals(path))
                return this.rootNode.Value;

            throw new NotImplementedException();
        }

        private Node GetOrCreateRootNode()
        {
            var root = new Node(RootPath, new Reference<Guid>(Guid.NewGuid()));
            this.nodes.Upsert(root);
            return root;
        }

        public IEnumerable<TreesorItem> GetChildItems(TreesorItemPath treesorNodePath)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TreesorItem> GetDescendants(TreesorItemPath treesorNodePath)
        {
            throw new System.NotImplementedException();
        }

        internal Node New(TreesorItemPath treesorNodePath)
        {
            return null;
            //Elementary.Hierarchy.Generic.HasIdentifiableChildNodeExtensions.TryGetDescendantAt(this.rootNode.Value,TryGetChildNode);
            //return result;
        }

        private (bool,Node) TryGetChildNode(Node parent, string childKey)
        {
            throw new NotImplementedException();
        }
    }
}