using Elementary.Hierarchy;
using LiteDB;
using Moq;
using System;
using System.IO;
using System.Linq;
using Treesor.Abstractions;
using Treesor.Model;
using Xunit;
using static Treesor.Model.TreesorItemPath;

namespace Treesor.Persistence.LiteDb.Test
{
    public class LiteDbTreesorServiceUsingHierarchyTest
    {
        private readonly LiteDatabase database;
        private readonly LiteCollection<LiteDbTreesorItemRepository.Node> nodeCollection;

        public Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock { get; }
        public LiteDbTreesorModel treesorModel { get; }

        private readonly MemoryStream databaseStream;

        public LiteDbTreesorServiceUsingHierarchyTest()
        {
            this.databaseStream = new MemoryStream();
            this.database = new LiteDatabase(this.databaseStream);
            this.nodeCollection = this.database.GetCollection<LiteDbTreesorItemRepository.Node>(LiteDbTreesorItemRepository.node_collection);

            //this.database.GetCollection<LiteDbTreesoModel.ColumnEntity>(LiteDbTreesorService.node_collection).EnsureIndex(c => c.Name);

            this.hierarchyMock = new Mock<IHierarchy<string, Reference<Guid>>>();
            this.treesorModel = new LiteDbTreesorModel(this.hierarchyMock.Object, this.database);
        }

        #region Get(root)

        [Fact]
        public void LiteDbService_creates_root_node()
        {
            // ACT
            // retrieve root node

            var result = this.treesorModel.Items.Get(RootPath);

            // ASSERT
            // root node exists and is the only persistent node

            Assert.NotNull(result);
            Assert.Equal(this.nodeCollection.FindAll().Single().Id, result.Id);
            Assert.Equal(RootPath, this.nodeCollection.FindAll().Single().Path);
        }

        [Fact]
        public void LiteDbService_reads_existing_root_node()
        {
            // ARRANGE
            // create root node

            var created = this.treesorModel.Items.Get(RootPath);

            // ACT
            // retrieve root node

            var result = this.treesorModel.Items.Get(RootPath);

            // ASSERT
            // root node exists and is the only persistent node

            Assert.Equal(created.Id, this.nodeCollection.FindAll().Single().Id);
            Assert.Equal(RootPath, this.nodeCollection.FindAll().Single().Path);
        }

        #endregion Get(root)

        #region NewItem > Get,Upsert

        [Fact]
        public void LiteDbService_creates_node_under_root()
        {
            // ACT

            var result = this.treesorModel.NewItem(TreesorItemPath.CreatePath("item"), newItemValue: null);

            // ASSERT

            // ASSERT
            // root node exists and is the only persistent node

            Assert.Equal(2, this.nodeCollection.Count());
            Assert.Equal(result.Path, this.nodeCollection.FindById(result.Id).Path);
            
        }

        [Fact]
        public void LiteDbService_NewItem_doesnt_accept_value()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorModel.NewItem(TreesorItemPath.CreatePath("item"), new object()));

            // ASSERT

            Assert.True(result.Message.Contains($"A value for node {CreatePath("item")} is not allowed"));
        }

        [Fact]
        public void NewItem_fails_if_item_exists_already()
        {
            // ARRANGE

            this.hierarchyMock
                .Setup(h => h.Add(HierarchyPath.Create("item"), It.IsAny<Reference<Guid>>()))
                .Throws(new ArgumentException());

            // ACT

            var result = Assert.Throws<ArgumentException>(() => this.treesorModel.NewItem(TreesorItemPath.CreatePath("item"), newItemValue: null));

            // ASSERT

            Assert.NotNull(result);
        }

        #endregion NewItem > Get,Upsert
    }
}