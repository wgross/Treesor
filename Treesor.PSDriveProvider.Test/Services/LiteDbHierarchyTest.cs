using Elementary.Hierarchy;
using LiteDB;
using System;
using System.IO;
using Treesor.PSDriveProvider.Services;
using Xunit;

namespace Treesor.PSDriveProvider.Test.Services
{
    public class LiteDbHierarchyTest
    {
        private readonly LiteDatabase database;
        private readonly MemoryStream databaseStream;
        private readonly LiteDbHierarchy<string, Guid> hierarchy;

        public LiteDbHierarchyTest()
        {
            this.databaseStream = new MemoryStream();
            this.database = new LiteDatabase(this.databaseStream);
            this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).EnsureIndex(c => c.Name);
            this.hierarchy = new LiteDbHierarchy<string, Guid>(this.database);
        }

        [Fact]
        public void IHierarchy_root_node_has_no_value()
        {
            // ACT & ASSERT

            Guid value;
            Assert.False(this.hierarchy.TryGetValue(HierarchyPath.Create<string>(), out value));
            Assert.False(this.hierarchy.Traverse(HierarchyPath.Create<string>()).HasValue);
        }

        [Fact]
        public void IHierarchy_Add_value_to_root_node()
        {
            // ARRANGE

            Guid value = Guid.NewGuid();

            // ACT

            hierarchy.Add(HierarchyPath.Create<string>(), value);

            // ASSERT

            Guid result;

            // new hierarchy contains all values
            Assert.True(hierarchy.TryGetValue(HierarchyPath.Create<string>(), out result));
            Assert.Equal(result, value);
        }
    }
}