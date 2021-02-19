using CommandLine;
using CommandLine.Text;
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

        [Option(SetName = "VariableIn", HelpText = "Paralel execution based on this list, and use this variable as Output. Example: SDA JOG")]
        public IEnumerable<string> VariableIn { get; set; }

        [Option(SetName = "VariableInFile", Default= "VariableInFile.txt", HelpText = "Filename with fullpath, Paralel execution based on this list, and use this variable as Output. Example: C:\\AreaCodeList.txt")]
        public string VariableInFile { get; set; }

        [Option(SetName = "VariableInQuery", HelpText = "Query Filename with fullpath, Parallel execution based on query result, and use column as VariableInx. Example: C:\\VariableInQuery.sql")]
        public string VariableInQuery { get; set; }

        [Option(HelpText = "Required if VariableInQuery is set. Servermame where VariableInQuery will be executed.")]
        public string ServerName { get; set; }

        [Option(HelpText = "Required if VariableInQuery is set. DBName where VariableInQuery will be executed.")]
        public string DBName { get; set; }

        [Option(Required = true, HelpText = "List from VariableIn will be store as VariableOut. Example : $AreaCode")]
        public IEnumerable<string> VariableOut { get; set; }

        [Option(Required = true, HelpText = "Application Executable file with full path included. Example : Notepad.exe")]
        public string Command { get; set; }

        [Option(Required = false, HelpText = "Application Parameter. When executed, automatically replace text as define by [VariableOut] with list of [VariableIn]. \n Note: don't use double dash character, if your Application argument contain double quote please use [\\\"] instead of [\"] \n Example : C:\\Temp\\$AreaCode.txt")]
        public string Argument { get; set; }

        [Option(Default = 3, HelpText = "Wait in hour after [Command] has been executed. before finally force close. Example : 3")]
        public int WaitForExit { get; set; }

        [Usage(ApplicationAlias = "HyperRun.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Define VariableIn Manually", new Options { VariableIn = new string[] { "AMI", "JOG" }, VariableOut = new string[] { "{AreaCode}" }, Command = "Notepad.exe", Argument = "{AreaCode}.txt" });
                yield return new Example("Define VariableIn by Query", new Options { VariableInQuery = @"E:\QueryVariableIn.sql", ServerName = @"PMIIDSUBDEV42\DEV2012", DBName = "Users", VariableOut = new string[] { "{DBName} {AreaCode}" }, Command = "Notepad.exe", Argument = "{DBName}_{AreaCode}.txt" });
                //yield return new Example("Define VariableIn by File", UnParserSettings.WithGroupSwitchesOnly(), new HeadOptions { FileName = "file.bin", Quiet = true });
                //yield return new Example("read more lines", new[] { UnParserSettings.WithGroupSwitchesOnly(), UnParserSettings.WithUseEqualTokenOnly() }, new Options { FileName = "file.bin", Lines = 10 });
            }
        }

    }
}
