using FormPlay.Data;
using FormPlay.Models;
using Microsoft.EntityFrameworkCore;

namespace FormPlay.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // Since we only have two users, this is a convenience method
        // to get the current user and partner in one go
        public async Task<(User currentUser, User partner)> GetCurrentUserAndPartnerAsync(int currentUserId)
        {
            var users = await _context.Users.ToListAsync();
            var currentUser = users.FirstOrDefault(u => u.Id == currentUserId);
            var partner = users.FirstOrDefault(u => u.Id != currentUserId);
            
            return (currentUser, partner);
        }
    }
}
