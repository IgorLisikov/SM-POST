using CQRS.Core.Outbox;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Post.Cmd.Infrastructure.Config;

namespace Post.Cmd.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<OutboxMessage> _outboxCollection;

        public OutboxRepository(IOptions<MongoDbConfig> config, IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            var mongoDatabase = _mongoClient.GetDatabase(config.Value.Database);
            _outboxCollection = mongoDatabase.GetCollection<OutboxMessage>(config.Value.OutboxCollection);
        }

        public async Task SaveAsync(OutboxMessage message, IClientSessionHandle session = null)
        {
            if (session == null)
                await _outboxCollection.InsertOneAsync(message);
            else
                await _outboxCollection.InsertOneAsync(session, message);
        }

        public async Task<List<OutboxMessage>> GetUnpublishedAsync(int limit = 100)
        {
            var filter = Builders<OutboxMessage>.Filter.Eq(x => x.Published, false);
            var find = _outboxCollection.Find(filter)
                                        .SortBy(x => x.OccurredOn)
                                        .Limit(limit);

            return await find.ToListAsync();
        }

        public async Task MarkAsPublishedAsync(Guid id, DateTime publishedAt)
        {
            var filter = Builders<OutboxMessage>.Filter.Eq(x => x.Id, id);
            var update = Builders<OutboxMessage>.Update
                .Set(x => x.Published, true)
                .Set(x => x.PublishedAt, publishedAt);

            await _outboxCollection.UpdateOneAsync(filter, update);
        }
    }
}
