using Moq;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDriveNavigationCmdletProviderTest
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

        #region Set-Location

        [Test]
        public void Provider_sets_location_to_root_container()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.RootPath))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.RootPath))
                .Returns(new TreesorItem(TreesorNodePath.RootPath));

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Set-Location").AddParameter("Path", @"treesor:\");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Provider_gets_location_from_root_container()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.RootPath))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.RootPath))
                .Returns(new TreesorItem(TreesorNodePath.RootPath));

            this.powershell
                .AddStatement()
                .AddCommand("Set-Location").AddParameter("Path", @"treesor:\");

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Get-Location");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsInstanceOf<PathInfo>(result.Last().BaseObject);
            Assert.AreEqual(TreesorNodePath.Create(@"treesor:\"), TreesorNodePath.Create(((PathInfo)result.Last().BaseObject).Path));

            this.treesorService.VerifyAll();
        }

        [Test]
        public void Provider_set_location_to_container_under_root()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item")));

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Set-Location").AddParameter("Path", @"treesor:\item");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Provider_gets_location_from_container_under_root()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item")))
                .Returns(true);

            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item")));

            this.powershell
                .AddStatement()
                .AddCommand("Set-Location").AddParameter("Path", @"treesor:\item");

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Get-Location");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsInstanceOf<PathInfo>(result.Last().BaseObject);
            Assert.AreEqual(TreesorNodePath.Create(@"treesor:\item"), TreesorNodePath.Create(((PathInfo)result.Last().BaseObject).Path));

            this.treesorService.VerifyAll();
        }

        #endregion Set-Location
    }
}