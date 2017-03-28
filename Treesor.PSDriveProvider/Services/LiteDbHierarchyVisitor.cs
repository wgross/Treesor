using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treesor.PSDriveProvider.Services
{
    public class BsonDocumentAdapter<TKey, TValue> :
        IHierarchyNode<TKey, TValue>,
        IHasIdentifiableChildNodes<TKey, IHierarchyNode<TKey, TValue>>
    {
        private readonly LiteCollection<BsonDocument> nodes;
        private BsonDocument currentNode;

        private BsonDocumentAdapter<TKey, TValue> Wrap(BsonDocument document)
        {
            return new BsonDocumentAdapter<TKey, TValue>(this.nodes, document);
        }

        private IEnumerable<BsonDocumentAdapter<TKey, TValue>> Wrap(IEnumerable<BsonDocument> documents)
        {
            return documents.Select(b => Wrap(b));
        }

        public BsonDocumentAdapter(LiteCollection<BsonDocument> nodes, BsonDocument currentNode)
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

        #endregion IHasIdentifiableChildNodes Members
    }
}