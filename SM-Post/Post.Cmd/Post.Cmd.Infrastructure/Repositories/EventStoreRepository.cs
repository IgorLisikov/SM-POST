using CQRS.Core.Domain;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Post.Cmd.Infrastructure.Config;

namespace Post.Cmd.Infrastructure.Repositories;

public class EventStoreRepository : IEventStoreRepository
{
    // Database - a container for collections.
    // Collection - a grouping of documents, similar to a table in relational database.
    // Document - a record stored in BSON format, representing real-world data.

    private readonly IMongoCollection<EventModel> _eventStoreCollection;

    public EventStoreRepository(IOptions<MongoDbConfig> config)
    {
        var mongoClient = new MongoClient(config.Value.ConnectionString);          // create client with connectionString
        var mongoDatabase = mongoClient.GetDatabase(config.Value.Database);        // get object to access database, param - DB name

        _eventStoreCollection = mongoDatabase.GetCollection<EventModel>(config.Value.Collection);  // get object to access collection; param - collection name
    }


    public async Task<List<EventModel>> FindByAggregateId(Guid aggregateId)
    {
        return await _eventStoreCollection.Find(x => x.AggregateIdentifier == aggregateId).ToListAsync().ConfigureAwait(false);
    }

    public async Task SaveAsync(EventModel @event)
    {
        await _eventStoreCollection.InsertOneAsync(@event).ConfigureAwait(false);
    }
}
