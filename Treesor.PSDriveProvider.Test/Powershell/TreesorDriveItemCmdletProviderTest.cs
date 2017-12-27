using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using Treesor.Model;
using Xunit;
using static Treesor.PSDriveProvider.Test.TestDataGenerators;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDriveItemCmdletProviderTest : IDisposable
    {
        private readonly PowerShell powershell;
        private readonly Mock<ITreesorModel> treesorService;

        public TreesorDriveItemCmdletProviderTest()
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

        #region New-Item > NewItem, MakePath

        [Fact]
        public void Powershell_creates_new_item_under_root()
        {
            // ARRANGE
            // mock result of creation of new item

            Reference<Guid> id_item = new Reference<Guid>(Guid.NewGuid());
            TreesorItem newItem = null;
            this.treesorService
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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.NewItem(TreesorNodePath.Create("item"), null), Times.Once());

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
            this.treesorService
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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item", "a")), Times.Never());
            this.treesorService.Verify(s => s.NewItem(TreesorNodePath.Create("item", "a"), null), Times.Once());

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

            this.treesorService
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

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.NewItem(TreesorNodePath.Create("item"), null), Times.Once());

            Assert.True(this.powershell.HadErrors);
        }

        [Fact]
        public void Powershell_creates_new_item_under_root_with_value_fails()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.NewItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Throws(TreesorModelException.NotImplemented(TreesorNodePath.Create("item"), "item value"));

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("New-Item").AddParameter("Path", @"custTree:\item").AddParameter("Value", "value");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on outut pipe

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.NewItem(TreesorNodePath.Create("item"), "value"), Times.Once());
            this.treesorService.VerifyAll();

            Assert.True(this.powershell.HadErrors);
        }

        #endregion New-Item > NewItem, MakePath

        #region Test-Path > ItemExists

        [Fact]
        public void Powershell_asks_service_for_existence_of_missing_root()
        {
            // ACT
            // test for a missing item

            this.powershell
                .AddStatement()
                .AddCommand("Test-Path").AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // item wasn't found and service was ask for the path

            Assert.False(this.powershell.HadErrors);
            Assert.False((bool)result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once());
        }

        [Fact]
        public void Powershell_asks_service_for_existence_of_existing_root()
        {
            // ARRANGE
            this.treesorService.Setup(s => s.ItemExists(TreesorNodePath.RootPath)).Returns(true);

            // ACT
            // test for a missing item

            this.powershell
                .AddStatement()
                .AddCommand("Test-Path").AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // item wasn't found and service was ask for the path

            Assert.False(this.powershell.HadErrors);
            Assert.True((bool)result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once);
        }

        [Fact]
        public void Powershell_asks_service_for_existence_of_missing_item()
        {
            // ACT
            // test for a missing item

            this.powershell
                .AddStatement()
                .AddCommand("Test-Path").AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // item wasn't found and service was ask for the path

            Assert.False(this.powershell.HadErrors);
            Assert.False((bool)result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once);
        }

        [Fact]
        public void Powershell_asks_service_for_existence_of_exiting_item()
        {
            // ARRANGE

            this.treesorService.Setup(s => s.ItemExists(TreesorNodePath.Create("item"))).Returns(true);

            // ACT
            // test for a missing item

            this.powershell
                .AddStatement()
                .AddCommand("Test-Path").AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // item wasn't found and service was ask for the path

            Assert.False(this.powershell.HadErrors);
            Assert.True((bool)result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once);
        }

        #endregion Test-Path > ItemExists

        #region Get-Item > GetItem, MakePath

        [Fact]
        public void Powershell_retrieves_missing_root_item()
        {
            // ACT
            // getting a missing item fails

            this.powershell
                .AddStatement()
                .AddCommand("Get-Item").AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // reading an item that doesn't exist is an error

            Assert.True(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Never());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.RootPath), Times.Once());
        }

        [Fact]
        public void Powershell_retrieves_existing_root_item()
        {
            // ARRANGE

            Reference<Guid> id_root;
            this.treesorService.Setup(s => s.GetItem(TreesorNodePath.RootPath)).Returns(new TreesorItem(TreesorNodePath.RootPath, id_root = new Reference<Guid>(Guid.NewGuid())));

            // ACT
            // test for a missing item

            this.powershell
                .AddStatement()
                .AddCommand("Get-Item").AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // reading an item that doesn't exist is an error

            Assert.False(this.powershell.HadErrors);
            Assert.IsType<TreesorItem>(result.Last().BaseObject);
            Assert.Equal("TreesorDriveProvider\\Treesor::", result.Last().Properties["PSPath"].Value);

            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.VerifyAll();
        }

        [Fact]
        public void Powershell_retrieves_existing_node_item()
        {
            // ARRANGE

            Reference<Guid> id_root;
            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item", "a")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item", "a"), id_root = new Reference<Guid>(Guid.NewGuid())));

            // ACT
            // test for a missing item

            this.powershell
                .AddStatement()
                .AddCommand("Get-Item").AddParameter("Path", @"custTree:\item\a");

            var result = this.powershell.Invoke();

            // ASSERT
            // reading an item that doesn't exist is an error

            Assert.False(this.powershell.HadErrors);
            Assert.IsType<TreesorItem>(result.Last().BaseObject);
            Assert.Equal(@"TreesorDriveProvider\Treesor::item\a", result.Last().Properties["PSPath"].Value);
            Assert.Equal(@"TreesorDriveProvider\Treesor::item", result.Last().Properties["PSParentPath"].Value);

            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item", "a")), Times.Once());
            this.treesorService.VerifyAll();
        }

        #endregion Get-Item > GetItem, MakePath

        #region Set-Item > ItemExists, SetItem

        [Fact]
        public void Powershell_sets_root_item()
        {
            // ARRANGE
            // setting a value isn't supported currently

            this.treesorService
                .Setup(s => s.SetItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Throws<NotSupportedException>();

            // ACT
            // setting a missing item fails

            this.powershell
                .AddStatement()
                .AddCommand("Set-Item").AddParameter("Path", @"custTree:\").AddParameter("Value", "value");

            var result = this.powershell.Invoke();

            // ASSERT
            // ps invokes SetItem in any case to create or update the Item

            Assert.True(this.powershell.HadErrors);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Never());
            this.treesorService.Verify(s => s.SetItem(TreesorNodePath.RootPath, "value"), Times.Once());
        }

        #endregion Set-Item > ItemExists, SetItem

        #region Clear-Item > ItemExists, ClearItem

        [Fact]
        public void Powershell_clears_root_item()
        {
            // ACT
            // getting a missing item fails

            this.powershell
                .AddStatement()
                .AddCommand("Clear-Item").AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // clearing an item is donw always

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Never());
            this.treesorService.Verify(s => s.ClearItem(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.VerifyAll();
        }

        #endregion Clear-Item > ItemExists, ClearItem
    }
}