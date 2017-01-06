using Elementary.Properties.Sparse;

namespace Treesor.PSDriveProvider
{
    public class TreesorColumn
    {
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
            return this.propertyAccessor.GetValue(nodeItem.IdRef);
        }

        public void ClearValue(TreesorItem nodeItem)
        {
            this.propertyAccessor.SetValue(nodeItem.IdRef, null);
        }
    }
}