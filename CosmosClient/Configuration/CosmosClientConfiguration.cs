using CosmosDbClient.Interfaces.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CosmosDbClient.Configuration
{
    public static class CosmosClientConfiguration 
    {
        /// <summary>
        /// Get the configuration from local.settings.json (ex. Azure functions environment)
        /// </summary>
        /// <returns></returns>
        public static T GetAzureConfiguration<T>() where T : class, ICosmosSettings, new()
        {
            string key = typeof(T).Name;

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            T instance = new T();

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("local.settings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

            var configuration = builder.Build();

            configuration.Bind(key, instance);

            return instance;
        }

        public static T GetConfiguration<T>() where T : class, ICosmosSettings, new()
        {
            string key = typeof(T).Name;

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            T instance = new T();

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

            var configuration = builder.Build();

            configuration.Bind(key, instance);

            return instance;
        }

        /// <summary>
        /// Get configuration from specified json settings file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static T GetConfiguration<T>(string filename) where T : class, ICosmosSettings, new()
        {
            string key = typeof(T).Name;

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            T instance = new T();

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(filename, optional: true, reloadOnChange: true).AddEnvironmentVariables();

            var configuration = builder.Build();

            configuration.Bind(key, instance);

            return instance;
        }

    }
}
