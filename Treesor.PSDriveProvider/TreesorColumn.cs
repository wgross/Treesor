using System;

namespace Treesor.PSDriveProvider
{
    public class TreesorColumn
    {
        public string Name { get; set; }

        public void SetValue(TreesorItem nodeItem, object v)
        {
            throw new NotImplementedException();
        }

        public object GetValue(TreesorItem nodeItem)
        {
            throw new NotImplementedException();
        }
    }
}