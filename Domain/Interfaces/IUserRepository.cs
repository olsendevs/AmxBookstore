using Domain.Entities.Users;


namespace AmxBookstore.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User>> GetPagedUsersAsync(int page, int limit);
        Task<IEnumerable<User>> GetPagedClientsAsync(int page, int limit);
        Task AddAsync(User user, string password);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
    }
}
