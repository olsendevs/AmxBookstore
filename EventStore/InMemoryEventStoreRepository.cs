using AmxBookstore.Domain.Interfaces;
using System.Collections.Concurrent;


namespace AmxBookstore.Infrastructure.EventStore
{
    public class InMemoryEventStoreRepository : IEventStoreRepository
    {
        private readonly ConcurrentDictionary<Guid, List<object>> _eventStore = new();

        public Task SaveEventAsync<T>(T @event, Guid streamId)
        {
            if (!_eventStore.ContainsKey(streamId))
            {
                _eventStore[streamId] = new List<object>();
            }

            _eventStore[streamId].Add(@event);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> GetEventsAsync<T>(Guid streamId)
        {
            if (_eventStore.TryGetValue(streamId, out var events))
            {
                var typedEvents = events.OfType<T>();
                return Task.FromResult<IEnumerable<T>>(typedEvents);
            }

            return Task.FromResult<IEnumerable<T>>(Enumerable.Empty<T>());
        }
    }
}
