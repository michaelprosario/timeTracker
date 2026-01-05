using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TimeTrackerDbContext _context;
    
    public UserRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());
    }
    
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
    
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }
    
    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email.ToLower());
    }
}
