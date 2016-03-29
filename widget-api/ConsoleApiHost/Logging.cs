using System.IO;
using Logary;
using Logary.Configuration;
using Logary.Targets;
using Console = System.Console;

namespace ConsoleApiHost
{
    public class Logging
    {
        public static LogManager ConfigureLogary(string serviceName, string logPath, string errorLogPath, string logStashHostName, ushort logStashPort)
        {
            var stream = File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            var writer = new StreamWriter(stream);

            var errorStream = File.Open(errorLogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            var errorWriter = new StreamWriter(errorStream);

#if DEBUG
            var x = LogaryFactory.New(serviceName, with =>
                with
                .Target<Logary.Targets.TextWriter.Builder>("LogFile",
                    conf => conf.Target.WriteTo(writer, errorWriter))

                .Target<Logary.Targets.TextWriter.Builder>("Console",
                    conf => conf.Target.WriteTo(Console.Out, Console.Error)));

#else
            var x = LogaryFactory.New(serviceName, with =>
                with
                .Target<Logary.Targets.TextWriter.Builder>("LogFile",
                    conf => conf.Target.WriteTo(writer, errorWriter))

                .Target<Logary.Targets.TextWriter.Builder>("Console",
                    conf => conf.Target.WriteTo(Console.Out, Console.Error))

                .Target<Logstash.Builder>("LogStash",
                    conf => conf.Target
                                .Hostname(logStashHostName)
                                .Port(logStashPort)
                                .EventVersion(Logstash.EventVersion.One)
                                .Done()));
#endif

            return x;
        }

    }
}
