using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace AmxBookstore.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task AddAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, user.Role.ToString());
            }
        }

        public async Task UpdateAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                user.Deleted = true;
                await _userManager.UpdateAsync(user);
            }
        }
        public async Task<IEnumerable<User>> GetPagedUsersAsync(int page, int limit)
        {
            var query = _userManager.Users.AsQueryable();

            query = query.Skip((page - 1) * limit).Take(limit);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<User>> GetPagedClientsAsync(int page, int limit)
        {
            var query = _userManager.Users.AsQueryable();

            query = query.Where(user => user.Role == Domain.Entities.Users.Enum.UserRoles.Client).Skip((page - 1) * limit).Take(limit);

            return await query.ToListAsync();
        }
    }
}
