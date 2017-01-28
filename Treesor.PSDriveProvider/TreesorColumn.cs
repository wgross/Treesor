using Elementary.Properties.Sparse;
using System;

namespace Treesor.PSDriveProvider
{
    public class TreesorColumn
    {
        public TreesorColumn(string name, Type type)
            : this(name, type, SparsePropertyAccessorFactory<object>.Create())
        { }

        internal TreesorColumn(string name, Type type, SparsePropertyAccessor<object> propertyAccessor)
        {
            this.Name = name;
            this.Type = type;
            this.propertyAccessor = propertyAccessor;
        }

        private readonly SparsePropertyAccessor<object> propertyAccessor;

        public string Name { get; }

        public Type Type { get; }

        public void SetValue(Reference<Guid> id, object value)
        {
            if (!this.Type.Equals(value.GetType()))
                throw new InvalidOperationException($"Couldn't assign value '{value}'(type='{value.GetType()}') to property '{this.Name}' at node '{id.Value}': value.GetType() must be '{this.Type}'");

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