using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CrudWithMongoDB.Models
{
    public class Customer
    {
        [BsonId]
        [BsonElement("_id"),BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("name"), BsonRepresentation(BsonType.String)]
        public string? Name { get; set; }
        [BsonElement("address"), BsonRepresentation(BsonType.String)]
        public string? Address { get; set; }
        [BsonElement("email"), BsonRepresentation(BsonType.String)]
        public string? Email { get; set; }
    }
}
