using System;
using DigitalMemoryMap.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalMemoryMap.DAL.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Ensure database exists
        await context.Database.EnsureCreatedAsync();

        // Ensure Admin user exists
        var adminEmail = "admin@digitalmemory.com";
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (existingAdmin == null)
        {
            var admin = new User
            {
                FullName = "System Admin",
                Email = adminEmail,
                // BCrypt hashed "Admin@123"
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                RoleId = 2, // Admin
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
