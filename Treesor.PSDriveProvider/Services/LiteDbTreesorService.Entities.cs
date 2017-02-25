using System;

namespace Treesor.PSDriveProvider
{
    public partial class LiteDbTreesorService
    {
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