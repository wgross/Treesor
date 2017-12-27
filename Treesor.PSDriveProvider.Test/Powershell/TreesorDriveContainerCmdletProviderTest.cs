using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;
using static Treesor.PSDriveProvider.Test.TestDataGenerators;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDriveContainerCmdletProviderTest : IDisposable
    {
        private PowerShell powershell;
        private Mock<ITreesorModel> treesorService;

        public TreesorDriveContainerCmdletProviderTest()
        {
            this.treesorService = new Mock<ITreesorModel>();

            TreesorDriveInfo.TreesorModelFactory = _ => treesorService.Object;

            this.powershell = ShellWithDriveCreated();
        }

        public void Dispose()
        {
            this.powershell.Stop();
            this.powershell.Dispose();
        }

        #region Set-Location > GetItem

        [Fact]
        public void SetLocation_checks_if_item_is_container()
        {
            // ARRANGE
            // mock a root item

            TreesorItem rootItem = new TreesorItem(path: TreesorNodePath.RootPath, id: new Reference<Guid>(Guid.NewGuid()));

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.RootPath))
                .Returns(rootItem);

            // ACT
            // set location to new drives root

            this.powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // set location succeeded

            Assert.False(this.powershell.HadErrors);

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.RootPath), Times.Once());
        }

        #endregion Set-Location > GetItem

        #region Remove-Item > RemoveItem

        [Fact]
        public void Powershell_removes_existing_leaf_under_root()
        {
            // ARRANGE
            // mock item without children

            this.treesorService
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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), false), Times.Once());
            this.treesorService.Verify(s => s.HasChildItems(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());

            Assert.False(this.powershell.HadErrors);
        }

        [Fact]
        public void Powershell_removes_children_under_existing_item_under_root()
        {
            // ARRANGE
            // mock item with children

            this.treesorService
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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), true), Times.Once());
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.HasChildItems(TreesorNodePath.Create("item")), Times.Once());

            Assert.False(this.powershell.HadErrors);
        }

        #endregion Remove-Item > RemoveItem

        #region Get-ChildItem > GetChildItem

        [Fact]
        public void Powershell_retrieves_child_items_of_item_under_root()
        {
            // ARRANGE
            // mock item under root with one child

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(TreesorItem("item"));

            TreesorItem child = null;
            this.treesorService
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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.GetChildItems(TreesorNodePath.Create("item")), Times.Once());

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

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(TreesorItem("item"));

            TreesorItem grandChild = null;
            this.treesorService
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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Exactly(2)); // because of isContainer
            this.treesorService.Verify(s => s.GetDescendants(TreesorNodePath.Create("item")), Times.Once());

            Assert.Equal(2, result.Count());
            Assert.Same(grandChild, result.ElementAt(1).BaseObject);
        }

        #endregion Get-ChildItem > GetChildItem

        #region Copy-Item > CopyItem

        [Fact]
        public void Powershell_copies_item_to_new_name_under_root()
        {
            // ARRANGE
            // item2 doesn't exist yet

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Returns((TreesorItem)null);

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

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorService.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false), Times.Once());
            this.treesorService.VerifyAll();

            Assert.False(this.powershell.HadErrors);
        }

        [Fact]
        public void Powershell_copies_item_to_new_name_under_root_recursively()
        {
            // ARRANGE
            // destination item deosn exist

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Returns((TreesorItem)null);

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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorService.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), true), Times.Once());

            Assert.False(this.powershell.HadErrors);
        }

        #endregion Copy-Item > CopyItem

        #region Rename-Item > RenameItem

        [Fact]
        public void Powershell_renames_item_under_root()
        {
            // ARRANGE
            // item with new names desn exist yet

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorService
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

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.RenameItem(TreesorNodePath.Create("item"), "item2"), Times.Once());

            Assert.False(this.powershell.HadErrors);
        }

        #endregion Rename-Item > RenameItem

        #region Resolve-Path > ExpandPath, GetChildItems

        [Fact]
        public void Powershell_resolves_wildcard_under_root_node()
        {
            // ARRANGE

            Reference<Guid> id_a, id_b;
            this.treesorService
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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.GetDescendants(TreesorNodePath.RootPath), Times.Once());

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

            this.treesorService
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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.GetDescendants(TreesorNodePath.RootPath), Times.Once());

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