using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;

namespace gomind_backend_api.DB
{
    public interface IDynamoDbService
    {
        Task SaveAsync<T>(T item);
        Task<T?> GetAsync<T>(object key) where T : class;
        Task DeleteAsync<T>(object key);
    }    
    public class DynamoDbService : IDynamoDbService    
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly DynamoDBContext _context;

        public DynamoDbService(IAmazonDynamoDB dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient;
            _context = new DynamoDBContext(_dynamoDbClient);
        }

        public async Task SaveAsync<T>(T item)
        {
            await _context.SaveAsync(item);
        }

        public async Task<T?> GetAsync<T>(object key) where T : class
        {
            return await _context.LoadAsync<T>(key);
        }

        public async Task DeleteAsync<T>(object key)
        {
            await _context.DeleteAsync<T>(key);
        }
    }
}
