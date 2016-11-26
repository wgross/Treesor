using Elementary.Hierarchy.Collections;
using NLog;
using System;
using System.Collections.Generic;

namespace Treesor.PSDriveProvider
{
    public class TreesorService : ITreesorService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static Func<string, ITreesorService> Factory { get; set; }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls
        private IHierarchy<string, Guid> hierarchy;

        public TreesorService(IHierarchy<string, Guid> hierarchy)
        {
            this.hierarchy = hierarchy;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TreesorService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        public bool ItemExists(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        public TreesorItem GetItem(TreesorNodePath rootPath)
        {
            throw new NotImplementedException();
        }

        public void SetItem(TreesorNodePath rootPath, object value)
        {
            throw new NotImplementedException();
        }

        public void ClearItem(TreesorNodePath rootPath)
        {
            throw new NotImplementedException();
        }

        public TreesorItem NewItem(TreesorNodePath treesorNodePath, object newItemValue)
        {
            // currently a value is not accepted.
            // a data model of the nodes is not yet decided

            if (newItemValue != null)
                throw new NotSupportedException($"A value for node {treesorNodePath} is not allowed");

            this.hierarchy.Add(treesorNodePath.HierarchyPath, Guid.NewGuid());

            return new TreesorItem(treesorNodePath);
        }

        public void RemoveItem(TreesorNodePath treesorNodePath, bool recurse)
        {
            throw new NotImplementedException();
        }

        public bool HasChildItems(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreesorItem> GetChildItems(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreesorItem> GetDescendants(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        public void CopyItem(TreesorNodePath path, TreesorNodePath destinationPath, bool recurse)
        {
            throw new NotImplementedException();
        }

        public void RenameItem(TreesorNodePath treesorNodePath, string newName)
        {
            throw new NotImplementedException();
        }

        public void MoveItem(TreesorNodePath path, TreesorNodePath destination)
        {
            throw new NotImplementedException();
        }
    }
}