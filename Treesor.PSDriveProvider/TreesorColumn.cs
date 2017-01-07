using Elementary.Properties.Sparse;
using System;

namespace Treesor.PSDriveProvider
{
    public class TreesorColumn
    {
        public TreesorColumn(string name, string typeName)
        {
            this.Name = name;
            this.TypeName = typeName;
            this.propertyAccessor = SparsePropertyAccessorFactory<object>.Create();
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

        public void ClearValue(TreesorItem nodeItem)
        {
            this.propertyAccessor.UnsetValue(nodeItem.IdRef);
        }
    }
}