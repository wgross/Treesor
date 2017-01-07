using System;

namespace Treesor.PSDriveProvider
{
    public class TreesorItem
    {
        public TreesorItem(TreesorNodePath path, Reference<Guid> id)
        {
            this.Path = path;
            this.IdRef = id;
        }

        internal Reference<Guid> IdRef { get; }

        public Guid Id => this.IdRef.Value;

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