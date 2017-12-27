using System;
using System.IO;
using System.Management.Automation;
using Treesor.Model;

namespace Treesor.PSDriveProvider.Test
{
    public static class TestDataGenerators
    {
        #region Shells

        public static PowerShell ShellInModuleDirectory()
        {
            var powershell = PowerShell.Create(RunspaceMode.NewRunspace);
            powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                        .AddArgument(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
                        .Invoke();

            return powershell;
        }

        public static PowerShell ShellWithDriveCreated()
        {
            var powershell = ShellInModuleDirectory();

            powershell
                .AddStatement()
                    .AddCommand("Set-Location")
                    .AddArgument(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

            powershell
                .AddStatement()
                    .AddCommand("Import-Module").AddArgument("./TreesorDriveProvider.dll");

            powershell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "custTree")
                        .AddParameter("PsProvider", "Treesor")
                        .AddParameter("Root", @"\");

            powershell.Invoke();
            powershell.Commands.Clear();

            return powershell;
        }

        #endregion Shells

        #region TreesorItems

        public static TreesorItem TreesorItem(string name, Guid? id = null, Action<TreesorItem> setup = null) => TreesorItem(TreesorNodePath.Create(name), id, setup);

        public static TreesorItem TreesorItem(TreesorNodePath path, Guid? id = null, Action<TreesorItem> setup = null)
        {
            var tmp = new TreesorItem(path, new Reference<Guid>(id ?? Guid.NewGuid()));
            setup?.Invoke(tmp);
            return tmp;
        }

        #endregion TreesorItems
    }
}