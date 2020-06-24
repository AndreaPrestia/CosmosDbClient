Welcome to the CosmosDbClient wiki!

I did this lightweight Microsoft.Azure.Cosmos wrapper to simplify access and data management on Azure CosmosDB.

The concept behind this project is to have a single generic repository usable for any type of object on my database.

I did all as simple as possible.

Here i show you some examples :)

Initialize your repository
First of all create a class that implements the interface ICosmosSettings.

public class FeedRssDatabaseSettings : ICosmosSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string ContainerName { get; set; }
}
This class is used to bind, in the appsettings.json (or local.settings.json for Azure Functions) an object named FeedRssDatabaseSettings that contains CosmosDb connection string, database name and container to bind.

Here an example of json part of FeedRssDatabaseSettings.

"FeedRssDatabaseSettings": { "ContainerName": "feedrss", "ConnectionString": "XXX", "DatabaseName": "myDatabase" }

After that you've created the configuration class you can initialize an instance of a CosmosRepository class. I usually ad it as singleton in Startup.cs like this:

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton((s) =>
        {
            return new CosmosRepository<FeedRssModel>(CosmosClientConfiguration.GetAzureConfiguration<FeedRssDatabaseSettings>());
        });
    }
The example above is from an Azure Functions project. CosmosClientConfiguration.GetAzureConfiguration() => get the specific configuration. CosmosRepository => Initialize a CosmosRepository for a specific object. The object must be derived from the CosmosEntity object.

Usage of CosmosRepository
Once that you have initialized all the use is pretty simple. Just call the methods on the instance. If you added your repo as singleton just inject it where you need and call your private injected instance :)

Here an example with Azure Functions:

public class LatestNews {

private CosmosRepository _repository;

public LatestNews(CosmosRepository repository) { _repository = repository; }

 [FunctionName("LatestNews")]
 public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/news")] HttpRequest req,
        ILogger log)
 {
     try
     {
         List<FeedRssModel> news = await _repository.Query();

         return new OkObjectResult(news);
     }
     catch (Exception ex)
     {
        log.LogError(ex, ex.Message, null);
        return new BadRequestObjectResult(ex.Message);
     }
 }

public class FeedRssModel : CosmosEntity
{
    public FeedRssModel()
    {
        FeedRssItems = new List<FeedRssItemModel>();
    }

    #region CustomProperties
    [JsonProperty("link")]
    public string Link { get; set; }
    [JsonProperty("process")]
    public bool Process { get; set; }
    [JsonProperty("items")]
    public List<FeedRssItemModel> FeedRssItems { get; set; }
    #endregion
}

public class FeedRssItemModel
{
    public FeedRssItemModel()
    {

    }

    [JsonProperty("link")]
    public string Link { get; set; }
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("text")]
    public string Text { get; set; }
    [JsonProperty("created")]
    public long Created { get; set; }
    [JsonProperty("deleted")]
    public long Deleted { get; set; }
}
}

The example above shows the retrieve of the list of news from feedrss container. It's pretty straightforward.

Hope it helps! :)
