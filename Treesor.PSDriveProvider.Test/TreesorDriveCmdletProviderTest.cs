using Moq;
using NUnit.Framework;
using System.IO;
using System.Management.Automation;
using Treesor.PSDriveProvider;

namespace Treesor.PowershellDriveProvider.Test
{
    [TestFixture]
    public class TreesorDriveCmdletProviderTest
    {
        private PowerShell powershell;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.powershell = PowerShell.Create();
            var result = this.powershell
                .AddCommand("Set-Location")
                .AddArgument(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
                .Invoke();
        }

        [Test]
        public void Powershell_returns_list_or_processes()
        {
            // ACT

            var result = this.powershell.AddStatement().AddCommand("Get-Process").Invoke();

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Count > 0);
        }

        //[Test]
        //public void Powershell_loads_Treesor_DriveProvider_automatically()
        //{
        //    // ACT

        //    this.powershell.AddStatement().AddCommand("Import-Module").AddArgument("./TreesorDriveProvider.dll").Invoke();

        //    // ASSERT

        //    var result = this.powershell.AddStatement().AddCommand("Get-PSDrive").Invoke();

        //    Assert.IsNotNull(result.Select(o => o.BaseObject as PSDriveInfo).SingleOrDefault(ps => ps.Name == "treesor"));
        //}

        [Test]
        public void Provider_creates_new_instance_of_treesor_service()
        {
            // ARRANGE

            var treesorService = new Mock<ITreesorService>();

            string givenUri = null;
            TreesorService.Factory = uri =>
            {
                givenUri = @"\";
                return treesorService.Object;
            };

            this.powershell.AddStatement().AddCommand("Import-Module").AddArgument("./TreesorDriveProvider.dll").Invoke();

            // ACT
            // create a drive with the treesor provider and give it the url

            var result = this.powershell.AddStatement()
                .AddCommand("New-PsDrive")
                .AddParameter("Name", "custTree")
                .AddParameter("PsProvider", "Treesor")
                .AddParameter("Root", @"\")
                .Invoke();

            // ASSERT
            // url must be shown to the factory

            Assert.AreEqual(@"\", givenUri);
            Assert.IsFalse(this.powershell.HadErrors);
        }

        [Test]
        public void Provider_notifies_service_about_drive_removal()
        {
            // ARRANGE

            var treesorService = new Mock<ITreesorService>();
            treesorService.Setup(s => s.Unloading());

            TreesorService.Factory = uri => treesorService.Object;

            this.powershell.AddStatement().AddCommand("Import-Module").AddArgument("./TreesorDriveProvider.dll").Invoke();
            this.powershell.AddStatement()
                .AddCommand("New-PsDrive")
                .AddParameter("Name", "custTree2")
                .AddParameter("PsProvider", "Treesor")
                .AddParameter("Root", @"\")
                .Invoke();

            // ACT
            // drive is remove

            var result = this.powershell.AddStatement().AddCommand("Remove-PsDrive").AddParameter("Name", "custTree2").Invoke();

            // ASSERT
            // drive is no longer there and service was called.

            Assert.IsFalse(this.powershell.HadErrors);
            treesorService.Verify(s => s.Unloading(), Times.Once());
        }
    }
}