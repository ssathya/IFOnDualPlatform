using Models.Mongo;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoHandler
{
    public interface IDBConnectionHandler<T> where T : IBaseModel
	{
		IMongoCollection<T> ConnectToDatabase(string collectionName);

		Task<bool> Create(List<T> records);

		Task<T> Create(T newRecord);

		IEnumerable<T> Get();

		IEnumerable<T> Get(Expression<Func<T, bool>> predicate);

		T Get(string id);

		Task<bool> Remove(string id);

		Task<bool> RemoveAll();

		Task<bool> Update(string id, T record);

		Task<bool> UpdateMultiple(List<T> updateRecords);
	}
}
