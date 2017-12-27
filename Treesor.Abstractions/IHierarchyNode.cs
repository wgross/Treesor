using Elementary.Hierarchy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Treesor.Abstractions
{
    public interface IHierarchyNode<TKey,TValue> : IHasChildNodes<IHierarchyNode<TKey,TValue>>
    {
        HierarchyPath<TKey> Path { get;  }

        TValue Value { get; }
        bool HasValue { get; }
    }
}
