using Microsoft.Extensions.Configuration;
using Models.Mongo;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Utilities.StringHelpers;

namespace MongoHandler
{
	public class DBConnectionHandler<T> : IDBConnectionHandler<T> where T : IBaseModel
	{
		#region Private Fields

		private readonly IConfiguration _config;

		private readonly string _connectionString;
		private readonly string _dbName;
		private IMongoCollection<T> collection;

		#endregion Private Fields

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DBConnectionHandler{T}"/> class.
		/// </summary>
		/// <param name="config">The configuration.</param>
		public DBConnectionHandler(IConfiguration config)
		{
			_config = config;

			_connectionString = _config["MongoUri"];
			_dbName = _config["DbName"];

			if (_connectionString.IsNullOrWhiteSpace() || _dbName.IsNullOrWhiteSpace())
			{
				_connectionString = Environment.GetEnvironmentVariable("MongoUri");
				_dbName = Environment.GetEnvironmentVariable("DbName");
			}
			if (_dbName.IsNullOrWhiteSpace())
			{
				_dbName = "DropMe";
			}
		}

		#endregion Public Constructors

		#region Public Methods

		/// <summary>
		/// Connects to database.
		/// </summary>
		/// <param name="collectionName">Name of the collection.</param>
		/// <returns></returns>
		public IMongoCollection<T> ConnectToDatabase(string collectionName)
		{
			var client = new MongoClient(MongoUrl.Create(_connectionString));
			var db = client.GetDatabase(_dbName);
			collection = db.GetCollection<T>(collectionName);
			return collection;
		}

		/// <summary>
		/// Creates the specified new record.
		/// </summary>
		/// <param name="newRecord">The new record.</param>
		/// <returns></returns>
		public async Task<T> Create(T newRecord)
		{
			try
			{
				UpdateIdIfNeeded(newRecord);
				await collection.InsertOneAsync(newRecord);
				return newRecord;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while inserting a new record\n{ex.Message}");
				return default;
			}
		}

		/// <summary>
		/// Creates the specified records.
		/// </summary>
		/// <param name="records">The records.</param>
		/// <returns></returns>
		public async Task<bool> Create(List<T> records)
		{
			if (records == null || records.Count == 0)
			{
				return false;
			}

			foreach (var record in records)
			{
				UpdateIdIfNeeded(record);
			}
			try
			{
				await collection.InsertManyAsync(records);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while inserting a new records\n{ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Gets Enumarable of all records.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<T> Get()
		{
			return collection.Find(all => true).ToEnumerable();
		}

		/// <summary>
		/// Gets the specified identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public T Get(string id)
		{
			return Get(r => r.Id == id).FirstOrDefault();
		}

		/// <summary>
		/// Gets the specified predicate.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		public IEnumerable<T> Get(Expression<Func<T, bool>> predicate)
		{
			return collection.Find(predicate).ToEnumerable();
		}

		/// <summary>
		/// Removes the record with specified identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public async Task<bool> Remove(string id)
		{
			try
			{
				var record = Get(id);
				await collection.DeleteOneAsync(r => r.Id == record.Id);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while deleting a record\n{ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Removes all records.
		/// </summary>
		/// <returns></returns>
		public async Task<bool> RemoveAll()
		{
			try
			{
				await collection.DeleteManyAsync(all => true);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while deleting all records\n{ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Updates the specified identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="record">The record.</param>
		/// <returns></returns>
		public async Task<bool> Update(string id, T record)
		{
			try
			{
				UpdateIdIfNeeded(record);
				await collection.ReplaceOneAsync(rcd => rcd.Id == id, record, new UpdateOptions { IsUpsert = true });
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while Updating a record\n{ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Updates the multiple.
		/// </summary>
		/// <param name="updateRecords">The update records.</param>
		/// <returns></returns>
		public async Task<bool> UpdateMultiple(List<T> updateRecords)
		{
			var objsWithoutId = new List<T>();
			var updates = new List<WriteModel<T>>();
			var filterBuilder = Builders<T>.Filter;
			foreach (var updateRecord in updateRecords)
			{
				ExtractId(updateRecord, out Type serviceInterface, out string id);
				if (!id.IsNullOrWhiteSpace())
				{
					var filter = filterBuilder.Where(x => x.Id == updateRecord.Id);
					updates.Add(new ReplaceOneModel<T>(filter, updateRecord));
				}
				else
				{
					objsWithoutId.Add(updateRecord);
				}
			}
			try
			{
				var operation1Result = true;
				var operation2Result = true;
				if (updates.Count != 0)
				{
					var result1 = await collection.BulkWriteAsync(updates);
					operation1Result = result1.ModifiedCount != 0;
				}
				if (objsWithoutId.Count != 0)
				{
					operation2Result = await Create(objsWithoutId);
				}
				return operation1Result && operation2Result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while Bulk Updating a record\n{ex.Message}");
				return false;
			}
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Extracts the identifier.
		/// </summary>
		/// <param name="newRecord">The new record.</param>
		/// <param name="serviceInterface">The service interface.</param>
		/// <param name="id">The identifier.</param>
		private static void ExtractId(T newRecord, out Type serviceInterface, out string id)
		{
			serviceInterface = typeof(T);
			id = (string)serviceInterface.GetProperty("Id").GetValue(newRecord, null);
		}

		/// <summary>
		/// Updates the identifier if needed.
		/// </summary>
		/// <param name="newRecord">The new record.</param>
		private static void UpdateIdIfNeeded(T newRecord)
		{
			ExtractId(newRecord, out Type serviceInterface, out string id);
			if (id.IsNullOrWhiteSpace())
			{
				serviceInterface.GetProperty("Id").SetValue(newRecord, Guid.NewGuid().ToString());
			}
		}

		#endregion Private Methods
	}
}
