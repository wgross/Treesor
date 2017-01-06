using Elementary.Properties.Sparse;

namespace Treesor.PSDriveProvider
{
    public class TreesorColumn
    {
        internal class Ref<T> where T : struct
        {
            public Ref(T referencedValue)
            {
                ReferencedValue = referencedValue;
            }

            internal readonly T ReferencedValue;
        }

        public TreesorColumn(string name)
        {
            this.Name = name;
            this.propertyAccessor = SparsePropertyAccessorFactory<object>.Create();
        }

        private readonly SparsePropertyAccessor<object> propertyAccessor;

        public string Name { get; }

        public void SetValue(TreesorItem nodeItem, object v)
        {
            this.propertyAccessor.SetValue(nodeItem.IdRef, v);
        }

        public object GetValue(TreesorItem nodeItem)
        {
            return this.propertyAccessor.GetValue(nodeItem);
        }
    }
}