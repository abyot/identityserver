using IdentityServer.AppConfig;
using IdentityServer.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace IdentityServer.MongoContext
{
    public class MongoDbDataContext
    {
        private readonly string _connectionStrings = string.Empty;
        private readonly string _databaseName = string.Empty;
        private readonly string _collectionName = string.Empty;

        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoDbDataContext(string strCollectionName)
        {
            this._collectionName = strCollectionName;
            this._connectionStrings = AppConfiguration.GetConfiguration("ServerName");
            this._databaseName = AppConfiguration.GetConfiguration("DatabaseName");
            this._client = new MongoClient(_connectionStrings);
            this._database = _client.GetDatabase(_databaseName);
        }

        public IMongoClient Client
        {
            get { return _client; }
        }

        public IMongoDatabase Database
        {
            get { return _database; }
        }

        public IMongoCollection<Tenant> GetTenants
        {
            get { return _database.GetCollection<Tenant>(_collectionName); }
        }
    }
}
