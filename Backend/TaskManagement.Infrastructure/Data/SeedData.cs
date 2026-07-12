using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // Demo user ID (fixed for seed consistency)
            var demoUserId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

            // Pre-computed BCrypt hash for password "Demo@1234"
            // Generated via: BCrypt.Net.BCrypt.HashPassword("Demo@1234")
            const string hashedPassword = "$2a$11$K4GBPCLJiSYDAsZLxn5uWOQZQJlMdOJBwKFkZv8o5h7LjAqEi0Fje";

            var demoUser = new User
            {
                Id = demoUserId,
                Username = "demo",
                Email = "demo@taskmanagement.com",
                PasswordHash = hashedPassword,
                FirstName = "Demo",
                LastName = "User",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            };

            modelBuilder.Entity<User>().HasData(demoUser);

            // Sample categories
            var categories = new[]
            {
                new Category
                {
                    Id = Guid.Parse("b1c2d3e4-f5a6-7890-bcde-f12345678901"),
                    Name = "İş",
                    Description = "İş ile ilgili görevler",
                    Color = "#007bff",
                    UserId = demoUserId,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = Guid.Parse("c1d2e3f4-a5b6-7890-cdef-123456789012"),
                    Name = "Kişisel",
                    Description = "Kişisel görevler ve hatırlatmalar",
                    Color = "#28a745",
                    UserId = demoUserId,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = Guid.Parse("d1e2f3a4-b5c6-7890-defa-123456789023"),
                    Name = "Eğitim",
                    Description = "Eğitim ve öğrenme görevleri",
                    Color = "#ffc107",
                    UserId = demoUserId,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            modelBuilder.Entity<Category>().HasData(categories);
        }
    }
}
