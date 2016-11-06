using System;
using NLog;

namespace Treesor.PSDriveProvider
{
    public class TreesorService : ITreesorService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static Func<string,ITreesorService> Factory { get; set; }

        public void Unloading()
        {
            throw new NotImplementedException();
        }
    }
}