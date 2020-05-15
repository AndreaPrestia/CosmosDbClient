using Newtonsoft.Json;

namespace CosmosDbClient.Entities
{
    /// <summary>
    /// This is the base cosmos db model. It containts the common parameters for cosmos db items.
    /// </summary>
    public class CosmosEntity
    {
        /// <summary>
        /// Document identifier 
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
