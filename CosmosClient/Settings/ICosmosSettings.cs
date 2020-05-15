namespace CosmosDbClient.Settings
{
    /// <summary>
    /// This interface is the basic configuration interface.
    /// It contains cosmosdb connection string, database name and container name
    /// </summary>
    public interface ICosmosSettings
    {
        /// <summary>
        /// This is the connection string to your cosmos db instance
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// This is the database instance that you want to bind
        /// </summary>
        public string DatabaseName { get; set; }
        /// <summary>
        /// This is the container in your database that you want to bind
        /// </summary>
        public string ContainerName { get; set; }
    }
}
