using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treesor.PSDriveProvider
{
    public class TreesorItem
    {
        public TreesorItem(TreesorNodePath path)
        {
            this.Path = path;
        }
        public bool IsContainer => true;

        public TreesorNodePath Path { get; }
    }
}
