using Elementary.Properties.Sparse;
using System;

namespace Treesor.PSDriveProvider
{
    public class TreesorColumn
    {
        public TreesorColumn(string name, string typeName)
            : this(name, typeName, SparsePropertyAccessorFactory<object>.Create())
        { }

        internal TreesorColumn(string name, string typeName, SparsePropertyAccessor<object> propertyAccessor)
        {
            this.Name = name;
            this.TypeName = typeName;
            this.propertyAccessor = propertyAccessor;
        }

        private readonly SparsePropertyAccessor<object> propertyAccessor;

        public string Name { get; }
        public string TypeName { get; }

        public void SetValue(Reference<Guid> id, object value)
        {
            if (!StringComparer.Ordinal.Equals(this.TypeName, value.GetType().Name))
                throw new InvalidOperationException($"Couldn't assign value '{value}' to property '{this.Name}' at node '{id.Value}': value.GetType().Name must be '{this.TypeName}'");

            this.propertyAccessor.SetValue(id, value);
        }

        public object GetValue(Reference<Guid> id)
        {
            return this.propertyAccessor.GetValue(id);
        }

        public void UnsetValue(TreesorItem nodeItem)
        {
            this.propertyAccessor.UnsetValue(nodeItem.IdRef);
        }
    }
}