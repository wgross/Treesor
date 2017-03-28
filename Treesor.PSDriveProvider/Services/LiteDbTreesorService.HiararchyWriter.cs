namespace Treesor.PSDriveProvider.Services
{
    public class TreesorNodeVisitor
    {
        public virtual TreesorItem Visit(TreesorItem node)
        {
            return node;
        }
    }
}