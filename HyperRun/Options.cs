using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperRun
{
    class Options
    {
        [Option(HelpText = "Print process output to console")]
        public bool Verbose { get; set; }

        [Option(HelpText = "Full path Logging file or use filename only, by default App rootpath will be used. Example: C:\\Logs")]
        public string LogFile { get; set; }

        [Option(HelpText = "Paralel execution based on this list, and use this variable as Output. Example: SDA JOG")]
        public IEnumerable<string> VariableIn { get; set; }

        [Option(Default="VariableInFile.txt", HelpText = "Filename with fullpath, Paralel execution based on this list, and use this variable as Output. Example: C:\\AreaCodeList.txt")]
        public string VariableInFile { get; set; }

        [Option(Required = true, HelpText = "List from VariableIn will be use as output for VariableOut. Example : $AreaCode")]
        public string VariableOut { get; set; }

        [Option(Required = true, HelpText = "Application Executable file with full path included. Example : Notepad.exe")]
        public string Command { get; set; }

        [Option(Required = false, HelpText = "Application Parameter. When executed, automatically replace text as define by [VariableOut] with list of [VariableIn]. Example : C:\\Temp\\$AreaCode.txt")]
        public string Argument { get; set; }

        [Option(Default = 3, HelpText = "Wait in hour after [Command] has been executed. before finally force close. Example : 3")]
        public int WaitForExit { get; set; }

    }
}
