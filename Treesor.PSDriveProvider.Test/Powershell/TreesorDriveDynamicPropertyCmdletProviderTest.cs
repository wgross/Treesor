using Moq;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDriveDynamicPropertyCmdletProviderTest
    {
        private Mock<ITreesorService> treesorService;
        private PowerShell powershell;

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

        #region New-ItemProperty > CreateColumns/SetPropertyValue

        [Test]
        public void Powershell_creates_new_column_at_any_node()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("New-ItemProperty")
                .AddParameter("Path", @"treesor:\a").AddParameter("Name", "p")
                .AddParameter("PropertyType", "type").AddParameter("Value", "value");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CreateColumn("p", "type"), Times.Once());
            this.treesorService.Verify(s => s.SetPropertyValue(TreesorNodePath.Create("a"), "p", "value"), Times.Once());
        }

        #endregion New-ItemProperty > CreateColumns/SetPropertyValue

        #region Set-ItemProperty > SetPropertyValue

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.SetPropertyValue(TreesorNodePath.RootPath, "p", value), Times.Once());
        }

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.SetPropertyValue(TreesorNodePath.Create("a"), "p", value), Times.Once());
        }

        #endregion Set-ItemProperty > SetPropertyValue

        #region Get-ItemProperty > GetPropertyValue

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.AreEqual(5, (int)(result.Last().BaseObject));

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.GetPropertyValue(TreesorNodePath.RootPath, "p"), Times.Once());
        }

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.AreEqual(5, (int)(result.Last().BaseObject));

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.GetPropertyValue(TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        #endregion Get-ItemProperty > GetPropertyValue

        #region Clear-ItemProperty > ClearPropertyValue

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ClearPropertyValue(TreesorNodePath.RootPath, "p"), Times.Once());
        }

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.VerifyAll();
            this.treesorService.Verify(s => s.ClearPropertyValue(TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        #endregion Clear-ItemProperty > ClearPropertyValue

        #region Copy-ItemProperty > CopyPropertyValue

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CopyPropertyValue(TreesorNodePath.Create("a"), "p", TreesorNodePath.RootPath, "p"), Times.Once());
        }

        #endregion Copy-ItemProperty > CopyPropertyValue

        #region Move-ItemProperty > MovePropertyValue

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        [Test]
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

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.MovePropertyValue(TreesorNodePath.Create("a"), "p", TreesorNodePath.RootPath, "p"), Times.Once());
        }

        #endregion Move-ItemProperty > MovePropertyValue

        #region Remove-ItemProperty > RemoveProperty

        [Test]
        public void Powershell_removesProperty_from_root_node()
        {
            // ACT

            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Remove-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RemoveProperty(TreesorNodePath.RootPath, "p"), Times.Once());
        }

        [Test]
        public void Powershell_removesProperty_from_inner_node()
        {
            // ACT

            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Remove-ItemProperty")
                .AddParameter("Path", @"treesor:\a").AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RemoveProperty(TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        #endregion Remove-ItemProperty > RemoveProperty

        #region Rename-ItemProperty > RenameProperty

        [Test]
        public void Powershell_renames_property_at_root_node()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Rename-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name", "p").AddParameter("NewName", "q");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RenameProperty(TreesorNodePath.RootPath, "p", "q"), Times.Once());
        }

        [Test]
        public void Powershell_renames_property_at_inner_node()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Rename-ItemProperty")
                .AddParameter("Path", @"treesor:\a").AddParameter("Name", "p").AddParameter("NewName", "q");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RenameProperty(TreesorNodePath.Create("a"), "p", "q"), Times.Once());
        }

        #endregion Rename-ItemProperty > RenameProperty
    }
}