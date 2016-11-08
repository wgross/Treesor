using Moq;
using NUnit.Framework;
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

        #region Test-Path > ItemExists

        [Test]
        public void Provider_asks_service_for_existence_of_missing_root()
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
        public void Provider_asks_service_for_existence_of_existing_root()
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
        public void Provider_asks_service_for_existence_of_missing_item()
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
        public void Provider_asks_service_for_existence_of_exiting_item()
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
        public void Provider_retrieves_missing_root_item()
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
        public void Provider_retrieves_existing_root_item()
        {
            // ARRANGE
            this.treesorService.Setup(s => s.ItemExists(TreesorNodePath.RootPath)).Returns(true);
            this.treesorService.Setup(s => s.GetItem(TreesorNodePath.RootPath)).Returns(new TreesorItem());

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
        public void Provider_sets_missing_root_item()
        {
            // ACT
            // setting a missing item fails

            this.powershell
                .AddStatement()
                .AddCommand("Set-Item").AddParameter("Path", @"custTree:\").AddParameter("Value", "value");

            var result = this.powershell.Invoke();

            // ASSERT
            // ps invokes SetItem in any case to create or update the Item

            Assert.IsFalse(this.powershell.HadErrors);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Never());
            this.treesorService.Verify(s => s.SetItem(TreesorNodePath.RootPath, "value"), Times.Once());
        }

        [Test]
        public void Provider_sets_root_item()
        {
            // ARRANGE
            this.treesorService.Setup(s => s.ItemExists(TreesorNodePath.RootPath)).Returns(true);

            // ACT
            // setting a missing item fails

            this.powershell
                .AddStatement()
                .AddCommand("Set-Item").AddParameter("Path", @"custTree:\").AddParameter("Value", "value");

            var result = this.powershell.Invoke();

            // ASSERT
            // ps invokes SetItem in any case to create or update the Item

            Assert.IsFalse(this.powershell.HadErrors);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Never());
            this.treesorService.Verify(s => s.SetItem(TreesorNodePath.RootPath, "value"), Times.Once());
        }

        #endregion Set-Item > ItemExists, SetItem

        #region Clear-Item > ItemExists, ClearItem

        [Test]
        public void Provider_clears_missing_root_item()
        {
            // ACT
            // getting a missing item fails

            this.powershell
                .AddStatement()
                .AddCommand("Clear-Item").AddParameter("Path", @"custTree:\");

            var result = this.powershell.Invoke();

            // ASSERT
            // clearing an item is donw always

            Assert.IsTrue(this.powershell.HadErrors);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.Verify(s => s.ClearItem(TreesorNodePath.RootPath), Times.Never());
        }

        [Test]
        public void Provider_clears_existing_root_item()
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
    }
}