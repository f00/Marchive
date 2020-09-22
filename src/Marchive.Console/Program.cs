using System;
using Marchive.App;
using Marchive.App.Exceptions;
using Marchive.App.Settings;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marchive.Console
{
    public class Program
    {
        private const string AppName = "Marchive archiver";
        private const string AppDescription =
            "Marchive is a command line utility that lets you easily archive multiple files into a single one, in a jiffy.";
        private const string DefaultArchiveFileName = "archive";
        private static IMarchive _marchive;

        public static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(configure =>
                        {
                            configure.AddConsole(cfg => cfg.LogToStandardErrorThreshold = LogLevel.Information);
                            configure.AddDebug();
                        });
                    services.AddMarchive(configure =>
                    {
                        configure.EncryptionAlgorithm = EncryptionAlgorithm.Aes;
                        configure.FileNameEncoding = System.Console.InputEncoding;
                    });

                });
            var host = builder.Build();
            _marchive = host.Services.GetService<IMarchive>();

            var app = new CommandLineApplication(false)
            {
                FullName = AppName,
                Name = "Marchive.Console",
                Description = AppDescription
            };
            app.HelpOption("-? | -h | --help");

            ConfigureArchiveCommand(app);

            ConfigureUnArchiveCommand(app);

            try
            {
                app.Execute(args);
            }
            catch (InvalidEncryptionKeyException)
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("Invalid password provided (or archive is not password protected).");
                System.Console.ResetColor();
            }
            catch (Exception e)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("ERROR: " + e.Message);
                System.Console.ResetColor();
            }
        }

        private static void ConfigureArchiveCommand(CommandLineApplication app)
        {
            app.Command("archive",
                archive =>
                {
                    archive.Description = "Archive one or more files into one single file";
                    var filesToArchive = archive.Argument(
                        "files",
                        "The names of the files to be archived",
                        multipleValues: true);
                    var archiveFileName = archive.Option(
                        "-n | --name",
                        "The name of the archive file (default is 'archive').",
                        CommandOptionType.SingleValue);
                    var encryptionKey = archive.Option(
                        "-p | --password",
                        "The secret password to use for encrypting the archive. Note that passwords are case sensitive.",
                        CommandOptionType.SingleValue);
                    archive.HelpOption("-? | -h | --help");
                    archive.OnExecute(() =>
                    {
                        _marchive.Archive(filesToArchive.Values,
                            archiveFileName.HasValue() ? archiveFileName.Value() : DefaultArchiveFileName,
                            encryptionKey.Value());

                        return 0;
                    });
                });
        }

        private static void ConfigureUnArchiveCommand(CommandLineApplication app)
        {
            app.Command("un-archive",
                unArchive =>
                {
                    unArchive.Description = "Un archive / extract a previously created archive file";
                    var archiveFileName = unArchive.Argument(
                        "archive",
                        "The name of the archived file",
                        multipleValues: false);
                    var archiveDirectory = unArchive.Option(
                        "-d | --directory",
                        "The directory in which the extracted files will be placed (defaults to current).",
                        CommandOptionType.SingleValue);
                    var decryptionKey = unArchive.Option(
                        "-p | --password",
                        "The secret password to use for decrypting the archive. Note that passwords are case sensitive.",
                        CommandOptionType.SingleValue);
                    unArchive.HelpOption("-? | -h | --help");
                    unArchive.OnExecute(() =>
                    {
                        _marchive.UnArchive(archiveFileName.Value, archiveDirectory?.Value(), decryptionKey.Value());

                        return 0;
                    });
                });
        }
    }
}
