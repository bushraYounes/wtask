using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WorkiomTest.Services;
using WorkiomTest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;



namespace WorkiomTest.Controllers
{
    [Controller]
    [Route("api/contacts")]
    public class ContactsController : Controller
    {
        private readonly MongoDBService<Contact> _contactService;

        public ContactsController(MongoDBService<Contact> mongoDBService)
        {
            _contactService = mongoDBService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllContacts()
        {
            var contacts = await _contactService.GetAll();
            return Ok(contacts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContact(string id)
        {
            var contact = await _contactService.GetById(id);
            if (contact == null)
                return NotFound();

            return Ok(contact);
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] Contact contact)
        {
            await _contactService.Insert(contact);
            return CreatedAtAction("GetContact", new { id = contact.Id }, contact);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContact(string id, [FromBody] Contact contact)
        {
            var success = await _contactService.Update(id, contact);
            if (!success)
                return NotFound();

            return Ok(contact);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(string id)
        {
            var success = await _contactService.Delete(id);
            if (!success)
                return NotFound();

            return NoContent();
        }


        // Add a new column to the tables
        [HttpPost("addColumn")]
        public async Task<IActionResult> AddColumnToContact(string columnName, string columnType)
        {

            var filter = Builders<Contact>.Filter.Empty;
            var update = Builders<Contact>.Update
                .Set($"extendedFields.{columnName}", BsonNull.Value)
                .Set($"extendedFields.Types.{columnName}", columnType);
            var options = new UpdateOptions { IsUpsert = true };

            _contactService.UpdateCollectionSchema(filter, update, options);
            return Ok();
        }



        [HttpGet("filter")]
        public async Task<IActionResult> FilterContactFields([FromQuery] Dictionary<string, object> filters)
        {
            IMongoCollection<BsonDocument> collection;
            collection = _contactService as IMongoCollection<BsonDocument>;

            var filter = Builders<BsonDocument>.Filter.Empty;

            if (filters != null && filters.Count > 0)
            {
                // Filter the regular fields
                var fieldFilters = filters
                    .Where(kv => kv.Key != "extendedFields") // Exclude the extendedFields from field filters
                    .Select(kv => Builders<BsonDocument>.Filter.Eq(kv.Key, kv.Value));

                filter = Builders<BsonDocument>.Filter.And(fieldFilters);

                // Filter the extended fields
                if (filters.ContainsKey("extendedFields") && filters["extendedFields"] is Dictionary<string, object> extendedFields)
                {
                    var extendedFieldFilters = extendedFields
                        .Select(kv => Builders<BsonDocument>.Filter.Eq($"extendedFields.{kv.Key}", kv.Value));

                    filter &= Builders<BsonDocument>.Filter.And(extendedFieldFilters);
                }
            }

            var result = collection.Find(filter).ToList();

            return Ok(result);
        }
        
        


    }
}