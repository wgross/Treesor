using System;

namespace Treesor.PSDriveProvider
{
    public partial class LiteDbTreesorService
    {
        public static readonly string column_collection = nameof(column_collection);

        public static readonly string value_collection = nameof(value_collection);

        public static readonly string node_collection = nameof(value_collection);

        /// <summary>
        /// Represents a persistent copy of a <see cref="TreesorColumn"/> in a LiteDb database.
        /// </summary>
        public class ColumnEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string TypeName { get; set; }

            public Type GetColumnType() => Type.GetType(this.TypeName, throwOnError: true, ignoreCase: true);
        }
    }

    public class ColumValue
    {
        public Guid Id { get; internal set; }
    }
}