using Models.Mongo;
using System;

namespace Models.Application
{
	public class LastUpateCollectionMd : IBaseModel
	{
		public string Id { get; set; }
		public string CollectionName { get; set; }
		public DateTime LastUpdate { get; set; }
	}
}
