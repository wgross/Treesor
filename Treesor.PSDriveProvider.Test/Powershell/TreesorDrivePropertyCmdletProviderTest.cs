using Moq;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Treesor.PSDriveProvider.Test
{
    public class TreesorDrivePropertyCmdletProviderTest
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
    }
}