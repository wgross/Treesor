using Moq;
using NUnit.Framework;
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

        #region New-Item > NewItem

        [Test]
        public void Provider_creates_new_item_under_root()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.NewItem(It.IsAny<TreesorNodePath>(), It.IsAny<object>()))
                .Returns<TreesorNodePath, object>((p, v) => new TreesorItem(p));

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

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsInstanceOf<TreesorItem>(result.Last().BaseObject);
        }

        #endregion New-Item > NewItem

        #region Remove-Item > RemoveItem

        [Test]
        public void Provider_removes_existing_item_under_root()
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
        public void Provider_removes_missing_item_under_root()
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
        public void Provider_removes_children_under_existing_item_under_root()
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
        public void Provider_retrieves_child_items_of_item_under_root()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item")));

            this.treesorService
                .Setup(s => s.GetChildItems(TreesorNodePath.Create("item")))
                .Returns(new[] { new TreesorItem(TreesorNodePath.Create("child")) });

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
        public void Provider_retrieves_grandchildren_items_of_item_under_root()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item")));

            this.treesorService
                .Setup(s => s.GetDescendants(TreesorNodePath.Create("item")))
                .Returns(new[] {
                    new TreesorItem(TreesorNodePath.Create("child")),
                    new TreesorItem(TreesorNodePath.Create("grandchild"))
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
    }
}