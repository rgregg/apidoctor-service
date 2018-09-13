using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace service_runner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string portVariable = System.Environment.GetEnvironmentVariable("PORT");
            int port;
            if (!Int32.TryParse(portVariable, out port)) 
            {
                port = 8080;
            }
            CreateWebHostBuilder(args, port).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, int httpPort) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options => {
                    options.ListenAnyIP(httpPort);   // HTTP port
                })
                .UseStartup<Startup>();
    }
}
