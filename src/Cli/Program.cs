using PioneerConverter.Core.Application;
using PioneerConverter.Core.Common;
using PioneerConverter.Infrastructure.Thermo;

namespace PioneerConverter.Cli;

internal static class Program
{
    public static void Main(string[] args)
    {
        AssemblyResolver.Register();

        ConversionOptions options = ArgumentParser.ParseArguments(args);

        if (options.ShouldShowVersion)
        {
            Console.WriteLine($"{AppMetadata.AppName} {AppMetadata.Version}");
            return;
        }

        if (options.ShouldShowHelp)
        {
            HelpPrinter.ShowHelp();
            return;
        }

        if (string.IsNullOrEmpty(options.RawPath))
        {
            Console.WriteLine("Missing required RAW_PATH argument.");
            HelpPrinter.ShowHelp();
            return;
        }

        IReporter reporter = new ConsoleReporter();
        var runner = new ConversionRunner(
            new ThermoFileConverter(reporter),
            new ThermoOutputCompletenessChecker(),
            reporter);

        runner.Run(options, $"{AppMetadata.AppName} {AppMetadata.Version}");
    }
}
