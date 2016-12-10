using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDriveItemCmdletProviderTest
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

        #region New-Item > NewItem

        [Test]
        public void Powershell_creates_new_item_under_root()
        {
            // ARRANGE

            Guid id_item = Guid.NewGuid();
            this.treesorService
                .Setup(s => s.NewItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Returns<TreesorNodePath, object>((p, v) => new TreesorItem(p, id_item));

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("New-Item").AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on outut pipe

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.NewItem(TreesorNodePath.Create("item"), null), Times.Once());
            this.treesorService.VerifyAll();

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsInstanceOf<TreesorItem>(result.Last().BaseObject);
            Assert.AreEqual(id_item, ((TreesorItem)result.Last().BaseObject).Id);
        }

        [Test]
        public void Powershell_creates_new_item_under_root_twice_fails()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.NewItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Throws(new ArgumentException());

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("New-Item").AddParameter("Path", @"custTree:\item");

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on outut pipe

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.NewItem(TreesorNodePath.Create("item"), null), Times.Once());
            this.treesorService.VerifyAll();

            Assert.IsTrue(this.powershell.HadErrors);
        }

        [Test]
        public void Powershell_creates_new_item_under_root_with_value_fails()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.NewItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Throws(new NotSupportedException());

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

            Assert.IsTrue(this.powershell.HadErrors);
        }

        #endregion New-Item > NewItem

        #region Test-Path > ItemExists

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsFalse((bool)result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once);
        }

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsTrue((bool)result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once);
        }

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsFalse((bool)result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once);
        }

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsTrue((bool)result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once);
        }

        #endregion Test-Path > ItemExists

        #region Get-Item > ItemExists, GetItem

        [Test]
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

            Assert.IsTrue(this.powershell.HadErrors);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.RootPath), Times.Never());
        }

        [Test]
        public void Powershell_retrieves_existing_root_item()
        {
            // ARRANGE
            this.treesorService.Setup(s => s.ItemExists(TreesorNodePath.RootPath)).Returns(true);
            Guid id_root;
            this.treesorService.Setup(s => s.GetItem(TreesorNodePath.RootPath)).Returns(new TreesorItem(TreesorNodePath.RootPath, id_root = Guid.NewGuid()));

            // ACT
            // test for a missing item

            this.powershell
                .AddStatement()
                .AddCommand("Get-Item").AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // reading an item that doesn't exist is an error

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsInstanceOf<TreesorItem>(result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.RootPath), Times.Once());
        }

        #endregion Get-Item > ItemExists, GetItem

        #region Set-Item > ItemExists, SetItem

        [Test]
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

            Assert.IsTrue(this.powershell.HadErrors);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Never());
            this.treesorService.Verify(s => s.SetItem(TreesorNodePath.RootPath, "value"), Times.Once());
        }

        #endregion Set-Item > ItemExists, SetItem

        #region Clear-Item > ItemExists, ClearItem

        [Test]
        public void Powershell_clears_missing_root_item()
        {
            // ACT
            // getting a missing item fails

            this.powershell
                .AddStatement()
                .AddCommand("Clear-Item").AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // clearing an item is done always

            Assert.IsTrue(this.powershell.HadErrors);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.Verify(s => s.ClearItem(TreesorNodePath.RootPath), Times.Never());
        }

        [Test]
        public void Powershell_clears_existing_root_item()
        {
            // ARRANGE

            this.treesorService.Setup(s => s.ItemExists(TreesorNodePath.RootPath)).Returns(true);

            // ACT
            // getting a missing item fails

            this.powershell
                .AddStatement()
                .AddCommand("Clear-Item").AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // clearing an item is donw always

            Assert.IsFalse(this.powershell.HadErrors);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.Verify(s => s.ClearItem(TreesorNodePath.RootPath), Times.Once());
        }

        #endregion Clear-Item > ItemExists, ClearItem

        #region Resolve-Path > ExpandPath, GetChildItems

        [Test]
        public void Powershell_resolves_wildcard_under_root_node()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.GetChildItemsByWildcard(TreesorNodePath.Create("*")))
                .Returns(new[]
                {
                    new TreesorItem(TreesorNodePath.Create("a"), Guid.NewGuid()),
                    new TreesorItem(TreesorNodePath.Create("b"), Guid.NewGuid())
                });

            // ACT
            // request expansioon fo the wild card by the provider

            this.powershell
                .AddStatement()
                .AddCommand("Resolve-Path").AddParameter("Path", "custTree:*");

            var result = this.powershell.Invoke();

            // ASSERT
            // treesorservice is resolving the wildcard with the two prepared items

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

            this.treesorService.Verify(s => s.GetChildItemsByWildcard(TreesorNodePath.Create("*")), Times.Once());
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_resolves_wildcard_under_node()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Resolve-Path").AddParameter("Path", @"custTree:item\t*");

            var result = this.powershell.Invoke();

            // ASSERT
            // treesorservice is resolving the wildcard

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.GetChildItemsByWildcard(TreesorNodePath.Create(@"item/t*")), Times.Once());
            this.treesorService.VerifyAll();
        }

        #endregion Resolve-Path > ExpandPath, GetChildItems
    }
}