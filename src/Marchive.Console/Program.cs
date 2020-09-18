using System;
using System.IO;
using Marchive.App;
using Marchive.App.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace Marchive.Console
{
    class Program
    {
        // TODO Move to args
        private static string[] _inputFilesToArchive = { "input1.txt", "input2.txt", "input3.txt" };
        private static string _outputArchiveFileName = Path.Combine("Archive", "archive");
        private static string _inputArchiveFileName = Path.Combine("Archive", "archive");
        private static string _outputUnArchiveDirectory = "UnArchive";
        private const string DefaultArchiveFileName = "archive";
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Description =
                "Marchive is a command line utility that lets you easily archive multiple files into a single one in a jiffy.";

            app.Command("archive",
                archive =>
                {
                    var filesToArchive = archive.Argument(
                        "files",
                        "The names of the files to be archived",
                        multipleValues: true);
                    var archiveFileName = archive.Option(
                        "-n | --name",
                        "The name of the archive file (default is 'archive').",
                        CommandOptionType.SingleValue);
                    archive.HelpOption("-? | -h | --help");
                    archive.OnExecute(() =>
                    {
                        using var archiver = new Archiver(new FileSystemProxy());
                        archiver.Archive(filesToArchive.Values, archiveFileName.HasValue() ? archiveFileName.Value() : DefaultArchiveFileName);

                        return 0;
                    });
                });

            app.Command("un-archive",
                unArchive =>
                {
                    var archiveFileName = unArchive.Argument(
                        "archive",
                        "The name of the archived file",
                        multipleValues: false);
                    var archiveDirectory = unArchive.Option(
                        "-d | --directory",
                        "The directory in which the extracted files will be placed (defaults to current).",
                        CommandOptionType.SingleValue);
                    unArchive.HelpOption("-? | -h | --help");
                    unArchive.OnExecute(() =>
                    {
                        var unArchiver = new UnArchiver(new FileSystemProxy());
                        unArchiver.UnArchive(archiveFileName.Value, archiveDirectory?.Value());

                        return 0;
                    });
                });

            app.HelpOption("-? | -h | --help");

            try
            {
                app.Execute(args);
            }
            catch (Exception e)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("ERROR: " + e.Message);
                System.Console.ResetColor();
            }
        }
    }
}
