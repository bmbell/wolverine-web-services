using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Bell.Common.Models.Configuration;
using Bell.Common.WebApi.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wolverine.WebApi.DryIoc;

namespace Wolverine.WebApi
{
    /// <summary>
    /// The entry point into the application
    /// </summary>
    public class Startup
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the start-up
        /// </summary>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            var configurationRoot = builder.Build();

            Configuration = BuildApplicationConfiguration(env, configurationRoot);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The application's configuration
        /// </summary>
        public static ApplicationConfiguration Configuration { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method gets called by the runtime
        /// </summary>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return WebApiConfiguration.ConfigureServices(services, Configuration, new ApplicationModule());
        }

        /// <summary>
        /// This method gets called by the runtime
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Configure the application...
            WebApiConfiguration.ConfigureApplication(app, env, loggerFactory, Configuration);
        }

        #endregion

        #region Private Methods

        private static ApplicationConfiguration BuildApplicationConfiguration(IHostingEnvironment hostingEnvironment, IConfigurationRoot configurationRoot)
        {
            var basePath = AppContext.BaseDirectory;
            var xmlDocumentFilePaths = new List<string>
            {
                Path.Combine(basePath, "Wolverine.Models.xml"),
                Path.Combine(basePath, "Wolverine.WebApi.xml")
            };

            var connectionStrings =
                WebApiConfiguration.FindConnectionStrings(configurationRoot);

            var configuration = new ApplicationConfiguration
            {
                ApiName = "Graph Services",
                ApiDescription = "©2019 Brandon Bell. All rights reserved.",
                ApiVersions = new List<int> { 1, 2 },
                ApiXmlDocumentFilePaths = xmlDocumentFilePaths,
                ApplicationName = "Wolverine",
                ApplicationToken = configurationRoot.GetSection("ApplicationToken").Value,
                ConnectionStrings = connectionStrings,
                CoreWebServicesBaseUrl = connectionStrings["CoreWebServicesBaseUrl"],
                EnableSwagger = true,
                Environment = hostingEnvironment.EnvironmentName,
                ExceptionDebuggingHook = ExceptionDebuggingHook,
                UniversalApplicationId = configurationRoot.GetSection("UniversalApplicationId").Value
            };

            return configuration;
        }

        private static void ExceptionDebuggingHook(Exception exception)
        {
            // Put a breakpoint here to debug exceptions...
            Debug.Write(exception);
        }

        #endregion
    }
}
