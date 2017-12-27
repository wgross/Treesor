using Moq;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDriveDynamicPropertyCmdletProviderTest : IDisposable
    {
        private Mock<ITreesorService> treesorService;
        private PowerShell powershell;

        public TreesorDriveDynamicPropertyCmdletProviderTest()
        {
            this.treesorService = new Mock<ITreesorService>();
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

        #region New-ItemProperty - Not Supported

        [Fact]
        public void Powershell_fails_on_NewItemProperty()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("New-ItemProperty")
                .AddParameter("Path", @"treesor:\a").AddParameter("Name", "p")
                .AddParameter("PropertyType", "string").AddParameter("Value", "value");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.True(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CreateColumn("p", It.IsAny<Type>()), Times.Never());
            this.treesorService.Verify(s => s.SetPropertyValue(TreesorNodePath.Create("a"), "p", "value"), Times.Never());
        }

        #endregion New-ItemProperty - Not Supported

        #region Remove-ItemProperty - Not Supported

        [Fact]
        public void Powershell_fails_on_RemoveItemProperty()
        {
            // ACT

            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Remove-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.True(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RemoveColumn("p"), Times.Never());
        }

        #endregion Remove-ItemProperty - Not Supported

        #region Rename-ItemProperty - NotSupported

        [Fact]
        public void Powershell_fails_on_RenameProperty()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Rename-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name", "p").AddParameter("NewName", "q");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.True(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RenameColumn("p", "q"), Times.Never());
        }

        #endregion Rename-ItemProperty - NotSupported

        #region Set-ItemProperty > SetPropertyValue

        [Fact]
        public void Powershell_creates_property_at_root_node()
        {
            // ACT

            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Set-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name", "p").AddParameter("Value", value);

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.SetPropertyValue(TreesorNodePath.RootPath, "p", value), Times.Once());
        }

        [Fact]
        public void Powershell_creates_property_at_inner_node()
        {
            // ACT

            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Set-ItemProperty")
                .AddParameter("Path", @"treesor:\a").AddParameter("Name", "p").AddParameter("Value", value);

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.SetPropertyValue(TreesorNodePath.Create("a"), "p", value), Times.Once());
        }

        #endregion Set-ItemProperty > SetPropertyValue

        #region Get-ItemProperty > GetPropertyValue

        [Fact]
        public void Powershell_retrieves_root_property_value()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.GetPropertyValue(TreesorNodePath.RootPath, "p"))
                .Returns((object)5);

            // ACT

            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Get-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);
            Assert.Equal(5, (int)(result.Last().BaseObject));

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.GetPropertyValue(TreesorNodePath.RootPath, "p"), Times.Once());
        }

        [Fact]
        public void Powershell_retrieves_inner_node_property_value()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.GetPropertyValue(TreesorNodePath.Create("a"), "p"))
                .Returns((object)5);

            // ACT

            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Get-ItemProperty")
                .AddParameter("Path", @"treesor:\a").AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);
            Assert.Equal(5, (int)(result.Last().BaseObject));

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.GetPropertyValue(TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        #endregion Get-ItemProperty > GetPropertyValue

        #region Clear-ItemProperty > ClearPropertyValue

        [Fact]
        public void Powershell_clears_root_property_value()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ClearPropertyValue(TreesorNodePath.RootPath, "p"));

            // ACT

            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Clear-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ClearPropertyValue(TreesorNodePath.RootPath, "p"), Times.Once());
        }

        [Fact]
        public void Powershell_clears_inner_node_property_value()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.ClearPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ACT

            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Clear-ItemProperty")
                .AddParameter("Path", @"treesor:\a").AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ClearPropertyValue(TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        #endregion Clear-ItemProperty > ClearPropertyValue

        #region Copy-ItemProperty > CopyPropertyValue

        [Fact]
        public void Powershell_copies_property_value_from_root_node()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Copy-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name", "p")
                .AddParameter("Destination", @"treesor:\a");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        [Fact]
        public void Powershell_copies_property_value_from_inner_node()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Copy-ItemProperty")
                .AddParameter("Path", @"treesor:\a").AddParameter("Name", "p")
                .AddParameter("Destination", @"treesor:\");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CopyPropertyValue(TreesorNodePath.Create("a"), "p", TreesorNodePath.RootPath, "p"), Times.Once());
        }

        #endregion Copy-ItemProperty > CopyPropertyValue

        #region Move-ItemProperty > MovePropertyValue

        [Fact]
        public void Powershell_moves_property_value_from_root_node()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Move-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name", "p")
                .AddParameter("Destination", @"treesor:\a");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        [Fact]
        public void Powershell_moves_property_value_from_inner_node()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Move-ItemProperty")
                .AddParameter("Path", @"treesor:\a").AddParameter("Name", "p")
                .AddParameter("Destination", @"treesor:\");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.MovePropertyValue(TreesorNodePath.Create("a"), "p", TreesorNodePath.RootPath, "p"), Times.Once());
        }

        #endregion Move-ItemProperty > MovePropertyValue
    }
}