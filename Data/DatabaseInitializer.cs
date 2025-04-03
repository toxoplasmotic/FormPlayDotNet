using FormPlay.Models;

namespace FormPlay.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if users already exist
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            // Seed users (only two as per requirements)
            var users = new List<User>
            {
                new User { Id = 1, Name = "Matt", Email = "matt@formplay.local" },
                new User { Id = 2, Name = "Mina", Email = "mina@formplay.local" }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}
