﻿using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using Treesor.Model;
using Xunit;
using static Treesor.Model.TreesorItemPath;
using static Treesor.PSDriveProvider.Test.TestDataGenerators;

namespace Treesor.PSDriveProvider.Test
{
    [Collection("Uses_powershell")]
    public class TreesorDriveNavigationCmdletProviderTest : IDisposable
    {
        private readonly PowerShell powershell;
        private readonly Mock<ITreesorModel> treesorModel;
        private readonly Mock<ITreesorItemRepository> treesorModelItems;

        public TreesorDriveNavigationCmdletProviderTest()
        {
            this.treesorModelItems = new Mock<ITreesorItemRepository>();
            this.treesorModel = new Mock<ITreesorModel>();
            this.treesorModel.Setup(s => s.Items).Returns(this.treesorModelItems.Object);

            TreesorDriveInfo.TreesorModelFactory = _ => this.treesorModel.Object;

            this.powershell = ShellWithDriveCreated();
        }

        public void Dispose()
        {
            this.powershell.Stop();
            this.powershell.Dispose();
        }

        #region Set-Location > IsItemContainer, GetItem > GetItem

        [Theory]
        [InlineData("")]
        [InlineData("item")]
        [InlineData("item\a")]
        public void Powershell_sets_location_to_container(string path)
        {
            // ARRANGE
            // location exists

            this.treesorModelItems
                .Setup(s => s.Get(ParsePath(path)))
                .Returns(TreesorItem(ParsePath(path)));

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
            this.treesorModelItems.VerifyAll();
            this.treesorModelItems.Verify(s => s.Exists(ParsePath(path)), Times.Never());
            this.treesorModelItems.Verify(s => s.Get(ParsePath(path)), Times.Once());

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

            this.treesorModelItems
                .Setup(s => s.Get(ParsePath(path)))
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
            this.treesorModelItems.VerifyAll();
            this.treesorModelItems.Verify(s => s.Exists(ParsePath(path)), Times.Never());
            this.treesorModelItems.Verify(s => s.Get(ParsePath(path)), Times.Once());

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

            this.treesorModelItems
                .Setup(s => s.Get(ParsePath(path)))
                .Returns(TreesorItem(ParsePath(path)));

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
            this.treesorModelItems.VerifyAll();

            Assert.False(this.powershell.HadErrors);
            Assert.IsType<PathInfo>(result.Last().BaseObject);
            Assert.Equal(CreatePath($@"custTree:\{path}"), CreatePath(((PathInfo)result.Last().BaseObject).Path));
        }

        #endregion Set-Location > IsItemContainer, GetItem > GetItem

        #region Move-Item > MoveItem

        [Fact]
        public void Powershell_moves_child_of_root_under_other_child()
        {
            // ARRANGE

            this.treesorModelItems
                .Setup(s => s.Exists(CreatePath("item1")))
                .Returns(true);

            // ACT

            this.powershell
                .AddStatement()
                .AddCommand("Move-Item").AddParameter("Path", @"custTree:\item1").AddParameter("Destination", @"custTree:\item2");

            var result = this.powershell.Invoke();

            // ASSERT

            this.treesorModel.VerifyAll();
            this.treesorModelItems.VerifyAll();
            this.treesorModelItems.VerifyAll();
            this.treesorModel.Verify(s => s.MoveItem(CreatePath("item1"), CreatePath("item2")), Times.Once());

            Assert.False(this.powershell.HadErrors);
        }

        #endregion Move-Item > MoveItem
    }
}