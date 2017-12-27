using Moq;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Treesor.PSDriveProvider;
using Xunit;
using static Treesor.PSDriveProvider.Test.TestDataGenerators;

namespace Treesor.PowershellDriveProvider.Test
{
    public class TreesorDriveCmdletProviderTest : IDisposable
    {
        private readonly PowerShell powershell;
        private readonly Mock<ITreesorModel> treesorService;

        public TreesorDriveCmdletProviderTest()
        {
            this.treesorService = new Mock<ITreesorModel>();

            TreesorDriveInfo.TreesorModelFactory = _ => treesorService.Object;

            this.powershell = ShellInModuleDirectory();
        }

        public void Dispose()
        {
            this.powershell.Stop();
            this.powershell.Dispose();
        }

        [Fact]
        public void Powershell_returns_list_or_processes()
        {
            // ACT

            var result = this.powershell.AddStatement().AddCommand("Get-Process").Invoke();

            // ASSERT

            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public void Powershell_loads_Treesor_DrivePowershell_automatically()
        {
            // ACT
            // import the module

            this.powershell
                .AddStatement()
                    .AddCommand("Import-Module")
                        .AddArgument("./TreesorDriveProvider.dll")
                        .Invoke();

            // ACT
            // retrieve the drive infos

            this.powershell
                    .AddStatement()
                        .AddCommand("Get-PSDrive");

            var result = this.powershell.Invoke();

            // ASSERT
            // the result set contains the default drive

            var defaultDrive = result.Select(o => o.BaseObject as PSDriveInfo).SingleOrDefault(ps => ps.Name == "treesor");

            Assert.NotNull(defaultDrive);
            Assert.Equal("\\", defaultDrive.Root);
            Assert.Equal("treesor", defaultDrive.Name);
        }

        [Fact]
        public void Powershell_creates_new_instance_of_treesor_service()
        {
            // ARRANGE
            // import teh module

            this.powershell
                .AddStatement()
                    .AddCommand("Import-Module")
                        .AddArgument("./TreesorDriveProvider.dll")
                        .Invoke();

            // ACT
            // create a drive with the treesor provider and give it the url

            this.powershell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "custTree")
                        .AddParameter("PsProvider", "Treesor")
                        .AddParameter("Root", @"\");

            var result = this.powershell.Invoke();

            // ASSERT
            // url must be shown to the factory

            Assert.False(this.powershell.HadErrors);
        }

        [Fact]
        public void Powershell_notifies_service_about_drive_removal()
        {
            // ARRANGE
            // import the module and create a drive

            this.powershell
                .AddStatement()
                    .AddCommand("Import-Module")
                        .AddArgument("./TreesorDriveProvider.dll")
                        .Invoke();

            this.powershell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "custTree")
                        .AddParameter("PsProvider", "Treesor")
                        .AddParameter("Root", @"\");

            // ACT
            // remove the drive

            this.powershell
                .AddStatement()
                .AddCommand("Remove-PsDrive").AddParameter("Name", "custTree");

            var result = this.powershell.Invoke();

            // ASSERT
            // drive is no longer there and service was called.

            Assert.False(this.powershell.HadErrors);
            treesorService.Verify(s => s.Dispose(), Times.Once());
        }

        [Fact]
        public void Powershell_returns_drive_info()
        {
            // ARRANGE
            // import the module and create the drive

            this.powershell
                .AddStatement()
                    .AddCommand("Import-Module")
                        .AddArgument("./TreesorDriveProvider.dll")
                        .Invoke();

            this.powershell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "custTree")
                        .AddParameter("PsProvider", "Treesor")
                        .AddParameter("Root", @"\");

            // ACT
            // get drive info from powershell

            this.powershell
                .AddStatement()
                    .AddCommand("Get-PsDrive")
                        .AddParameter("Name", "custTree");

            var result = this.powershell.Invoke().LastOrDefault();

            Assert.NotNull(result);
            Assert.IsType<TreesorDriveInfo>(result.BaseObject);

            var treesorDriveInfo = result.BaseObject as TreesorDriveInfo;

            Assert.Equal("\\", treesorDriveInfo.Root);
            Assert.Equal("custTree", treesorDriveInfo.Name);
        }
    }
}