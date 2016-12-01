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
    }
}