using System;

namespace Treesor.PSDriveProvider
{
    public class TreesorItem
    {
        public TreesorItem(TreesorNodePath path, Guid id)
        {
            this.Path = path;
            this.Id = id;
        }

        public Guid Id { get; }

        public bool IsContainer => true;

        public TreesorNodePath Path { get; }

        public override bool Equals(object obj)
        {
            var objAsTreesorItem = obj as TreesorItem;
            if (objAsTreesorItem == null)
                return false;

            return this.Id.Equals(objAsTreesorItem.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}