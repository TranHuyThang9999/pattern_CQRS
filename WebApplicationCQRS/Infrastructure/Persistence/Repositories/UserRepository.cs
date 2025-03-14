using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WebApplicationCQRS.Domain.Entities;
using WebApplicationCQRS.Domain.Interfaces;
using WebApplicationCQRS.Infrastructure.Persistence.Context;

namespace WebApplicationCQRS.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User> GetUserById(int id) =>
        await _context.Users.FindAsync(id);

    public async Task<List<User>> GetActiveUsers(int currentUserId) =>
        await _context.Users
            .Where(u => u.Id != currentUserId)
            .ToListAsync();

    public async Task<int> CreateUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user.Id;
    }


    public async Task<bool> UpdateUserById(UserUpdateProfile userDto)
    {
        var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id);

        if (userToUpdate == null)
            return false;

        if (!string.IsNullOrWhiteSpace(userDto.AvatarUrl))
            userToUpdate.AvatarUrl = userDto.AvatarUrl;

        if (!string.IsNullOrWhiteSpace(userDto.Email))
            userToUpdate.Email = userDto.Email;

        if (!string.IsNullOrWhiteSpace(userDto.Password))
            userToUpdate.Password = userDto.Password;
        userToUpdate.LastPasswordChangedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task DeleteUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<User> GetUserByUsername(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Name == username);
    }

    public async Task<bool> CheckListUserExistsByUserIDs(List<int> userIDs)
    {
        if (userIDs == null || userIDs.Count == 0)
            return false;
        var count = await _context.Users
            .Where(u => userIDs.Contains(u.Id))
            .CountAsync();

        return count == userIDs.Count;
    }
}