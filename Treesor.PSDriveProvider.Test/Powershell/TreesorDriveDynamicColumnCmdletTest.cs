using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using Treesor.Model;
using Xunit;
using static Treesor.PSDriveProvider.Test.TestDataGenerators;

namespace Treesor.PSDriveProvider.Test
{
    [Collection("Uses_powershell")]
    public class TreesorDriveDynamicColumnCmdletProviderTest : IDisposable
    {
        private PowerShell powershell;
        private Mock<ITreesorModel> treesorModel;

        public TreesorDriveDynamicColumnCmdletProviderTest()
        {
            this.treesorModel = new Mock<ITreesorModel>();

            TreesorDriveInfo.TreesorModelFactory = _ => this.treesorModel.Object;

            this.powershell = ShellWithDriveCreated();
        }

        public void Dispose()
        {
            this.powershell.Stop();
            this.powershell.Dispose();
        }

        #region New-TreesorColumn > CreateColumn

        [Fact]
        public void Powershell_adds_new_column_with_type_name_to_named_drive()
        {
            // ARRANGE

            this.treesorModel
                .Setup(s => s.CreateColumn("p", typeof(string)))
                .Returns(new TreesorColumn("p", typeof(string)));

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("New-TreesorColumn")
                    .AddParameter("DriveName", "custTree")
                    .AddParameter("Name", "p")
                    .AddParameter("ColumnType", typeof(string).ToString());

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new column on output pipe

            Assert.False(this.powershell.HadErrors);
            Assert.Equal("p", ((TreesorColumn)result.Single().BaseObject).Name);
            Assert.Equal(typeof(string), ((TreesorColumn)result.Single().BaseObject).Type);

            this.treesorModel.Verify(s => s.CreateColumn("p", typeof(string)), Times.Once());
        }

        [Fact]
        public void Powershell_adds_new_column_with_type_name_to_current_drive()
        {
            // ARRANGE

            this.powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddParameter("Path", @"custTree:\");

            this.treesorModel
                .Setup(s => s.CreateColumn("p", typeof(string)))
                .Returns(new TreesorColumn("p", typeof(string)));

            // ACT

            this.powershell
                .AddStatement()
                    .AddCommand("New-TreesorColumn")
                        .AddParameter("Name", "p")
                        .AddParameter("ColumnType", typeof(string).ToString());

            var result = this.powershell.Invoke();

            // ASSERT
            // provider write new item on output pipe

            Assert.False(this.powershell.HadErrors);
            Assert.Equal("p", ((TreesorColumn)result.Single().BaseObject).Name);
            Assert.Equal(typeof(string), ((TreesorColumn)result.Single().BaseObject).Type);

            this.treesorModel.Verify(s => s.CreateColumn("p", typeof(string)), Times.Once());
        }

        [Fact]
        public void Powershell_adds_new_column_with_type_to_named_drive()
        {
            // ARRANGE

            this.treesorModel
                .Setup(s => s.CreateColumn("p", typeof(int)))
                .Returns(new TreesorColumn("p", typeof(int)));

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

            Assert.False(this.powershell.HadErrors);
            Assert.Equal("p", ((TreesorColumn)result.Single().BaseObject).Name);
            Assert.Equal(typeof(int), ((TreesorColumn)result.Single().BaseObject).Type);

            this.treesorModel.Verify(s => s.CreateColumn("p", typeof(int)), Times.Once());
        }

        #endregion New-TreesorColumn > CreateColumn

        #region Remove-TreesorColumn > RemoveColumn

        [Fact]
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

            Assert.False(this.powershell.HadErrors);

            this.treesorModel.Verify(s => s.RemoveColumn("p"), Times.Once());
        }

        [Fact]
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

            Assert.False(this.powershell.HadErrors);

            this.treesorModel.Verify(s => s.RemoveColumn("p"), Times.Once());
        }

        #endregion Remove-TreesorColumn > RemoveColumn

        #region Rename-TreesorColumn > RenameColumn

        [Fact]
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

            Assert.False(this.powershell.HadErrors);

            this.treesorModel.Verify(s => s.RenameColumn("p", "q"), Times.Once());
        }

        [Fact]
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

            Assert.False(this.powershell.HadErrors);

            this.treesorModel.Verify(s => s.RenameColumn("p", "q"), Times.Once());
        }

        #endregion Rename-TreesorColumn > RenameColumn

        #region Get-TreesorColumn > GetColumn

        [Fact]
        public void Powershell_retrieves_empty_column_set_from_named_drive()
        {
            // ARRANGE

            this.treesorModel
                .Setup(s => s.GetColumns()).Returns(Enumerable.Empty<TreesorColumn>());

            // ACT

            this.powershell
                .AddStatement()
                    .AddCommand("Get-TreesorColumn").AddParameter("DriveName", "custTree");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);
            Assert.Equal(0, result.Count());

            this.treesorModel.Verify(s => s.GetColumns(), Times.Once());
        }

        [Fact]
        public void Powershell_retrieves_column_set_from_named_drive()
        {
            // ARRANGE

            this.treesorModel
                .Setup(s => s.GetColumns()).Returns(new[] { new TreesorColumn("p", typeof(string)) });

            // ACT

            this.powershell
                .AddStatement()
                    .AddCommand("Get-TreesorColumn").AddParameter("DriveName", "custTree");

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.False(this.powershell.HadErrors);
            Assert.Equal(1, result.Count());
            Assert.Equal("p", ((TreesorColumn)result.Single().BaseObject).Name);
            Assert.Equal(typeof(string), ((TreesorColumn)result.Single().BaseObject).Type);

            this.treesorModel.Verify(s => s.GetColumns(), Times.Once());
        }

        [Fact]
        public void Powershell_retrieves_column_set_from_current_drive()
        {
            // ARRANGE

            this.treesorModel
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

            Assert.False(this.powershell.HadErrors);
            Assert.Equal(1, result.Count());
            Assert.Equal("p", ((TreesorColumn)result.Single().BaseObject).Name);
            Assert.Equal(typeof(string), ((TreesorColumn)result.Single().BaseObject).Type);

            this.treesorModel.Verify(s => s.GetColumns(), Times.Once());
        }

        #endregion Get-TreesorColumn > GetColumn
    }
}