namespace CosmosDbClient.Interfaces.Configuration
{
    interface ICosmosConfiguration<T> where T : class, ICosmosSettings, new()
    {
        T GetConfiguration();

        T GetAzureConfiguration();

        T GetConfiguration(string filename);
    }
}
