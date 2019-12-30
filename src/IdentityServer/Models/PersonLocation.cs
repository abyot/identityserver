using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class PersonLocation
    {
        [BsonElement("id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public Int64 pid { get; set; }
        public int kommunenummer { get; set; }
        public int Category { get; set; }
    }
}
