using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using Treesor.Model;
using Xunit;
using static Treesor.PSDriveProvider.Test.TestDataGenerators;

namespace Treesor.PSDriveProvider.Test
{
    [Collection("Uses_powershell")]
    public class TreesorDriveContainerCmdletProviderTest : IDisposable
    {
        private PowerShell powershell;
        private Mock<ITreesorModel> treesorModel;

        public TreesorDriveContainerCmdletProviderTest()
        {
            this.treesorModel = new Mock<ITreesorModel>();

            TreesorDriveInfo.TreesorModelFactory = _ => treesorModel.Object;

            this.powershell = ShellWithDriveCreated();
        }

        public void Dispose()
        {
            this.powershell.Stop();
            this.powershell.Dispose();
        }

        #region New-Item > NewItem, MakePath

        [Fact]
        public void Powershell_creates_new_item_under_root()
        {
            // ARRANGE
            // mock result of creation of new item

            Reference<Guid> id_item = new Reference<Guid>(Guid.NewGuid());
            TreesorItem newItem = null;
            this.treesorModel
                .Setup(s => s.NewItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Returns<TreesorNodePath, object>((p, v) => TreesorItem(p, setup: ti => newItem = ti));

            // ACT
            // create new item under root.

            this.powershell
                .AddStatement()
                    .AddCommand("New-Item")
                        .AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider writes new item on outut pipe

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorModel.Verify(s => s.NewItem(TreesorNodePath.Create("item"), null), Times.Once());

            Assert.False(this.powershell.HadErrors);
            Assert.IsType<TreesorItem>(result.Last().BaseObject);
            Assert.Equal(newItem.Id, ((TreesorItem)result.Last().BaseObject).Id);
            Assert.Equal(@"TreesorDriveProvider\Treesor::item", result.Last().Properties["PSPath"].Value);
        }

        [Fact]
        public void Powershell_creates_new_item_under_node()
        {
            // ARRANGE
            // mock result of creation

            TreesorItem newItem = null;
            this.treesorModel
                .Setup(s => s.NewItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Returns<TreesorNodePath, object>((p, v) => TreesorItem(p, setup: ti => newItem = ti));

            // ACT
            // create a under item

            this.powershell
                .AddStatement()
                    .AddCommand("New-Item")
                        .AddParameter("Path", @"custTree:\item\a");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider writes new item on outut pipe

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item", "a")), Times.Never());
            this.treesorModel.Verify(s => s.NewItem(TreesorNodePath.Create("item", "a"), null), Times.Once());

            Assert.False(this.powershell.HadErrors);
            Assert.IsType<TreesorItem>(result.Last().BaseObject);
            Assert.Equal(newItem.Id, ((TreesorItem)result.Last().BaseObject).Id);
            Assert.Equal(@"TreesorDriveProvider\Treesor::item\a", result.Last().Properties["PSPath"].Value);
            Assert.Equal(@"TreesorDriveProvider\Treesor::item", result.Last().Properties["PSParentPath"].Value);
        }

        [Fact]
        public void Powershell_creates_new_item_under_root_twice_fails()
        {
            // ARRANGE
            // treesor service throws in existing item

            this.treesorModel
                .Setup(s => s.NewItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Throws(TreesorModelException.DuplicateItem(TreesorNodePath.Create("item")));

            // ACT
            // create a new item under root.

            this.powershell
                .AddStatement()
                    .AddCommand("New-Item")
                        .AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on output pipe

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorModel.Verify(s => s.NewItem(TreesorNodePath.Create("item"), null), Times.Once());

            Assert.True(this.powershell.HadErrors);
        }

        [Fact]
        public void Powershell_creates_new_item_with_value_fails()
        {
            // ARRANGE
            // model rejecst non-null value

            this.treesorModel
                .Setup(s => s.NewItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Throws(TreesorModelException.NotImplemented(TreesorNodePath.Create("item"), "item value"));

            // ACT

            this.powershell
                .AddStatement()
                    .AddCommand("New-Item")
                        .AddParameter("Path", @"custTree:\item")
                        .AddParameter("Value", "value");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on outut pipe

            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorModel.Verify(s => s.NewItem(TreesorNodePath.Create("item"), "value"), Times.Once());
            this.treesorModel.VerifyAll();

            Assert.True(this.powershell.HadErrors);
        }

        #endregion New-Item > NewItem, MakePath

        #region Remove-Item > RemoveItem,HasChildItems

        [Fact]
        public void Powershell_removes_existing_leaf_under_root()
        {
            // ARRANGE
            // mock item without children

            this.treesorModel
                .Setup(s => s.HasChildItems(TreesorNodePath.Create("item")))
                .Returns(false);

            // ACT
            // removes existing item

            this.powershell
                .AddStatement()
                    .AddCommand("Remove-Item")
                        .AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider wants item removed but not recursive

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), false), Times.Once());
            this.treesorModel.Verify(s => s.HasChildItems(TreesorNodePath.Create("item")), Times.Once());
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());

            Assert.False(this.powershell.HadErrors);
        }

