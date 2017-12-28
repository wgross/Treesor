namespace Treesor.Model
{
    public interface ITreesorItemRepository
    {
        /// <summary>
        /// Validates if the specified item path exists in the model
        /// </summary>
        /// <param name="treesorNodePath"></param>
        /// <returns></returns>
        bool Exists(TreesorItemPath treesorNodePath);

        /// <summary>
        /// Retrieves the item identified by the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        TreesorItem Get(TreesorItemPath path);
    }
}