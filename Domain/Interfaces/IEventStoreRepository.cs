using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmxBookstore.Domain.Interfaces
{
    public interface IEventStoreRepository
    {
        Task SaveEventAsync<T>(T @event, Guid streamId);
        Task<IEnumerable<T>> GetEventsAsync<T>(Guid streamId);
    }
}
