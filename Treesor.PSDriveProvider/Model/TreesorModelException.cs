using System;
using System.Runtime.Serialization;

namespace Treesor.Model
{
    public class TreesorModelException : Exception
    {
        public TreesorModelErrorCodes ErrorCode { get; }

        private TreesorModelException(TreesorModelErrorCodes errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        private TreesorModelException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static TreesorModelException DuplicateItem(TreesorNodePath path) => new TreesorModelException(TreesorModelErrorCodes.DuplicateItem, $"item(path='{path.ToString()}') already exists");

        public static TreesorModelException NotImplemented(TreesorNodePath treesorNodePath, string message) => new TreesorModelException(TreesorModelErrorCodes.NotImplemented, message);
    }
}