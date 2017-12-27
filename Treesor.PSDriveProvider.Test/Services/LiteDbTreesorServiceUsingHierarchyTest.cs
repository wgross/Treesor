using Elementary.Hierarchy;
using LiteDB;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using Treesor.Abstractions;
using Treesor.PSDriveProvider.Test.Service.Base;

namespace Treesor.PSDriveProvider.Test.Services
{
    public class LiteDbTreesorServiceUsingHierarchyTest : TreesorServiceUsingHierarchyTestBase
    {
        private LiteDatabase database;
        private MemoryStream databaseStream;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.databaseStream = new MemoryStream();
            this.database = new LiteDatabase(this.databaseStream);
            this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.node_collection).EnsureIndex(c => c.Name);

            this.hierarchyMock = new Mock<IHierarchy<string, Reference<Guid>>>();
            this.treesorService = new LiteDbTreesorService(this.hierarchyMock.Object, this.database);
        }

        #region NewItem > Add

        [Test]
        public void LiteDbService_creates_hierarchy_node_under_root()
        {
            // ACT

            this.treesorService.NewItem(TreesorNodePath.Create("item"), newItemValue: null);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item"), It.IsAny<Reference<Guid>>()), Times.Once());
        }

        [Test]
        public void LiteDbService_NewItem_doesnt_accept_value()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorService.NewItem(TreesorNodePath.Create("item"), new object()));

            // ASSERT

            Assert.IsTrue(result.Message.Contains($"A value for node {TreesorNodePath.Create("item")} is not allowed"));
        }

        [Test]
        public void NewItem_fails_if_item_exists_already()
        {
            // ARRANGE

            this.hierarchyMock
                .Setup(h => h.Add(HierarchyPath.Create("item"), It.IsAny<Reference<Guid>>()))
                .Throws(new ArgumentException());

            // ACT

            var result = Assert.Throws<ArgumentException>(() => this.treesorService.NewItem(TreesorNodePath.Create("item"), newItemValue: null));

            // ASSERT

            Assert.NotNull(result);
        }

        #endregion NewItem > Add
    }
}