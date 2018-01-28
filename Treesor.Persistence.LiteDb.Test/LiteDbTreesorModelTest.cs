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
    public class LiteDbTreesorModelTest : IDisposable
    {
        private readonly LiteDatabase database;
        private readonly LiteCollection<BsonDocument> nodeCollection;
        private readonly Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock;

        public LiteDbTreesorModel treesorModel { get; }

        private readonly MockRepository mocks;
        private readonly MemoryStream databaseStream;

        public LiteDbTreesorModelTest()
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

        [Fact]
        public void NewItem_fails_on_null_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorModel.NewItem(null, treesorItemValue: null));

            // ASSERT

            Assert.Equal("treesorItemPath", result.ParamName);
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
        public void Exists_returns_true_on_existing_item_CI()
        {
            // ACT

            this.treesorModel.NewItem(CreatePath("item"), treesorItemValue: null);

            // ACT

            var result = this.treesorModel.Items.Exists(CreatePath("ITEM"));

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void Exists_returns_fails_on_missing_item()
        {
            // ACT

            var result = this.treesorModel.Items.Exists(CreatePath("item"));

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void Exists_fails_on_null_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorModel.Items.Exists(null));

            // ASSERT

            Assert.Equal("treesorItemPath", result.ParamName);
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
        public void Get_returns_existing_descendant_CI()
        {
            // ARRANGE

            this.treesorModel.NewItem(CreatePath("Item"), treesorItemValue: null);

            // ACT

            var result = this.treesorModel.Items.Get(CreatePath("ITEM"));

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(CreatePath("Item"), result.Path);
        }

        [Fact]
        public void Get_returns_null_on_missing_item()
        {
            // ACT

            var result = this.treesorModel.Items.Get(CreatePath("item"));

            // ASSERT

            Assert.Null(result);
        }

        [Fact]
        public void Get_fails_on_null_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorModel.Items.Get(null));

            // ASSERT

            Assert.Equal("treesorItemPath", result.ParamName);
        }

        #endregion Get(..)

        #region ClearItem: Does nothing

        [Fact]
        public void ClearItem_does_nothing()
        {
            // ACT

            this.treesorModel.ClearItem(TreesorItemPath.CreatePath("item"));
        }

        #endregion ClearItem: Does nothing

        #region RenameItem

        [Fact]
        public void Rename_changes_nodes_key_parents_childKey()
        {
            // ARRANGE

            var item = this.treesorModel.NewItem(CreatePath("item"), null);
            var child = this.treesorModel.NewItem(CreatePath("item", "child"), null);

            // ACT

            this.treesorModel.RenameItem(CreatePath("item", "child"), "child2");

            // ASSERT

            var item2 = this.treesorModel.Items.Get(CreatePath("item", "child2"));

            Assert.Equal(CreatePath("item", "child2"), item2.Path);
        }

        [Fact]
        public void Rename_fails_for_unknown_child()
        {
            // ARRANGE

            var item = this.treesorModel.NewItem(CreatePath("item"), null);
            var child = this.treesorModel.NewItem(CreatePath("item", "child"), null);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorModel.RenameItem(CreatePath("item", "child2"), "child1"));

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(@"Renaming TreesorItem(path='item\child2') failed: It doesn't exist.", result.Message);
        }

        [Fact]
        public void Rename_root_fails()
        {
            // ACT

            var result = Assert.Throws<ArgumentException>(() => this.treesorModel.RenameItem(RootPath, "child1"));

            // ASSERT

            Assert.NotNull(result);
            Assert.Contains("Root node can't be renamed", result.Message);
            Assert.Equal("treesorItemPath", result.ParamName);
        }

        #endregion RenameItem
    }
}