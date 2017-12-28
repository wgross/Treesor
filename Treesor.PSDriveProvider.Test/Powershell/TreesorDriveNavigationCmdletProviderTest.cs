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
    public class TreesorDriveNavigationCmdletProviderTest : IDisposable
    {
        private readonly PowerShell powershell;
        private readonly Mock<ITreesorModel> treesorModel;

        public TreesorDriveNavigationCmdletProviderTest()
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

        #region Set-Location > IsItemContainer > GetItem

        [Theory]
        [InlineData("")]
        [InlineData("item")]
        [InlineData("item\a")]
        public void Powershell_sets_location_to_container(string path)
        {
            // ARRANGE
            // location exists

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Parse(path)))
                .Returns(TreesorItem(TreesorNodePath.Parse(path)));

            // ACT
            // change location

            this.powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddParameter("Path", $@"custTree:\{path}");

            var result = this.powershell.Invoke();

            // ASSERT
            // destination item was retrieved

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Parse(path)), Times.Never());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Parse(path)), Times.Once());

            Assert.False(this.powershell.HadErrors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("item")]
        [InlineData(@"item\a")]
        public void Powershell_sets_location_to_missing_container_fails(string path)
        {
            // ARRANGE
            // location exists

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Parse(path)))
                .Throws(TreesorModelException.MissingItem(path));

            // ACT
            // change location

            this.powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddParameter("Path", $@"custTree:\{path}");

            var result = this.powershell.Invoke();

            // ASSERT
            // destination item was retrieved

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.ItemExists(TreesorNodePath.Parse(path)), Times.Never());
            this.treesorModel.Verify(s => s.GetItem(TreesorNodePath.Parse(path)), Times.Once());

            Assert.True(this.powershell.HadErrors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("item")]
        [InlineData(@"item\a")]
        public void Powershell_gets_location_from_drive(string path)
        {
            // ARRANGE
            // item exists

            this.treesorModel
                .Setup(s => s.GetItem(TreesorNodePath.Parse(path)))
                .Returns(TreesorItem(TreesorNodePath.Parse(path)));

            this.powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddParameter("Path", $@"custTree:\{path}");

            // ACT
            // ask for current location

            this.powershell
                .AddStatement()
                .AddCommand("Get-Location");

            var result = this.powershell.Invoke();

            // ASSERT

            this.treesorModel.VerifyAll();

            Assert.False(this.powershell.HadErrors);
            Assert.IsType<PathInfo>(result.Last().BaseObject);
            Assert.Equal(TreesorNodePath.Create($@"custTree:\{path}"), TreesorNodePath.Create(((PathInfo)result.Last().BaseObject).Path));
        }

        #endregion Set-Location > IsItemContainer > GetItem

        #region Move-Item > MoveItem

        [Fact]
        public void Powershell_moves_child_of_root_under_other_child()
        {
            // ARRANGE

            this.treesorModel
                .Setup(s => s.ItemExists(TreesorNodePath.Create("item1")))
                .Returns(true);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Move-Item").AddParameter("Path", @"custTree:\item1").AddParameter("Destination", @"custTree:\item2");

            var result = this.powershell.Invoke();

            // ASSERT

            this.treesorModel.VerifyAll();
            this.treesorModel.Verify(s => s.MoveItem(TreesorNodePath.Create("item1"), TreesorNodePath.Create("item2")), Times.Once());

            Assert.False(this.powershell.HadErrors);
        }

        #endregion Move-Item > MoveItem
    }
}