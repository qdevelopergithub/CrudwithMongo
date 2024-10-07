using CrudWithMongoDB.DataAccess;
using CrudWithMongoDB.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CrudWithMongoDB.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMongoCollection<Customer> _customerCollection;
        public CustomerController(MongoDbServices mongoDbServices)
        {
            _customerCollection = mongoDbServices.Database?.GetCollection<Customer>("customer");   
        }
        [HttpGet]
        public async Task<IEnumerable<Customer>> GetAll()
        {
            return await _customerCollection.Find(FilterDefinition<Customer>.Empty).ToListAsync();
        }
        [HttpGet]
        public async Task<ActionResult<Customer?>> GetById(string id)
        {
            var filter  = Builders<Customer>.Filter.Eq(x=>x.Id, id);
            var customer = _customerCollection.Find(filter).FirstOrDefault();
            return customer == null ? NotFound() : Ok(customer);    
        }
        [HttpPost]
        public async Task<ActionResult> CreateCustomer(Customer customer)
        {
            await _customerCollection.InsertOneAsync(customer);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }
        [HttpPut]
        public async Task <ActionResult> UpdateCustomer(Customer customer)
        {
            var filter = Builders<Customer>.Filter.Eq(x => x.Id, customer.Id);
          await _customerCollection.ReplaceOneAsync(filter,customer);
            return Ok();
        }
        [HttpDelete]
        public async Task<ActionResult> Delete(string id)
        {
            var filter  = Builders<Customer>.Filter.Eq(x=>x.Id, id);
            await _customerCollection.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
