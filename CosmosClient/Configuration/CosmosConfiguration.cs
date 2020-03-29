using CosmosDbClient.Interfaces.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CosmosDbClient.Configuration
{
    /// <summary>
    /// Use to initialize configuration of a databasesettings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CosmosConfiguration<T> : ICosmosConfiguration<T> where T : class, ICosmosSettings, new()
    {
        public CosmosConfiguration()
        {

        }

        /// <summary>
        /// Get the configuration from local.settings.json (ex. Azure functions environment)
        /// </summary>
        /// <returns></returns>
        public T GetAzureConfiguration()
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

        /// <summary>
        /// Get the configuration from appsettings.json
        /// </summary>
        /// <returns></returns>
        public T GetConfiguration()
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
        public T GetConfiguration(string filename)
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
