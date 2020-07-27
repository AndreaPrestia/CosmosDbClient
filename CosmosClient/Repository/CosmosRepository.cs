using CosmosDbClient.Entities;
using CosmosDbClient.Exceptions;
using CosmosDbClient.Resources;
using CosmosDbClient.Settings;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDbClient.Repository
{
    /// <summary>
    /// This is the generic repository object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CosmosRepository<T> : IDisposable where T : CosmosEntity, new()
    {
        private bool _disposed = false;
        private readonly CosmosClient _client;
        private readonly Database _database;
        private readonly Container _container;

        public CosmosRepository(ICosmosSettings cosmosSettings)
        {
            if(cosmosSettings == null)
                throw new ArgumentNullException($"{nameof(cosmosSettings)} reference not set to an instance of an object");

            if (string.IsNullOrEmpty(cosmosSettings.ConnectionString))
                throw new ArgumentNullException($"{nameof(cosmosSettings.ConnectionString)} is null or empty");

            if (string.IsNullOrEmpty(cosmosSettings.DatabaseName))
                throw new ArgumentNullException($"{nameof(cosmosSettings.DatabaseName)} is null or empty");

            if (string.IsNullOrEmpty(cosmosSettings.ContainerName))
                throw new ArgumentNullException($"{nameof(cosmosSettings.ContainerName)} is null or empty");

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
        /// Return an object by id field
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get(string id) => _container.GetItemLinqQueryable<T>(true).Where(x => x.Id == id).FirstOrDefault();

        /// <summary>
        /// Insert an element in specified container.
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws when element is null</exception>
        /// <exception cref="CosmosDbClientException">Throws when Cosmos response code is not between 200-299</exception>
        /// <param name="element"></param>
        /// <returns></returns>
        public async Task<T> Insert(T element)
        {
            if (element == null)
                throw new ArgumentNullException($"{nameof(element)} reference not set to an instance of an object<{typeof(T)}>");

            if (string.IsNullOrEmpty(element.Id) || Guid.Parse(element.Id) == Guid.Empty)
                element.Id = Guid.NewGuid().ToString();

            ItemResponse<T> response = await _container.CreateItemAsync(element).ConfigureAwait(false);

            if (!IsSuccessStatusCode(response))
                throw new CosmosDbClientException($"Cannot upsert element response code {response.StatusCode}");

            return response.Resource;
        }

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
                FeedResponse<T> resultSet = await queryResult.ReadNextAsync().ConfigureAwait(false);

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
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentNullException">Throws when queryText is null</exception>
        /// <returns></returns>
        public async Task<List<T>> Query(string queryText, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(queryText))
                throw new ArgumentNullException($"{nameof(queryText)} is empty or null");

            List<T> result = new List<T>();

            QueryDefinition queryDefinition = new QueryDefinition(queryText);

            if(parameters != null)
            {
                foreach(KeyValuePair<string, object> parameter in parameters)
                {
                    queryDefinition.WithParameter(parameter.Key, parameter.Value);
                }
            }

            var queryResult = _container.GetItemQueryIterator<T>(queryDefinition);

            while (queryResult.HasMoreResults)
            {
                FeedResponse<T> resultSet = await queryResult.ReadNextAsync().ConfigureAwait(false);

                foreach (T element in resultSet)
                {
                    result.Add(element);
                }
            }

            return result;
        }

        
        /// <summary>
        /// List of element of current container specified by a queryText and continuation token. It use the sql api
        /// </summary>
        /// <param name="queryText"></param>
        /// <param name="continuationToken"></param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentNullException">Throws when queryText or continuationToken is null</exception>
        /// <returns></returns>
        public async Task<List<T>> Query(string queryText, string continuationToken, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(queryText))
                throw new ArgumentNullException($"{nameof(queryText)} is empty or null");

            if (string.IsNullOrEmpty(continuationToken))
                throw new ArgumentNullException($"{nameof(continuationToken)} is empty or null");

            List<T> result = new List<T>();

            QueryDefinition queryDefinition = new QueryDefinition(queryText);

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    queryDefinition.WithParameter(parameter.Key, parameter.Value);
                }
            }

            var queryResult = _container.GetItemQueryIterator<T>(queryDefinition, continuationToken);

            while (queryResult.HasMoreResults)
            {
                FeedResponse<T> resultSet = await queryResult.ReadNextAsync().ConfigureAwait(false);

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
        /// <exception cref="ArgumentNullException">Throws when where is null</exception>
        /// <returns></returns>
        public List<T> QueryLinq(Func<T, Boolean> where) => where == null ? throw new ArgumentNullException($"{nameof(where)} is null") : _container.GetItemLinqQueryable<T>(true).Where(where).AsEnumerable().ToList();

        /// <summary>
        /// Update a specfic item in current container.
        /// </summary>
        /// <param name="element"></param>
        /// <exception cref="ArgumentNullException">Throws when element or element.Id is null</exception>
        /// <exception cref="CosmosDbClientException">Throws when Cosmos response code is not between 200-299</exception>
        /// <returns></returns>    
        public async Task<T> Upsert(T element)
        {
            if (element == null)
                throw new ArgumentNullException($"{nameof(element)} reference not set to an instance of an object<{typeof(T)}>");

            if (string.IsNullOrEmpty(element.Id) || Guid.Parse(element.Id) == Guid.Empty)
                element.Id = Guid.NewGuid().ToString();

            ItemResponse<T> response = await _container.UpsertItemAsync<T>(element).ConfigureAwait(false);

            if (!IsSuccessStatusCode(response))
                throw new CosmosDbClientException($"Cannot upsert element response code {response.StatusCode}");

            return response.Resource;
        }

        /// <summary>
        /// Update a specfic item in current container.
        /// </summary>
        /// <param name="element"></param>
        /// <exception cref="ArgumentNullException">Throws when element or element.Id is null</exception>
        /// <exception cref="ArgumentException">Throws when element.Id is an empty Guid</exception>
        /// <exception cref="CosmosDbClientException">Throws when Cosmos response code is not between 200-299</exception>
        /// <returns></returns>
        public async Task<T> Update(T element)
        {
            if (element == null)
                throw new ArgumentNullException($"{nameof(element)} reference not set to an instance of an object<{typeof(T)}>");

            if (string.IsNullOrEmpty(element.Id))
                throw new ArgumentNullException($"{nameof(element.Id)} is null or empty");

            if (Guid.Parse(element.Id) == Guid.Empty)
                throw new ArgumentException(ErrorMessages.InvalidElementId);

            ItemResponse<T> response = await _container.ReplaceItemAsync(element, element.Id).ConfigureAwait(false);

            if (!IsSuccessStatusCode(response))
                throw new CosmosDbClientException($"Cannot upsert element response code {response.StatusCode}");

            return response.Resource;
        }

        /// <summary>
        /// Update a specfic item in current container.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="element"></param>
        /// <exception cref="ArgumentNullException">Throws when element or element.Id or partitionKeyValue is null</exception>
        /// <exception cref="ArgumentException">Throws when element.Id is an empty Guid</exception>
        /// <exception cref="CosmosDbClientException">Throws when Cosmos response code is not between 200-299</exception>
        /// <returns></returns>
        public async Task<T> Update(T element, string partitionKeyValue)
        {
            if (element == null)
                throw new ArgumentNullException($"{nameof(element)} reference not set to an instance of an object<{typeof(T)}>");

            if (string.IsNullOrEmpty(element.Id))
                throw new ArgumentNullException($"{nameof(element.Id)} is null or empty");

            if (Guid.Parse(element.Id) == Guid.Empty)
                throw new ArgumentException(ErrorMessages.InvalidElementId);

            if (string.IsNullOrEmpty(partitionKeyValue))
                throw new ArgumentNullException($"{nameof(partitionKeyValue)} is null or empty");

            ItemResponse<T> response = await _container.ReplaceItemAsync(element, element.Id, new PartitionKey(partitionKeyValue)).ConfigureAwait(false);

            if (!IsSuccessStatusCode(response))
                throw new CosmosDbClientException($"Cannot upsert element response code {response.StatusCode}");

            return response.Resource;
        }

        /// <summary>
        /// Delete a specific item in current container, with id and partitionKey.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partitionKey"></param>        
        /// <exception cref="ArgumentNullException">Throws when id or partitionKey is null</exception>
        /// <exception cref="ArgumentException">Throws when id is an empty Guid</exception>
        /// <exception cref="CosmosDbClientException">Throws when Cosmos response code is not between 200-299</exception>
        /// <returns></returns>
        public async Task Delete(string id, string partitionKey)
        {

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException($"{nameof(id)} is null or empty");

            if (Guid.Parse(id) == Guid.Empty)
                throw new ArgumentException(ErrorMessages.InvalidElementId);

            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentNullException($"{nameof(partitionKey)} is null or empty");

            ItemResponse<T> response = await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);

            if (!IsSuccessStatusCode(response))
                throw new CosmosDbClientException($"Cannot upsert element response code {response.StatusCode}");
        }

        /// <summary>
        /// Delete a specific item in current container.
        /// </summary>
        /// <param name="element"></param>
        /// <exception cref="ArgumentNullException">Throws when element or element.Id or partitionKeyValue is null</exception>
        /// <exception cref="ArgumentException">Throws when element.Id is an empty Guid</exception>
        /// <exception cref="CosmosDbClientException">Throws when Cosmos response code is not between 200-299</exception>
        /// <returns></returns>
        public async Task Delete(T element)
        {
            if (element == null)
                throw new ArgumentNullException($"{nameof(element)} reference not set to an instance of an object<{typeof(T)}>");

            if (string.IsNullOrWhiteSpace(element.Id))
                throw new ArgumentNullException($"{nameof(element.Id)} is null or empty");

            if (Guid.Parse(element.Id) == Guid.Empty)
                throw new ArgumentException(ErrorMessages.InvalidElementId);

            ItemResponse<T> response = await _container.DeleteItemAsync<T>(element.Id, new PartitionKey(element.Id)).ConfigureAwait(false);

            if (!IsSuccessStatusCode(response))
                throw new CosmosDbClientException($"Cannot upsert element response code {response.StatusCode}");
        }

        /// <summary>
        /// Create a database in the current client , specificing the id
        /// </summary>
        /// <param name="databaseId"></param>
        /// <exception cref="ArgumentNullException">Throws when element databaseId is null</exception>
        /// <exception cref="CosmosDbClientException">Throws when Cosmos response code is not between 200-299</exception>
        public async Task CreateDatabase(string databaseId)
        {
            if (string.IsNullOrEmpty(databaseId))
                throw new ArgumentNullException($"{nameof(databaseId)} is null or empty");

            DatabaseResponse response = await _client.CreateDatabaseIfNotExistsAsync(databaseId).ConfigureAwait(false);

            if(IsSuccessStatusCode(response))
                throw new CosmosDbClientException($"Cannot create database {databaseId} response code {response.StatusCode}");
        }

        /// <summary>
        /// Create a container inside a database
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="partitionKeyPath"></param>
        /// <exception cref="ArgumentNullException">Throws when element containerId or partitionKeyPath is null</exception>
        /// <exception cref="CosmosDbClientException">Throws when Cosmos response code is not between 200-299</exception>
        /// <returns></returns>
        public async Task CreateContainer(string containerId, string partitionKeyPath)
        {
            if (string.IsNullOrEmpty(containerId))
                throw new ArgumentNullException($"{nameof(containerId)} is null or empty");

            if (string.IsNullOrEmpty(partitionKeyPath))
                throw new ArgumentNullException($"{nameof(partitionKeyPath)} is null or empty");

            ContainerResponse response = await _database.CreateContainerAsync(containerId, partitionKeyPath).ConfigureAwait(false);

            if (IsSuccessStatusCode(response))
                throw new CosmosDbClientException($"Cannot create database {containerId} response code {response.StatusCode}");
        }

        /// <summary>
        /// Count all elements from the container.
        /// </summary>
        /// <returns></returns>
        public async Task<double> Count()
        {
            var queryResult = _container.GetItemQueryIterator<T>();

            int result = 0;

            if (queryResult.HasMoreResults)
            {
                FeedResponse<T> resultSet = await queryResult.ReadNextAsync().ConfigureAwait(false);

                result = resultSet.Count();
            }

            return result;
        }

        /// <summary>
        /// Count all elements from the container with a specific linq query.
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public double Count(Func<T, Boolean> where) => _container.GetItemLinqQueryable<T>(true).Where(where).Count();

        private static bool IsSuccessStatusCode(ItemResponse<T> response)
        {
            if (response == null)
                return false;

            int statusCode = (int)response.StatusCode;

            if (statusCode >= 200 && statusCode <= 299)
                return true;

            return false;
        }

        private static bool IsSuccessStatusCode(DatabaseResponse response)
        {
            if (response == null)
                return false;

            int statusCode = (int)response.StatusCode;

            if (statusCode >= 200 && statusCode <= 299)
                return true;

            return false;
        }

        private static bool IsSuccessStatusCode(ContainerResponse response)
        {
            if (response == null)
                return false;

            int statusCode = (int)response.StatusCode;

            if (statusCode >= 200 && statusCode <= 299)
                return true;

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _client.Dispose();
            }

            _disposed = true;
        }
    }
}
