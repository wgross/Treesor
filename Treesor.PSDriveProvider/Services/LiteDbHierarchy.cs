using Elementary.Hierarchy;
using Elementary.Hierarchy.Generic;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using Treesor.Abstractions;

namespace Treesor.PSDriveProvider.Services
{
    public class LiteDbHierarchy<TKey, TValue> : IHierarchy<TKey, TValue>
    {
        private class Node : IHierarchyNode<TKey, TValue>, IHasIdentifiableChildNodes<TKey, IHierarchyNode<TKey, TValue>>
        {
            private readonly LiteCollection<BsonDocument> nodes;
            private BsonDocument currentNode;

            private Node Wrap(BsonDocument document)
            {
                return new Node(this.nodes, document);
            }

            private IEnumerable<Node> Wrap(IEnumerable<BsonDocument> documents)
            {
                return documents.Select(b => Wrap(b));
            }

            public Node(LiteCollection<BsonDocument> nodes, BsonDocument currentNode)
            {
                this.currentNode = currentNode;
                this.nodes = nodes;
            }

            #region IHasChildNodes Members

            public IEnumerable<IHierarchyNode<TKey, TValue>> ChildNodes
                => Wrap(this.nodes.Find(Query.EQ("parent", this.currentNode.Get("_id"))));

            public bool HasChildNodes => this.nodes.Exists(Query.EQ("parent", this.currentNode.Get("_id")));

            #endregion IHasChildNodes Members

            public HierarchyPath<TKey> Path
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool HasValue => this.currentNode.ContainsKey("value");

            public TValue Value => (TValue)this.currentNode.Get("value").RawValue;

            #region IHasParentNode Members

            private BsonValue ParentId => this.currentNode.Get("parent");

            public bool HasParentNode => this.nodes.Exists(Query.EQ("_id", this.ParentId));

            public IHierarchyNode<TKey, TValue> ParentNode => Wrap(this.nodes.FindOne(Query.EQ("_id", this.ParentId)));

            #endregion IHasParentNode Members

            #region IHasIdentifiableChildNodes Members

            public BsonValue Key => this.currentNode.Get("key");

            public bool TryGetChildNode(TKey key, out IHierarchyNode<TKey, TValue> childNode)
            {
                childNode = null;
                var node = this.nodes
                    .Find(Query.And(
                        Query.EQ("parent", this.currentNode.Get("_id")),
                        Query.EQ("key", new BsonValue(key))))
                    .SingleOrDefault();

                if (node == null)
                    return false;

                childNode = Wrap(node);
                return true;
            }

            internal void AddChildNode(Node node)
            {
                throw new NotImplementedException();
            }

            #endregion IHasIdentifiableChildNodes Members
        }

        private LiteDatabase database;
        private readonly LiteCollection<BsonDocument> nodes;

        public LiteDbHierarchy(LiteDatabase database)
        {
            this.database = database;
            this.nodes = this.database.GetCollection("nodes_collection");
        }

        #region IHierarchy Members

        public TValue this[HierarchyPath<TKey> hierarchyPath]
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(HierarchyPath<TKey> hierarchyPath, TValue value)
        {
            var node = GetOrCreateNode(hierarchyPath);
        }

        public bool Remove(HierarchyPath<TKey> hierarchyPath, int? maxDepth = default(int?))
        {
            throw new NotImplementedException();
        }

        public bool RemoveNode(HierarchyPath<TKey> hierarchyPath, bool recurse)
        {
            throw new NotImplementedException();
        }

        public IHierarchyNode<TKey, TValue> Traverse(HierarchyPath<TKey> startAt)
        {
            return new Node(this.nodes, this.GetOrCreateRootNode());
        }

        public bool TryGetValue(HierarchyPath<TKey> hierarchyPath, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        #endregion IHierarchy Members

        #region LiteDb access implementations

        private BsonDocument GetOrCreateRootNode()
        {
            var root = this.nodes.FindOne(Query.EQ("key", null));
            if (root == null)
            {
                root = new BsonDocument();
                root.Set("key", null);
                this.nodes.Insert(root);
            }
            return root;
        }

        private Node GetOrCreateNode(HierarchyPath<TKey> hierarchyPath)
        {
            return null;
        //    var visitor = new Node(this.nodes, this.GetOrCreateRootNode());
        //    var currentPosition = HierarchyPath.Create<TKey>();

        //    return visitor.DescendantAt<string, IHierarchyNode<string, Reference<Guid>>>(delegate (IHierarchyNode<string, Reference<Guid>> current, TKey key, out IHierarchyNode<string, Reference<Guid>> child)
        //    {
        //        currentPosition = currentPosition.Join(key);

        //        // if the child isn't found, just create a new one on-the-fly
        //        if (!current.TryGetChildNode(key, out child))
        //        {
        //            if (currentPosition.Items.Count() < hierarchyPath.Items.Count() && this.getDefaultValue != null)
        //            {
        //                current.AddChildNode(child = new Node(key, this.getDefaultValue(currentPosition)));
        //            }
        //            else
        //            {
        //                current.AddChildNode(child = new Node(key));
        //            }
        //        }
        //        return true;
        //    }, hierarchyPath);
        }

        #endregion LiteDb access implementations
    }
}