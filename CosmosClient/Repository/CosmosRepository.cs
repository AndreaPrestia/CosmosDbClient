using CosmosDbClient.Interfaces.Configuration;
using CosmosDbClient.Interfaces.Models;
using CosmosDbClient.Interfaces.Repository;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDbClient.Repository
{
    /// <summary>
    /// Implementation of ICosmosRepository<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CosmosRepository<T> : ICosmosRepository<T> where T : class, ICosmosModel, new()
    {
        private readonly CosmosClient _client;
        private readonly Database _database;
        private readonly Container _container;

        public CosmosRepository(ICosmosSettings cosmosSettings)
        {
            _client = new CosmosClient(cosmosSettings.ConnectionString);
            _database = _client.GetDatabase(cosmosSettings.DatabaseName);
            _container = _database.GetContainer(cosmosSettings.ContainerName);
        }

        public CosmosRepository(string connectionString, string databaseName, string containerName)
        {
            _client = new CosmosClient(connectionString);
            _database = _client.GetDatabase(databaseName);
            _container = _database.GetContainer(containerName);
        }

        /// <summary>
        /// Return an object by id field and partition key.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public async Task<T> Get(string id, string partitionKey) => await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
        /// <summary>
        /// Insert an element in specified container.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public async Task Insert(T element) => await _container.CreateItemAsync(element);
        /// <summary>
        /// List of all elements of current container
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> Query()
        {
            List<T> result = new List<T>();

            var queryResult = _container.GetItemQueryIterator<T>();

            while (queryResult.HasMoreResults)
            {
                FeedResponse<T> resultSet = await queryResult.ReadNextAsync();

                foreach (T element in resultSet)
                {
                    result.Add(element);
                }
            }

            return result;
        }
        /// <summary>
        /// List of element of current container specified by a queryText. It use the sql api
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns></returns>
        public async Task<List<T>> Query(string queryText)
        {
            List<T> result = new List<T>();

            var queryResult = _container.GetItemQueryIterator<T>(queryText);

            while (queryResult.HasMoreResults)
            {
                FeedResponse<T> resultSet = await queryResult.ReadNextAsync();

                foreach (T element in resultSet)
                {
                    result.Add(element);
                }
            }

            return result;
        }
        /// <summary>
        /// List of all elements of current container with linq where condition
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<List<T>> QueryLinq(Func<T, Boolean> where)
        {
            return _container.GetItemLinqQueryable<T>(true).Where(where).AsEnumerable().ToList();

        }
        /// <summary>
        /// Update a specfic item in current container.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>    
        public async Task<T> Upsert(T element)
        {
            return await _container.UpsertItemAsync<T>(element);
        }
        /// <summary>
        /// Update a specfic item in current container.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public async Task Update(string id, T element) => await _container.ReplaceItemAsync(element, id);
        /// <summary>
        /// Delete a specific item in current container.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public async Task Delete(string id, string partitionKey) => await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
    }
}
