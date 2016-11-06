using Moq;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDriveItemCmdletProviderTest
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
        
        [Test]
        public void Provider_retrieves_always_existing_root_item()
        {
            // ACT
            // test for a missing item

            this.powershell
                .AddStatement()
                .AddCommand("Get-Item").AddParameter("Path", @"custTree:\").Invoke();

            var result = this.powershell.Invoke();

            // ASSERT
            // item wasn't found and service was ask for the path

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsTrue((bool)result.Last().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once);
        }

        [Test]
        public void Provider_asks_service_for_existsnce_of_item()
        {
            // ACT
            // test for a missing item

            var result = this.powershell.AddStatement().AddCommand("Test-Path").AddParameter("Path", @"custTree:\item").Invoke();

            // ASSERT
            // item wasn't found and service was ask for the path

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.IsFalse((bool)result.First().BaseObject);
            this.treesorService.Verify(s => s.ItemExists(TreesorNodePath.Create("item")), Times.Once);
        }
    }
}