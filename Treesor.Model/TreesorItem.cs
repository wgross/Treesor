using System;

namespace Treesor.Model
{
    public class TreesorItem
    {

        public TreesorItem(TreesorItemPath path, Reference<Guid> id)
        {
            this.Path = path;
            this.IdRef = id;
        }

        public Reference<Guid> IdRef { get; private set; }

        public Guid Id
        {
            set => this.IdRef = new Reference<Guid>(value);
            get => this.IdRef.Value;
        }

        /// <summary>
        /// a treesor item can always contain other items
        /// </summary>
        public bool IsContainer => true;

        public TreesorItemPath Path { get; }

        /// <summary>
        /// Equality of an entity depends on the Id
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>true, if objects are the same or have same id</returns>
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