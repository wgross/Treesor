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

        public static TreesorModelException DuplicateItem(TreesorItemPath path) => new TreesorModelException(TreesorModelErrorCodes.DuplicateItem, $"item(path='{path}') already exists");

        public static TreesorModelException NotImplemented(TreesorItemPath treesorNodePath, string message) => new TreesorModelException(TreesorModelErrorCodes.NotImplemented, message);

        public static TreesorModelException MissingItem(string path) => new TreesorModelException(TreesorModelErrorCodes.MissingItem, $"item(path='{path}') doesn't exist");
    }
}