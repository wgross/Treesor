using Moq;
using NUnit.Framework;
using System.IO;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDriveDynamicColumnCmdletProviderTest
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

        #region New-TreesorColumn > CreateColumn

        [Test]
        public void Powershell_creates_new_columnn_to_drive()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("New-TreesorColumn")
                    .AddParameter("DriveName", "custTree")
                    .AddParameter("Name", "p")
                    .AddParameter("TypeName", typeof(string).Name);

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on output pipe

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CreateColumn("p", typeof(string).Name), Times.Once());
        }

        #endregion New-TreesorColumn > CreateColumn

        #region Remove-TreesorColumn > RemoveColumn

        [Test]
        public void Powershell_removes_existing_column_from_drive_by_name()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Remove-TreesorColumn")
                    .AddParameter("DriveName", "custTree")
                    .AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            this.treesorService.Verify(s => s.RemoveColumn("p"), Times.Once());
        }

        #endregion Remove-TreesorColumn > RemoveColumn

        #region Rename-TreesorColumn > RenameColumn

        [Test]
        public void Powershell_renames_existing_column_by_name()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Rename-TreesorColumn")
                    .AddParameter("DriveName", "custTree")
                    .AddParameter("Name", "p")
                    .AddParameter("NewName", "q");

            var result = this.powershell.Invoke();

            // ASSERT

            this.treesorService.Verify(s => s.RenameColumn("p", "q"), Times.Once());
        }

        #endregion Rename-TreesorColumn > RenameColumn
    }
}