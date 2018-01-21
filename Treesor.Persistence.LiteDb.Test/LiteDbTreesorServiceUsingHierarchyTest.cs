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
    public class LiteDbTreesorServiceUsingHierarchyTest : IDisposable
    {
        private readonly LiteDatabase database;
        private readonly LiteCollection<BsonDocument> nodeCollection;
        private readonly Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock;

        public LiteDbTreesorModel treesorModel { get; }

        private readonly MockRepository mocks;
        private readonly MemoryStream databaseStream;

        public LiteDbTreesorServiceUsingHierarchyTest()
        {
            this.mocks = new MockRepository(MockBehavior.Strict);

            this.databaseStream = new MemoryStream();
            this.database = new LiteDatabase(this.databaseStream);
            this.nodeCollection = this.database.GetCollection<BsonDocument>(LiteDbTreesorItemRepository.node_collection);

            //this.database.GetCollection<LiteDbTreesoModel.ColumnEntity>(LiteDbTreesorService.node_collection).EnsureIndex(c => c.Name);

            this.hierarchyMock = this.mocks.Create<IHierarchy<string, Reference<Guid>>>();
            this.treesorModel = new LiteDbTreesorModel(this.hierarchyMock.Object, this.database);
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        #region Get(root)

        [Fact]
        public void Get_root_always_succeeds()
        {
            // ACT
            // retrieve root node

            var result = this.treesorModel.Items.Get(RootPath);

            // ASSERT
            // root node exists and is the only persistent node

            Assert.NotNull(result);
            Assert.Equal(RootPath, result.Path);
            Assert.Single(this.nodeCollection.FindAll());

            var single = this.nodeCollection.FindAll().Single();

            Assert.Null(single.Key());
            Assert.NotNull(single.Id());
            Assert.Equal(single, this.nodeCollection.FindById(single.Id()));
        }

        #endregion Get(root)

        #region NewItem > Get,Upsert

        [Fact]
        public void NewItem_creates_child_under_root()
        {
            // ACT

            var result = this.treesorModel.NewItem(TreesorItemPath.CreatePath("item"), treesorItemValue: null);

            // ASSERT
            // two nodes exist now, root has a single child

            Assert.NotNull(result);
            Assert.Equal(2, this.nodeCollection.Count());

            var children = this.treesorModel.Items.GetChildItems(RootPath);

            Assert.Single(children);
            Assert.Equal("item", children.Single().Path.ToString());

            var descandants = this.treesorModel.Items.GetDescendants(RootPath);

            Assert.Single(children);
            Assert.Equal("item", descandants.Single().Path.ToString());
        }

        [Fact]
        public void NewItem_creates_grandchild_under_root()
        {
            // ACT

            var result = this.treesorModel.NewItem(TreesorItemPath.CreatePath("child", "item"), treesorItemValue: null);

            // ASSERT
            // two nodes exist now, root has a single child

            Assert.NotNull(result);
            Assert.Equal(3, this.nodeCollection.Count());

            var children = this.treesorModel.Items.GetChildItems(RootPath);

            Assert.Single(children);
            Assert.Equal("child", children.Single().Path.ToString());

            var descandants = this.treesorModel.Items.GetDescendants(RootPath);

            Assert.Single(children);
            Assert.Equal("child", descandants.ElementAt(0).Path.ToString());
            Assert.Equal(@"child\item", descandants.ElementAt(1).Path.ToString());
        }

        [Fact]
        public void NewItem_fails_if_value_is_supplied()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorModel.NewItem(TreesorItemPath.CreatePath("item"), new object()));

            // ASSERT

            Assert.True(result.Message.Contains($"Creating treesorItem(path='{CreatePath("item")}' failed: a value isn't allowed."));
        }

        [Fact]
        public void NewItem_fails_if_item_exists_already()
        {
            // ARRANGE

            this.treesorModel.NewItem(TreesorItemPath.CreatePath("item"), treesorItemValue: null);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorModel.NewItem(TreesorItemPath.CreatePath("item"), treesorItemValue: null));

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal("Creating TreesorItem(path='item') failed: It already exists.", result.Message);
        }

        #endregion NewItem > Get,Upsert

        #region Exists

        [Fact]
        public void Exists_returns_true_on_existing_item()
        {
            // ACT

            this.treesorModel.NewItem(CreatePath("item"), treesorItemValue: null);

            // ACT

            var result = this.treesorModel.Items.Exists(CreatePath("item"));

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void Exists_returns_false_on_missing_item()
        {
            // ACT

            var result = this.treesorModel.Items.Exists(CreatePath("item"));

            // ASSERT

            Assert.False(result);
        }

        #endregion Exists

        #region Get(..)

        [Fact]
        public void Get_returns_existing_descendant()
        {
            // ARRANGE

            this.treesorModel.NewItem(CreatePath("item"), treesorItemValue: null);

            // ACT

            var result = this.treesorModel.Items.Get(CreatePath("item"));

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(CreatePath("item"), result.Path);
        }

        [Fact]
        public void Get_returns_null_on_missing_item()
        {
            // ACT

            var result = this.treesorModel.Items.Get(CreatePath("item"));

            // ASSERT

            Assert.Null(result);
        }

        #endregion Get(..)
    }
}