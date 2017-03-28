using Elementary.Hierarchy;
using LiteDB;
using NUnit.Framework;
using System;
using System.IO;
using Treesor.PSDriveProvider.Services;

namespace Treesor.PSDriveProvider.Test.Services
{
    [TestFixture]
    public class LiteDbHierarchyTest
    {
        private LiteDatabase database;
        private MemoryStream databaseStream;
        private LiteDbHierarchy<string, Guid> hierarchy;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.databaseStream = new MemoryStream();
            this.database = new LiteDatabase(this.databaseStream);
            this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).EnsureIndex(c => c.Name);
            this.hierarchy = new LiteDbHierarchy<string, Guid>(this.database);
        }

        [Test]
        public void IHierarchy_root_node_has_no_value()
        {
            // ACT & ASSERT

            Guid value;
            Assert.IsFalse(this.hierarchy.TryGetValue(HierarchyPath.Create<string>(), out value));
            Assert.IsFalse(this.hierarchy.Traverse(HierarchyPath.Create<string>()).HasValue);
        }

        [Test]
        public void IHierarchy_Add_value_to_root_node()
        {
            // ARRANGE

            Guid value = Guid.NewGuid();

            // ACT

            hierarchy.Add(HierarchyPath.Create<string>(), value);

            // ASSERT

            Guid result;

            // new hierarchy contains all values
            Assert.IsTrue(hierarchy.TryGetValue(HierarchyPath.Create<string>(), out result));
            Assert.AreSame(result, value);
        }
    }
}