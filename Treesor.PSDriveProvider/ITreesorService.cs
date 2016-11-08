using System;

namespace Treesor.PSDriveProvider
{
    /// <summary>
    /// Interface contract for data servcies
    /// </summary>
    public interface ITreesorService : IDisposable
    {
        bool ItemExists(TreesorNodePath treesorNodePath);

        TreesorItem GetItem(TreesorNodePath rootPath);

        void SetItem(TreesorNodePath rootPath, object value);

        void ClearItem(TreesorNodePath rootPath);
    }
}