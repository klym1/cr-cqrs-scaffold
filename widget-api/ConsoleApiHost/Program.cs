﻿using System;
using System.Configuration;
using System.Linq;
using Logary.Metrics;
using Nancy.Hosting.Self;
using Topshelf;

namespace ConsoleApiHost
{
    class Program
    {
        //TODO: Customize this for correct application name
        static void Main(string[] args)
        {
            var logPath = ConfigurationManager.AppSettings["logFile"];
            var errorLogPath = ConfigurationManager.AppSettings["errorLogFile"];
            var logHost = ConfigurationManager.AppSettings["logHost"];
            var logPort = ushort.Parse(ConfigurationManager.AppSettings["logPort"]);

            var logManager = Logging.ConfigureLogary(
                "Widgets API", logPath, errorLogPath, logHost, logPort);

            HostFactory.Run(
                x =>
                {
                    x.Service<ApiHost>(
                        s =>
                        {
                            s.ConstructUsing(name => new ApiHost());
                            s.WhenStarted(tc => tc.Start());
                            s.WhenStopped(tc => tc.Stop());
                        });
                    x.RunAsLocalService();
                    x.SetDescription("Widgets API Host");
                    x.SetDisplayName("WidgetsAPIHost");
                    x.SetServiceName("WidgetsAPIHost");
                    x.UseLogary(logManager);
                    x.StartAutomaticallyDelayed();
                });

            Console.ReadLine();
        }
    }

    class ApiHost
    {
        private NancyHost _nancyHost;

        public void Start()
        {
            var hostUrls = ConfigurationManager.AppSettings["hostUrls"];
            var urlList = hostUrls.Split(',').Select(f => new Uri(f)).ToArray();
            _nancyHost = new NancyHost(urlList);
            _nancyHost.Start();
        }

        public void Stop()
        {
            _nancyHost.Stop();
        }
    }
}
