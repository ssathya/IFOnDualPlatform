using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Mongo
{
    public interface IBaseModel
    {
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		string Id { get; set; }
	}
}
