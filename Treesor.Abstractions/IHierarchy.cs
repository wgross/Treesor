using Elementary.Hierarchy;
using System.Collections.Generic;

namespace Treesor.Abstractions
{
    public interface IHierarchy<TKey, TValue>
    {
        IHierarchyNode<TKey, TValue> Traverse(HierarchyPath<TKey> hierarchyPath);

        bool TryGetValue(HierarchyPath<TKey> hierarchyPath, out TValue id);

        void Add(HierarchyPath<TKey> hierarchyPath, TValue reference);

        bool Remove(HierarchyPath<TKey> hierarchyPath, int? maxDepth = default(int?));

        bool RemoveNode(HierarchyPath<TKey> hierarchyPath, bool recurse);
    }
}