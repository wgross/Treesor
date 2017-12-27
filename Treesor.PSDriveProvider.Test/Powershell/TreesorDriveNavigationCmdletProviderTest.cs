using Moq;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Treesor.Model;
using Xunit;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDriveNavigationCmdletProviderTest : IDisposable
    {
        private PowerShell powershell;
        private Mock<ITreesorModel> treesorService;

        public TreesorDriveNavigationCmdletProviderTest()
        {
            this.treesorService = new Mock<ITreesorModel>();
            InMemoryTreesorService.Factory = uri => treesorService.Object;

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

        
        public void Dispose()
        {
            this.powershell.Stop();
            this.powershell.Dispose();
        }

        #region Set-Location

        [Fact]
        public void Powershell_sets_location_to_root_container()
        {
            // ARRANGE

            Reference<Guid> id_root;
            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.RootPath))
                .Returns(new TreesorItem(TreesorNodePath.RootPath, id_root = new Reference<Guid>(Guid.NewGuid())));

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Set-Location").AddParameter("Path", @"treesor:\");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.RootPath), Times.Never());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.RootPath), Times.Once());
            this.treesorService.VerifyAll();
        }

        [Fact]
        public void Powershell_gets_location_from_root_container()
        {
            // ARRANGE

            Reference<Guid> id_root;
            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.RootPath))
                .Returns(new TreesorItem(TreesorNodePath.RootPath, id_root = new Reference<Guid>(Guid.NewGuid())));

            this.powershell
                .AddStatement()
                .AddCommand("Set-Location").AddParameter("Path", @"treesor:\");

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Get-Location");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);
            Assert.IsType<PathInfo>(result.Last().BaseObject);
            Assert.Equal(TreesorNodePath.Create(@"treesor:\"), TreesorNodePath.Create(((PathInfo)result.Last().BaseObject).Path));

            this.treesorService.VerifyAll();
        }

        [Fact]
        public void Powershell_set_location_to_container_under_root()
        {
            // ARRANGE

            Reference<Guid> id_item;
            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item"), id_item = new Reference<Guid>(Guid.NewGuid())));

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Set-Location").AddParameter("Path", @"treesor:\item");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Never());
            this.treesorService.Verify(s => s.GetItem(TreesorNodePath.Create("item")), Times.Once());
            this.treesorService.VerifyAll();
        }

        [Fact]
        public void Powershell_gets_location_from_container_under_root()
        {
            // ARRANGE

            Reference<Guid> id_item;
            this.treesorService
                .Setup(s => s.GetItem(TreesorNodePath.Create("item")))
                .Returns(new TreesorItem(TreesorNodePath.Create("item"), id_item = new Reference<Guid>(Guid.NewGuid())));

            this.powershell
                .AddStatement()
                .AddCommand("Set-Location").AddParameter("Path", @"treesor:\item");

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Get-Location");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);
            Assert.IsType<PathInfo>(result.Last().BaseObject);
            Assert.Equal(TreesorNodePath.Create(@"treesor:\item"), TreesorNodePath.Create(((PathInfo)result.Last().BaseObject).Path));

            this.treesorService.VerifyAll();
        }

        #endregion Set-Location

        #region Move-Item > MoveItem

        [Fact]
        public void Powershell_moves_child_of_root_under_other_child()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item1")))
                .Returns(true);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Move-Item").AddParameter("Path", @"treesor:\item1").AddParameter("Destination", @"treesor:\item2");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.MoveItem(TreesorNodePath.Create("item1"), TreesorNodePath.Create("item2")), Times.Once());
            this.treesorService.VerifyAll();
        }

        #endregion Move-Item > MoveItem
    }
}