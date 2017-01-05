using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

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

        [Test]
        public void Powershell_creates_property_at_root_node()
        {
            // ACT
            object value = "test";
            this.powershell
                .AddStatement()
                .AddCommand("Set-ItemProperty")
                .AddParameter("Path", @"treesor:\").AddParameter("Name","p").AddParameter("Value", value);

            var result = this.powershell.Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.SetPropertyValue(It.IsAny<TreesorNodePath>(),"p", value), Times.Once());
        }
    }
}
