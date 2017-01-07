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

            this.treesorService.Setup(s => s.HasChildItems(TreesorNodePath.Create("item"))).Returns(false);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Remove-Item").AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider wants item removed but not recursive

            this.treesorService.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), false), Times.Once());
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.HasChildItems(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.VerifyAll();

            Assert.IsFalse(this.powershell.HadErrors);
        }

        [Test]
        public void Powershell_removes_missing_item_under_root()
        {
            // ARRANGE
            // PS asks if item has child items

            this.treesorService.Setup(s => s.HasChildItems(TreesorNodePath.Create("item"))).Returns(false);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Remove-Item").AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider wants item removed but not recursive

            this.treesorService.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), false), Times.Once());
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.HasChildItems(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.VerifyAll();

            Assert.IsFalse(this.powershell.HadErrors);
        }

        [Test]
        public void Powershell_removes_children_under_existing_item_under_root()
        {
            // ARRANGE
            // PS asks if root is item is there

            this.treesorService.Setup(s => s.HasChildItems(TreesorNodePath.Create("item"))).Returns(true);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Remove-Item").AddParameter("Path", @"custTree:\item").AddParameter("Recurse");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider wants item removed but not recursive

            this.treesorService.Verify(s => s.RemoveItem(TreesorNodePath.Create("item"), true), Times.Once());
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
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

            Reference<Guid> id_item;
            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item"), id_item = new Reference<Guid>(Guid.NewGuid())));

            Reference<Guid> id_child;
            this.treesorService
                .Setup(s => s.GetChildItems(TreesorNodePath.Create("item")))
                .Returns(new[] { new TreesorItem(TreesorNodePath.Create("child"), id_child = new Reference<Guid>(Guid.NewGuid())) });

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

            Reference<Guid> id_item;
            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item"), id_item = new Reference<Guid>(Guid.NewGuid())));

            Reference<Guid> id_child;
            Reference<Guid> id_grandchild;
            this.treesorService
                .Setup(s => s.GetDescendants(TreesorNodePath.Create("item")))
                .Returns(new[] {
                    new TreesorItem(TreesorNodePath.Create("child"),id_child = new Reference<Guid>( Guid.NewGuid())),
                    new TreesorItem(TreesorNodePath.Create("grandchild"), id_grandchild = new Reference<Guid>( Guid.NewGuid()))
                });

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Get-ChildItem").AddParameter("Path", @"custTree:\item").AddParameter("Recurse");

            var result = this.powershell.Invoke();

            // ASSERT
            // the result contains the single child item of 'item'

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
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
                .Setup(s => s.GetItem(TreesorNodePath.Create("item2")))
                .Returns((TreesorItem)null);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Copy-Item").AddParameter("Path", @"custTree:\item").AddParameter("Destination", @"custTree:\item2");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorService.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false), Times.Once());
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_copies_item_to_new_name_under_root_recursively()
        {
            // ARRANGE

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

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item2")), Times.Once());
            this.treesorService.Verify(s => s.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), true), Times.Once());
            this.treesorService.VerifyAll();
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

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.RenameItem(TreesorNodePath.Create("item"), "item2"), Times.Once());
        }

        #endregion Rename-Item > RenameItem

        #region Resolve-Path > ExpandPath, GetChildItems

        [Test]
        public void Powershell_resolves_wildcard_under_root_node()
        {
            // ARRANGE

            Reference<Guid> id_a, id_b;
            this.treesorService
                .Setup(s => s.GetDescendants(TreesorNodePath.RootPath))
                .Returns(new[] {
                    new TreesorItem(TreesorNodePath.Create("a"), id_a = new Reference<Guid>(Guid.NewGuid())),
                    new TreesorItem(TreesorNodePath.Create("b"), id_b = new Reference<Guid>( Guid.NewGuid())),
                });

            // ACT
            // request expansion of the wild card by the provider

            this.powershell
                .AddStatement()
                .AddCommand("Resolve-Path").AddParameter("Path", "custTree:*");

            var result = this.powershell.Invoke();

            // ASSERT
            // Drive info is resolving the wildcard correctly

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.AreEqual(3, result.Count);
            Assert.IsInstanceOf<PathInfo>(result.ElementAt(1).ImmediateBaseObject);

            var pathInfo = (PathInfo)result.ElementAt(1).ImmediateBaseObject;

            Assert.AreEqual("custTree", pathInfo.Drive.Name);
            Assert.AreEqual(@"custTree:\a", pathInfo.Path);
            Assert.AreEqual("a", pathInfo.ProviderPath);

            Assert.IsInstanceOf<PathInfo>(result.ElementAt(2).ImmediateBaseObject);

            pathInfo = (PathInfo)result.ElementAt(2).ImmediateBaseObject;

            Assert.AreEqual("custTree", pathInfo.Drive.Name);
            Assert.AreEqual(@"custTree:\b", pathInfo.Path);
            Assert.AreEqual("b", pathInfo.ProviderPath);

            this.treesorService.Verify(s => s.GetDescendants(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_resolves_wildcard_under_node()
        {
            // ARRANGE
            // create a hierachy with positive and negative items at each level of selection

            this.treesorService
                .Setup(s => s.GetDescendants(TreesorNodePath.RootPath))
                .Returns(new[] {
                    new TreesorItem(TreesorNodePath.Create("item"), new Reference<Guid>( Guid.NewGuid())),
                    new TreesorItem(TreesorNodePath.Create("item2"), new Reference<Guid>( Guid.NewGuid())),
                    new TreesorItem(TreesorNodePath.Create("item","a1"), new Reference<Guid>( Guid.NewGuid())),
                    new TreesorItem(TreesorNodePath.Create("item","a2"), new Reference<Guid>( Guid.NewGuid())),
                    new TreesorItem(TreesorNodePath.Create("item","b"), new Reference<Guid>( Guid.NewGuid())),
                });

            // ACT
            // request expansion of path

            this.powershell
                .AddStatement()
                .AddCommand("Resolve-Path").AddParameter("Path", @"custTree:item\a*");

            var result = this.powershell.Invoke();

            // ASSERT
            // Drive info is resolving the wildcard correctly

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.AreEqual(3, result.Count);
            Assert.IsInstanceOf<PathInfo>(result.ElementAt(1).ImmediateBaseObject);

            var pathInfo = (PathInfo)result.ElementAt(1).ImmediateBaseObject;

            Assert.AreEqual(@"custTree:\item\a1", pathInfo.Path);

            Assert.IsInstanceOf<PathInfo>(result.ElementAt(2).ImmediateBaseObject);

            pathInfo = (PathInfo)result.ElementAt(2).ImmediateBaseObject;

            Assert.AreEqual(@"custTree:\item\a2", pathInfo.Path);

            this.treesorService.Verify(s => s.GetDescendants(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.VerifyAll();
        }

        #endregion Resolve-Path > ExpandPath, GetChildItems
    }
}