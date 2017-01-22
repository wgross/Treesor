using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Treesor.PSDriveProvider.Commands
{
    public class TreesorDriveNameCompleter : IArgumentCompleter
    {
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            switch (commandName.ToLowerInvariant())
            {
                case "new-treesorcolumn":
                default:
                    return Enumerable.Empty<CompletionResult>();
            }
        }
    }
}