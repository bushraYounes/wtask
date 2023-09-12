using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System.Collections.Generic;


namespace WorkiomTest.Models {
    public interface IEntity
    {
        ObjectId Id { get; set; }
    }
}