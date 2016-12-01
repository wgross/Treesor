using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Test
{
    [TestFixture]
    public class TreesorDriveContainerCmdletProviderTest
    {
        private PowerShell powershell;
        private Mock<ITreesorService> treesorService;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.treesorService = new Mock<ITreesorService>();
            TreesorService.Factory = uri => treesorService.Object;

            this.powershell = PowerShell.Create(RunspaceMode.NewRunspace);
            this.powershell
                .AddStatement()
                .AddCommand("Set-Location")
                .AddArgument(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

            this.powershell
                .AddStatement()
                .AddCommand("Import-Module").AddArgument("./TreesorDriveProvider.dll");
            this.powershell
                .AddStatement()
                .AddCommand("New-PsDrive").AddParameter("Name", "custTree").AddParameter("PsProvider", "Treesor").AddParameter("Root", @"\");
        }

        [TearDown]
        public void TearDownAllTests()
        {
            this.powershell.Stop();
            this.powershell.Dispose();
        }

        #region Remove-Item > RemoveItem

        [Test]
        public void Powershell_removes_existing_item_under_root()
        {
            // ARRANGE
            // PS asks if root is item is there

            this.treesorService.Setup(s => s.ItemExists(TreesorNodePath.Create("item"))).Returns(true);
            this.treesorService.Setup(s => s.HasChildItems(TreesorNodePath.Create("item"))).Returns(false);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Remove-Item").AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider wants item removed but not recursive

            this.treesorService.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), false), Times.Once());
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.HasChildItems(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.VerifyAll();

            Assert.IsFalse(this.powershell.HadErrors);
        }

        [Test]
        public void Powershell_removes_missing_item_under_root()
        {
            // ARRANGE
            // PS asks if root is item is there

            this.treesorService.Setup(s => s.ItemExists(TreesorNodePath.Create("item"))).Returns(false);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Remove-Item").AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider wants item removed but not recursive

            this.treesorService.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), false), Times.Never());
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.HasChildItems(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.VerifyAll();

            Assert.IsTrue(this.powershell.HadErrors);
        }

        [Test]
        public void Powershell_removes_children_under_existing_item_under_root()
        {
            // ARRANGE
            // PS asks if root is item is there

            this.treesorService.Setup(s => s.ItemExists(TreesorNodePath.Create("item"))).Returns(true);
            this.treesorService.Setup(s => s.HasChildItems(TreesorNodePath.Create("item"))).Returns(true);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Remove-Item").AddParameter("Path", @"custTree:\item").AddParameter("Recurse");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider wants item removed but not recursive

            this.treesorService.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), true), Times.Once());
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.HasChildItems(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.VerifyAll();

            Assert.IsFalse(this.powershell.HadErrors);
        }

        #endregion Remove-Item > RemoveItem

        #region Get-ChildItem > GetChildItem

        [Test]
        public void Powershell_retrieves_child_items_of_item_under_root()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            Guid id_item;
            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item"), id_item = Guid.NewGuid() ));

            Guid id_child;
            this.treesorService
                .Setup(s => s.GetChildItems(TreesorNodePath.Create("item")))
                .Returns(new[] { new TreesorItem(TreesorNodePath.Create("child"), id_child = Guid.NewGuid()) });

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Get-ChildItem").AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // the result cotains the single child item of 'item'
            
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.GetChildItems(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_retrieves_grandchildren_items_of_item_under_root()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            Guid id_item;
            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item"), id_item = Guid.NewGuid()));

            Guid id_child;
            Guid id_grandchild;
            this.treesorService
                .Setup(s => s.GetDescendants(TreesorNodePath.Create("item")))
                .Returns(new[] {
                    new TreesorItem(TreesorNodePath.Create("child"),id_child = Guid.NewGuid()),
                    new TreesorItem(TreesorNodePath.Create("grandchild"), id_grandchild = Guid.NewGuid())
                });

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Get-ChildItem").AddParameter("Path", @"custTree:\item").AddParameter("Recurse");

            var result = this.powershell.Invoke();

            // ASSERT
            // the result contains the single child item of 'item'

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Exactly(2)); // because of isContainer
            this.treesorService.Verify(s => s.GetDescendants(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.VerifyAll();
        }

        #endregion Get-ChildItem > GetChildItem

        #region Copy-Item > CopyItem

        [Test]
        public void Powershell_copies_Item_to_new_name_under_root()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Returns((TreesorItem)null);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Copy-Item").AddParameter("Path", @"custTree:\item").AddParameter("Destination", @"custTree:\item2");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorService.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false), Times.Once());
        }

        [Test]
        public void Powershell_copies_item_to_new_name_under_root_recursively()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Returns((TreesorItem)null);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Copy-Item").AddParameter("Path", @"custTree:\item").AddParameter("Destination", @"custTree:\item2").AddParameter("Recurse");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorService.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), true), Times.Once());
        }

        #endregion Copy-Item > CopyItem

        #region Rename-Item > RenameItem

        [Test]
        public void Powershell_renames_item_under_root()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Returns((TreesorItem)null);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Rename-Item").AddParameter("Path", @"custTree:\item").AddParameter("NewName", "item2");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Exactly(2));
            this.treesorService.Verify(s => s.RenameItem(TreesorNodePath.Create("item"), "item2"), Times.Once());
        }

        #endregion Rename-Item > RenameItem
    }
}