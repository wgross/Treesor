using Moq;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Treesor.Model;
using Xunit;
using static Treesor.PSDriveProvider.Test.TestDataGenerators;

namespace Treesor.PSDriveProvider.Test
{
    [Collection("Uses_powershell")]
    public class TreesorDriveDynamicPropertyCmdletProviderTest : IDisposable
    {
        private Mock<ITreesorModel> treesorModel;
        private PowerShell powershell;

        public TreesorDriveDynamicPropertyCmdletProviderTest()
        {
            this.treesorModel = new Mock<ITreesorModel>();

            TreesorDriveInfo.TreesorModelFactory = _ => treesorModel.Object;

            this.powershell = ShellWithDriveCreated();
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

            this.treesorModel.Verify(s => s.CreateColumn("p", It.IsAny<Type>()), Times.Never());
            this.treesorModel.Verify(s => s.SetPropertyValue(TreesorNodePath.Create("a"), "p", "value"), Times.Never());
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

            this.treesorModel.Verify(s => s.RemoveColumn("p"), Times.Never());
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

            this.treesorModel.Verify(s => s.RenameColumn("p", "q"), Times.Never());
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

            this.treesorModel.Verify(s => s.SetPropertyValue(TreesorNodePath.RootPath, "p", value), Times.Once());
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

            this.treesorModel.Verify(s => s.SetPropertyValue(TreesorNodePath.Create("a"), "p", value), Times.Once());
        }

        #endregion Set-ItemProperty > SetPropertyValue

        #region Get-ItemProperty > GetPropertyValue

        [Fact]
        public void Powershell_retrieves_root_property_value()
        {
            // ARRANGE

            this.treesorModel
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

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.GetPropertyValue(TreesorNodePath.RootPath, "p"), Times.Once());
        }

        [Fact]
        public void Powershell_retrieves_inner_node_property_value()
        {
            // ARRANGE

            this.treesorModel
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

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.GetPropertyValue(TreesorNodePath.Create("a"), "p"), Times.Once());
        }

        #endregion Get-ItemProperty > GetPropertyValue

        #region Clear-ItemProperty > ClearPropertyValue

        [Fact]
        public void Powershell_clears_root_property_value()
        {
            // ARRANGE

            this.treesorModel
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

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ClearPropertyValue(TreesorNodePath.RootPath, "p"), Times.Once());
        }

        [Fact]
        public void Powershell_clears_inner_node_property_value()
        {
            // ARRANGE

            this.treesorModel
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

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ClearPropertyValue(TreesorNodePath.Create("a"), "p"), Times.Once());
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

            this.treesorModel.Verify(s => s.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "p"), Times.Once());
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

            this.treesorModel.Verify(s => s.CopyPropertyValue(TreesorNodePath.Create("a"), "p", TreesorNodePath.RootPath, "p"), Times.Once());
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

            this.treesorModel.Verify(s => s.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "p"), Times.Once());
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

            this.treesorModel.Verify(s => s.MovePropertyValue(TreesorNodePath.Create("a"), "p", TreesorNodePath.RootPath, "p"), Times.Once());
        }

        #endregion Move-ItemProperty > MovePropertyValue
    }
}