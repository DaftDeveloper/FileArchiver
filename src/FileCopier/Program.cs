using DaftDev.FileCopier.Enums;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DaftDev.FileCopier
{
    class Program
    {
        internal static IConfiguration Configuration { get; set; }

        static void Main(string[] args)
        {
            BuildConfiguration();

            var app = CreateApp();
            app.Execute(args);
        }

        static private void BuildConfiguration()
        {
            Console.WriteLine("Building configuration");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        static private CommandLineApplication CreateApp()
        {
            Console.WriteLine("Configuring argument support.");

            var app = new CommandLineApplication();
            app.HelpOption("-h|--help");

            var inputDirectory = app.Option("-i|--input <DIRECTORY>", "Input Directory", CommandOptionType.SingleValue)
                .IsRequired()
                .Accepts(x => x.ExistingDirectory());

            var outputDirectory = app.Option("-o|--output <DIRECTORY>", "Output Directory", CommandOptionType.SingleValue)
                .IsRequired()
                .Accepts(x => x.ExistingDirectory());

            var parallelArgument = app.Option("-p|--parallel", "Process files in parallel.", CommandOptionType.NoValue);

            var allowedExtensions = CreateAllowedExtensionsList();
            var timestampSource = Enum.Parse<TimestampSource>(Configuration["TimestampSource"]);

            app.OnExecute(() =>
            {
                Console.WriteLine("Starting file copying service.");

                var fileService = new FileCopyService(inputDirectory.Value(), outputDirectory.Value(), allowedExtensions, parallelArgument.HasValue(), timestampSource);
                fileService.ProcessMoves();

                Console.WriteLine("Processing complete, press any key to continue...");
                Console.ReadKey();
            });

            return app;
        }

        static private IEnumerable<string> CreateAllowedExtensionsList()
        {
            var allowedExtensions = Configuration.GetSection("AllowedExtensions")
                .Get<string[]>();

            if (allowedExtensions == null || !allowedExtensions.Any())
            {
                Console.WriteLine("Extension list not provided in config, all files will be copied.");
                allowedExtensions = null;
            }

            Console.WriteLine($"The following extensions will be processed: {String.Join(", ", allowedExtensions)}");

            return allowedExtensions;
        }
    }
}