        [Fact]
        public void Powershell_removes_children_under_existing_item_under_root()
        {
            // ARRANGE
            // mock item with children

            this.treesorModel
                .Setup(s => s.HasChildItems(TreesorNodePath.Create("item")))
                .Returns(true);

            // ACT
            // remove item with recurse flag

            this.powershell
                .AddStatement()
                    .AddCommand("Remove-Item")
                        .AddParameter("Path", @"custTree:\item")
                        .AddParameter("Recurse");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider wants item removed recursively

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), true), Times.Once());
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorModel.Verify(s => s.HasChildItems(TreesorNodePath.Create("item")), Times.Once());

            Assert.False(this.powershell.HadErrors);
        }

        #endregion Remove-Item > RemoveItem,HasChildItems

        #region Get-ChildItem > GetChildItem

        [Fact]
        public void Powershell_retrieves_child_items_of_item_under_root()
        {
            // ARRANGE
            // mock item under root with one child

            this.treesorModel
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(TreesorItem("item"));

            TreesorItem child = null;
            this.treesorModel
                .Setup(s => s.GetChildItems(TreesorNodePath.Create("item")))
                .Returns(new[] { TreesorItem("child", setup: ti => child = ti) });

            // ACT
            // get the item recursively

            this.powershell
                .AddStatement()
                    .AddCommand("Get-ChildItem")
                        .AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // the result contains the single child item of 'item'

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Once());
            this.treesorModel.Verify(s => s.GetChildItems(TreesorNodePath.Create("item")), Times.Once());

            var resultItem = result.LastOrDefault();

            Assert.NotNull(resultItem);
            Assert.IsType<TreesorItem>(resultItem.BaseObject);
            Assert.Same(child, resultItem.BaseObject);
        }

        [Fact]
        public void Powershell_retrieves_grandchildren_items_of_item_under_root()
        {
            // ARRANGE
            // children and grandchildren ar returned in a flat list

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(TreesorItem("item"));

            TreesorItem grandChild = null;
            this.treesorModel
                .Setup(s => s.GetDescendants(TreesorNodePath.Create("item")))
                .Returns(new[] { TreesorItem("child"), TreesorItem("grandchild", setup: ti => grandChild = ti) });

            // ACT
            // retrieve children of item recursively

            this.powershell
                .AddStatement()
                    .AddCommand("Get-ChildItem")
                        .AddParameter("Path", @"custTree:\item")
                        .AddParameter("Recurse");

            var result = this.powershell.Invoke();

            // ASSERT
            // the result contains child and grand child

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Exactly(2)); // because of isContainer
            this.treesorModel.Verify(s => s.GetDescendants(TreesorNodePath.Create("item")), Times.Once());

            Assert.Equal(2, result.Count());
            Assert.Same(grandChild, result.ElementAt(1).BaseObject);
        }

        #endregion Get-ChildItem > GetChildItem

        #region Copy-Item > CopyItem,GetItem

        [Fact]
        public void Powershell_copies_item_with_new_name()
        {
            // ARRANGE
            // item2 doesn't exist yet, item doesn exist as container

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Throws(TreesorModelException.MissingItem("item2"));

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(TreesorItem("item"));

            // ACT
            // copy item to item2

            this.powershell
                .AddStatement()
                    .AddCommand("Copy-Item")
                        .AddParameter("Path", @"custTree:\item")
                        .AddParameter("Destination", @"custTree:\item2");

            var result = this.powershell.Invoke();

            // ASSERT
            // item2 is checked before and the copied.

            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Once());
            this.treesorModel.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false), Times.Once());
            this.treesorModel.VerifyAll();

            Assert.False(this.powershell.HadErrors);
        }

        [Fact]
        public void Powershell_copies_item_over_existing_item()
        {
            // ARRANGE
            // item2 doesn't exist yet, item doesn exist as container

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Returns(TreesorItem("item2"));

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(TreesorItem("item"));

            // ACT
            // copy item to item2

            this.powershell
                .AddStatement()
                    .AddCommand("Copy-Item")
                        .AddParameter("Path", @"custTree:\item")
                        .AddParameter("Destination", @"custTree:\item2");

            var result = this.powershell.Invoke();

            // ASSERT
            // item2 is checked before and the copied.

            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Once());
            this.treesorModel.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false), Times.Once());
            this.treesorModel.VerifyAll();

            Assert.False(this.powershell.HadErrors);
        }

        [Fact]
        public void Powershell_copies_item_to_new_name_under_root_recursively()
        {
            // ARRANGE
            // destination item doesn't exist but source does

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Throws(TreesorModelException.MissingItem("item2"));

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(TreesorItem("item"));

            // ACT
            // copy item recursively

            this.powershell
                .AddStatement()
                    .AddCommand("Copy-Item")
                        .AddParameter("Path", @"custTree:\item")
                        .AddParameter("Destination", @"custTree:\item2")
                        .AddParameter("Recurse");

            var result = this.powershell.Invoke();

            // ASSERT
            // item2 was checked, copy got recurse flag

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Once());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorModel.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), true), Times.Once());

            Assert.False(this.powershell.HadErrors);
        }

        [Fact]
        public void Powershell_copy_item_continues_if_sources_doesnt_exist()
        {
            // ARRANGE
            // item2 doesn't exist yet, item doesn exist as container

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Throws(TreesorModelException.MissingItem("item2"));

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Throws(TreesorModelException.MissingItem("item"));

            // ACT
            // copy item to item2

            this.powershell
                .AddStatement()
                    .AddCommand("Copy-Item")
                        .AddParameter("Path", @"custTree:\item")
                        .AddParameter("Destination", @"custTree:\item2");

            var result = this.powershell.Invoke();

            // ASSERT
            // item and item2 is checked before and the copied if they are containers
            // powershell doesn't make a plausability check and continues with the copy operation

            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Once());
            this.treesorModel.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false), Times.Once());
            this.treesorModel.VerifyAll();

            Assert.False(this.powershell.HadErrors);
        }

        #endregion Copy-Item > CopyItem,GetItem

        #region Rename-Item > RenameItem

        [Fact]
        public void Powershell_renames_item_under_root()
        {
            // ARRANGE
            // item with new names desn exist yet

            this.treesorModel
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Returns((TreesorItem)null);

            // ACT
            // rename item

            this.powershell
                .AddStatement()
                    .AddCommand("Rename-Item")
                        .AddParameter("Path", @"custTree:\item")
                        .AddParameter("NewName", "item2");

            var result = this.powershell.Invoke();

            // ASSERT
            // rename was sent to model

            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorModel.Verify(s => s.RenameItem(TreesorNodePath.Create("item"), "item2"), Times.Once());

            Assert.False(this.powershell.HadErrors);
        }

        #endregion Rename-Item > RenameItem

        #region Resolve-Path > ExpandPath, GetChildItems

        [Fact]
        public void Powershell_resolves_wildcard_under_root_node()
        {
            // ARRANGE

            this.treesorModel
                .Setup(s => s.GetDescendants(TreesorNodePath.RootPath))
                .Returns(new[] { TreesorItem("a"), TreesorItem("b") });

            // ACT
            // request expansion of the wildcard by the provider

            this.powershell
                .AddStatement()
                    .AddCommand("Resolve-Path")
                        .AddParameter("Path", "custTree:*");

            var result = this.powershell.Invoke();

            // ASSERT
            // Drive info is resolving the wildcard correctly, path infos are returned

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.GetDescendants(TreesorNodePath.RootPath), Times.Once());

            Assert.False(this.powershell.HadErrors);
            Assert.Equal(2, result.Count);
            Assert.IsType<PathInfo>(result.ElementAt(0).ImmediateBaseObject);

            var pathInfo = (PathInfo)result.ElementAt(0).ImmediateBaseObject;

            Assert.Equal("custTree", pathInfo.Drive.Name);
            Assert.Equal(@"custTree:\a", pathInfo.Path);
            Assert.Equal("a", pathInfo.ProviderPath);

            Assert.IsType<PathInfo>(result.ElementAt(1).ImmediateBaseObject);

            pathInfo = (PathInfo)result.ElementAt(1).ImmediateBaseObject;

            Assert.Equal("custTree", pathInfo.Drive.Name);
            Assert.Equal(@"custTree:\b", pathInfo.Path);
            Assert.Equal("b", pathInfo.ProviderPath);
        }

        [Fact]
        public void Powershell_resolves_wildcard_under_node()
        {
            // ARRANGE
            // create a hierachy with positive and negative items at each level of selection

            this.treesorModel
                .Setup(s => s.GetDescendants(TreesorNodePath.RootPath))
                .Returns(new[] {
                    TreesorItem(TreesorNodePath.Create("item")),
                    TreesorItem(TreesorNodePath.Create("item2")),
                    TreesorItem(TreesorNodePath.Create("item","a1")),
                    TreesorItem(TreesorNodePath.Create("item","a2")),
                    TreesorItem(TreesorNodePath.Create("item","b")),
                });

            // ACT
            // request expansion of path

            this.powershell
                .AddStatement()
                    .AddCommand("Resolve-Path")
                        .AddParameter("Path", @"custTree:item\a*");

            var result = this.powershell.Invoke();

            // ASSERT
            // Drive info is resolving the wildcard correctly

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.GetDescendants(TreesorNodePath.RootPath), Times.Once());

            Assert.False(this.powershell.HadErrors);
            Assert.Equal(2, result.Count);
            Assert.IsType<PathInfo>(result.ElementAt(0).ImmediateBaseObject);

            var pathInfo = (PathInfo)result.ElementAt(0).ImmediateBaseObject;

            Assert.Equal(@"custTree:\item\a1", pathInfo.Path);
            Assert.IsType<PathInfo>(result.ElementAt(1).ImmediateBaseObject);

            pathInfo = (PathInfo)result.ElementAt(1).ImmediateBaseObject;

            Assert.Equal(@"custTree:\item\a2", pathInfo.Path);
        }

        #endregion Resolve-Path > ExpandPath, GetChildItems
    }
}