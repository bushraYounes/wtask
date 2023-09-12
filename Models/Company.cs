using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System.Collections.Generic;


namespace WorkiomTest.Models{

    public class Company : IEntity {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public string name { get; set; }
        public int numberOfEmployees { get; set; }
        public Dictionary<string, object> extendedFields { get; set; }
    }
}

