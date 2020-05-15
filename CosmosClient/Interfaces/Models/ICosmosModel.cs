
using Newtonsoft.Json;

namespace CosmosDbClient.Interfaces.Models
{
    /// <summary>
    /// This is the base cosmos db model. It containts the common parameters for cosmos db items.
    /// </summary>
    public interface ICosmosModel
    {
        /// <summary>
        /// Document identifier 
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
