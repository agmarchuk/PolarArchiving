﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Turgunda7
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(true)
                .UseSetting("detailedErrors", "true")
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:52018")
                .UseKestrel(options => { options.Limits.MaxRequestBodySize = null; })
                .Build();
    }
}
