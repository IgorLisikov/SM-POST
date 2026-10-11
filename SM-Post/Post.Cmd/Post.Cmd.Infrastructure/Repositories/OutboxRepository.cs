using CQRS.Core.Outbox;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Post.Cmd.Infrastructure.Config;
using System.Runtime.CompilerServices;

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
            _outboxCollection = mongoDatabase.GetCollection<OutboxMessage>("outbox");
        }

        public async Task SaveAsync(OutboxMessage message, IClientSessionHandle session = null, CancellationToken token = default)
        {
            if (session == null)
                await _outboxCollection.InsertOneAsync(message);
            else
                await _outboxCollection.InsertOneAsync(session, message);
        }

        public async IAsyncEnumerable<OutboxMessage> SubscribeAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<OutboxMessage>>()
                .Match(change => change.OperationType == ChangeStreamOperationType.Insert);
            var options = new ChangeStreamOptions
            {
                MaxAwaitTime = TimeSpan.FromSeconds(10)  // 
            };
            using var changeCursor = await _outboxCollection.WatchAsync(pipeline, options, token);
            var recoveredMessageIds = new HashSet<Guid>();

            // Recovery Scan: process events that already exist at the moment the WatchAsync() is called, since they are not detected by changeCursor:
            using (var pendingCursor = await _outboxCollection
                .Find(message => !message.Published)
                .SortBy(message => message.OccurredOn)
                .ToCursorAsync(token))
            {
                while (await pendingCursor.MoveNextAsync(token))
                {
                    foreach (var message in pendingCursor.Current)
                    {
                        recoveredMessageIds.Add(message.Id);
                        yield return message;
                    }
                }
            }
            // End of Recovery Scan.

            // Process live stream of events that are detected by changeCursor:
            while (await changeCursor.MoveNextAsync(token))  // waits for MaxAwaitTime; can return sooner when a matching insert arrives
            {
                foreach (var change in changeCursor.Current)
                {
                    var message = change.FullDocument;

                    // Skip the event if it is already processed by Recovery Scan (Hashset is used to avoid database read):
                    if (!recoveredMessageIds.Remove(message.Id))
                        yield return message;
                }
            }
        }

        public async Task MarkAsPublishedAsync(Guid id, DateTime publishedAt, CancellationToken token = default)
        {
            var filter = Builders<OutboxMessage>.Filter.Eq(x => x.Id, id);
            var update = Builders<OutboxMessage>.Update
                .Set(x => x.Published, true)
                .Set(x => x.PublishedAt, publishedAt);

            await _outboxCollection.UpdateOneAsync(filter, update);
        }
    }
}
