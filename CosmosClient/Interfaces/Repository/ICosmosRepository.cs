using CosmosDbClient.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosDbClient.Interfaces.Repository
{
    /// <summary>
    /// This is the cosmos repository contract
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface ICosmosRepository<T> where T : class, ICosmosModel, new()
    {
        Task<T> Get(string id, string partitionKey);
        Task Insert(T element);
        Task Update(string id, T element);
        Task Delete(string id, string partitionKey);
        Task<List<T>> Query();
        Task<List<T>> Query(string queryText);
        List<T> QueryLinq(Func<T, Boolean> where);
        Task<T> Upsert(T element);
        Task CreateDatabase(string databaseId);
        Task CreateContainer(string containerId, string partitionKeyPath);
    }
}
