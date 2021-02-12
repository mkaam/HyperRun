﻿using CommandLine;
using NLog;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperRun
{
    class Program
    {
        private static Logger logger;
        private static Stopwatch _watch;
        private static string ExePath;
        private static string RootPath;
        private static string LogPath;
        private static bool ParserError = false;

        static void Main(string[] args)
        {
            string[] arg = args.AsEnumerable().Select(x => x.Contains("--") ? x: (x.Contains("-") ? ($"\"{x}\""): x) ).ToArray();

            ExePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            RootPath = ExePath;
            LogPath = Path.Combine(RootPath, "Logs");

            logger = new Logger("log");
            var parser = new Parser(config =>
            {
                config.IgnoreUnknownArguments = false;
                config.CaseSensitive = false;
                config.AutoHelp = true;
                config.AutoVersion = true;
                config.HelpWriter = Console.Error;
            });

            var result = parser.ParseArguments<Options>(arg)
                .WithParsed<Options>(s => RunOptions(s))
                .WithNotParsed(errors => HandleParseError(errors));
   
            if (!ParserError)
            {
                _watch.Stop();
                logger.Debug($"Application Finished. Elapsed time: {_watch.ElapsedMilliseconds}ms");
            }

            //Console.ReadLine();
        }

        static void PathConfigure(Options opts)
        {
            // Logs
            if (opts.LogFile != null && Path.GetFileName(opts.LogFile) == opts.LogFile)
            {
                if (!Directory.Exists(LogPath))                
                    Directory.CreateDirectory(LogPath);
                
                opts.LogFile = $"{Path.Combine(LogPath, opts.LogFile)}";
            }

            //
            if (opts.VariableInFile != null && Path.GetFileName(opts.VariableInFile) == opts.VariableInFile)
            {             
                opts.VariableInFile = $"{Path.Combine(RootPath, opts.VariableInFile)}";
            }

        }

        static void LoggerConfigure(Options opts)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile");
            if (opts.LogFile != null)
            {
                if (Path.GetFileName(opts.LogFile) == opts.LogFile)
                    logfile.FileName = $"{Path.Combine(Path.Combine(RootPath, "Logs"), opts.LogFile)}";
                else
                    logfile.FileName = $"{opts.LogFile}";
            }
            else
                logfile.FileName = $"{Path.Combine(Path.Combine(RootPath, "Logs"), $"{DateTime.Now.ToString("yyyyMMdd")}.csv")}";

            logfile.MaxArchiveFiles = 60;
            logfile.ArchiveAboveSize = 10240000;

            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            if (opts.Verbose)
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            else
                config.AddRule(LogLevel.Error, LogLevel.Fatal, logconsole);

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            // design layout for file log rotation
            CsvLayout layout = new CsvLayout();
            layout.Delimiter = CsvColumnDelimiterMode.Comma;
            layout.Quoting = CsvQuotingMode.Auto;
            layout.Columns.Add(new CsvColumn("Start Time", "${longdate}"));
            layout.Columns.Add(new CsvColumn("Elapsed Time", "${elapsed-time}"));
            layout.Columns.Add(new CsvColumn("Machine Name", "${machinename}"));
            layout.Columns.Add(new CsvColumn("Login", "${windows-identity}"));
            layout.Columns.Add(new CsvColumn("Level", "${uppercase:${level}}"));
            layout.Columns.Add(new CsvColumn("Message", "${message}"));
            layout.Columns.Add(new CsvColumn("Exception", "${exception:format=toString}"));
            logfile.Layout = layout;

            // design layout for console log rotation
            SimpleLayout ConsoleLayout = new SimpleLayout("${longdate}:${message}\n${exception}");
            logconsole.Layout = ConsoleLayout;

            // Apply config           
            NLog.LogManager.Configuration = config;
        }

        static void RunOptions(Options opts)
        {            
            PathConfigure(opts);
            LoggerConfigure(opts);

            // remove " char at first and last
            opts.Argument = opts.Argument.Substring(0, 1) == "\"" ? opts.Argument.Remove(0, 1): opts.Argument;
            opts.Argument = opts.Argument.Substring(opts.Argument.Length-1, 1) == "\"" ? opts.Argument.Remove(opts.Argument.Length - 1, 1) : opts.Argument;
            opts.Argument = opts.Argument.Replace("\"\"","\"");

            _watch = new Stopwatch();
            _watch.Start();
            logger.Debug("Application Start");

            if (opts.VariableIn.Count() <= 0)
            {
                opts.VariableIn = File.ReadLines(opts.VariableInFile, Encoding.Default);
            }

            Parallel.ForEach(opts.VariableIn, (VariableIn) =>
            {
                string argument = "";
                string cmd = "";

                if (opts.Argument != null)
                {
                    argument = opts.Argument.Replace(opts.VariableOut, VariableIn);
                    cmd = $"{opts.Command} {argument}";
                    logger.Debug($"Executing... : {cmd}");

                    LaunchCommandLineApp(opts.Command, opts.WaitForExit, argument);
                }
                else
                {
                    cmd = $"{opts.Command}";
                    logger.Debug($"Executing... : {cmd}");

                    LaunchCommandLineApp(opts.Command, opts.WaitForExit);
                }

                
                logger.Debug($"Done : {cmd}");
            });
                        
        }
        
        static void HandleParseError(IEnumerable<Error> errs)
        {
            ParserError = true;

            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
            {
            }
            else
                Console.WriteLine("Parameter unknown, please check the documentation or use '--help' for more information");


        }

        static void LaunchCommandLineApp(string ExeFile, int WaitForExit=3, string Argument = "")
        {   
            try
            {
                // Use ProcessStartInfo class
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = ExeFile;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (Argument != "")
                {
                    startInfo.Arguments = Argument;
                }


                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit(Convert.ToInt32(new TimeSpan(WaitForExit, 0, 0).TotalMilliseconds)); // exit after 5 hour processing
                    if (!exeProcess.HasExited)
                    {
                        exeProcess.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"LaunchCommandLineApp {ExeFile} {WaitForExit} {Argument}", ex);
            }
                   
        }
  
    }
}