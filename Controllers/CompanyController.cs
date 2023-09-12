using System;
using Microsoft.AspNetCore.Mvc;
using WorkiomTest.Services;
using WorkiomTest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

using MongoDB.Driver;

namespace WorkiomTest.Controllers
{
    [Controller]
    [Route("api/company")]
    public class CompanyController : Controller
    {
        private readonly MongoDBService<Company> _companyService;

        public CompanyController(MongoDBService<Company> mongoDBService)
        {
            _companyService = mongoDBService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _companyService.GetAll();
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(string id)
        {
            var company = await _companyService.GetById(id);
            if (company == null)
                return NotFound();

            return Ok(company);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] Company company)
        {
            await _companyService.Insert(company);
            return CreatedAtAction("GetCompany", new { id = company.Id }, company);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(string id, [FromBody] Company company)
        {
            var success = await _companyService.Update(id, company);
            if (!success)
                return NotFound();

            return Ok(company);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(string id)
        {
            var success = await _companyService.Delete(id);
            if (!success)
                return NotFound();

            return NoContent();
        }



        // Add a new column to the tables
        [HttpPost("addColumn")]
        public async Task<IActionResult> AddColumnToCompany(string columnName, string columnType)
        {
            var filter = Builders<Company>.Filter.Empty;
            var update = Builders<Company>.Update
                .Set($"extendedFields.{columnName}", BsonNull.Value)
                .Set($"extendedFields.Types.{columnName}", columnType);
            var options = new UpdateOptions { IsUpsert = true };

            _companyService.UpdateCollectionSchema(filter, update, options);
            return Ok();
        }


        [HttpGet("filter")]
        public async Task<IActionResult> FilterCompanyFields([FromQuery] Dictionary<string, object> filters)
        {
            IMongoCollection<BsonDocument> collection;
            collection = _companyService as IMongoCollection<BsonDocument>;

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