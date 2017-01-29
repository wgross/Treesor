using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
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

            // there is always a root node (for Set-Location)
            this.treesorService.Setup(s => s.GetItem(TreesorNodePath.RootPath)).Returns(new TreesorItem(TreesorNodePath.RootPath, new Reference<System.Guid>(Guid.NewGuid())));

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

            this.powershell.Invoke();
            this.powershell.Commands.Clear();
        }

        [TearDown]
        public void TearDownAllTests()
        {
            this.powershell.Stop();
            this.powershell.Dispose();
        }

        #region New-TreesorColumn > CreateColumn

        [Test]
        public void Powershell_adds_new_column_with_type_name_to_named_drive()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("New-TreesorColumn")
                    .AddParameter("DriveName", "custTree")
                    .AddParameter("Name", "p")
                    .AddParameter("ColumnType", typeof(string).ToString());

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on output pipe

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CreateColumn("p", typeof(string)), Times.Once());
        }

        [Test]
        public void Powershell_adds_new_column_with_type_name_to_current_drive()
        {
            // ARRANGE

            this.powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddParameter("Path", @"custTree:\");

            // ACT

            this.powershell
                .AddStatement()
                    .AddCommand("New-TreesorColumn")
                        .AddParameter("Name", "p")
                        .AddParameter("ColumnType", typeof(string).ToString());

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on output pipe

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CreateColumn("p", typeof(string)), Times.Once());
        }

        [Test]
        public void Powershell_adds_new_column_with_type_to_named_drive()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("New-TreesorColumn")
                    .AddParameter("DriveName", "custTree")
                    .AddParameter("Name", "p")
                    .AddParameter("ColumnType", typeof(int));

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on output pipe

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.CreateColumn("p", typeof(int)), Times.Once());
        }

        #endregion New-TreesorColumn > CreateColumn

        #region Remove-TreesorColumn > RemoveColumn

        [Test]
        public void Powershell_removes_existing_column_from_named_drive()
        {
            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Remove-TreesorColumn")
                    .AddParameter("DriveName", "custTree")
                    .AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RemoveColumn("p"), Times.Once());
        }

        [Test]
        public void Powershell_removes_existing_column_from_current_drive()
        {
            // ARRANGE

            this.powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddParameter("Path", @"custTree:\");

            this.powershell.Invoke();
            this.powershell.Commands.Clear();

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Remove-TreesorColumn")
                    .AddParameter("Name", "p");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RemoveColumn("p"), Times.Once());
        }

        #endregion Remove-TreesorColumn > RemoveColumn

        #region Rename-TreesorColumn > RenameColumn

        [Test]
        public void Powershell_renames_existing_column_at_named_drive()
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

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RenameColumn("p", "q"), Times.Once());
        }

        [Test]
        public void Powershell_renames_existing_column_at_current_drive()
        {
            // ARRANGE

            this.powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddParameter("Path", @"custTree:\");

            this.powershell.Invoke();
            this.powershell.Commands.Clear();

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Rename-TreesorColumn")
                    .AddParameter("Name", "p")
                    .AddParameter("NewName", "q");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.RenameColumn("p", "q"), Times.Once());
        }

        #endregion Rename-TreesorColumn > RenameColumn

        #region Get-TreesorColumn > GetColumn

        [Test]
        public void Powershell_retrieves_empty_column_set_from_named_drive()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.GetColumns()).Returns(Enumerable.Empty<TreesorColumn>());

            // ACT

            this.powershell
                .AddStatement()
                    .AddCommand("Get-TreesorColumn").AddParameter("DriveName", "custTree");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.AreEqual(0, result.Count());

            this.treesorService.Verify(s => s.GetColumns(), Times.Once());
        }

        [Test]
        public void Powershell_retrieves_column_set_from_named_drive()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.GetColumns()).Returns(new[] { new TreesorColumn("p", typeof(string)) });

            // ACT

            this.powershell
                .AddStatement()
                    .AddCommand("Get-TreesorColumn").AddParameter("DriveName", "custTree");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("p", ((TreesorColumn)result.Single().BaseObject).Name);
            Assert.AreEqual(typeof(string), ((TreesorColumn)result.Single().BaseObject).Type);

            this.treesorService.Verify(s => s.GetColumns(), Times.Once());
        }

        [Test]
        public void Powershell_retrieves_column_set_from_current_drive()
        {
            // ARRANGE

            this.treesorService
                .Setup(s => s.GetColumns()).Returns(new[] { new TreesorColumn("p", typeof(string)) });

            // ARRANGE

            this.powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddParameter("Path", @"custTree:\");

            this.powershell.Invoke();
            this.powershell.Commands.Clear();

            // ACT

            this.powershell
                .AddStatement()
                    .AddCommand("Get-TreesorColumn").AddParameter("DriveName", "custTree");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("p", ((TreesorColumn)result.Single().BaseObject).Name);
            Assert.AreEqual(typeof(string), ((TreesorColumn)result.Single().BaseObject).Type);

            this.treesorService.Verify(s => s.GetColumns(), Times.Once());
        }

        #endregion Get-TreesorColumn > GetColumn
    }
}