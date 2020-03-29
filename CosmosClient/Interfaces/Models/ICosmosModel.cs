using System;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// Timestamp of last edit of a document in cosmos db
        /// </summary>
        [JsonPropertyName("_ts")]
        public int Timestamp { get; set; }
    }
}
