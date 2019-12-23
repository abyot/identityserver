using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class Tenant
    {
        [BsonElement("id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Email { get; set; }
        public List<Int32> Kommunenummerer { get; set; }
        public List<Int32> Organisasjonsnummerer { get; set; }
        public int Category { get; set; }
    }
}
