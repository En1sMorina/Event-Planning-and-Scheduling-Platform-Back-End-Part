using System;
using System.Linq;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;

namespace EventPlanner.Infrastructure.Data
{
	public static class DbInitializer
	{
		public static void Initialize(ApplicationDbContext context)
		{
		
			if (!context.Users.Any())
			{
				context.Users.Add(new User
				{
					FullName = "Admin",
					Username = "admin",
					Email = "admin@test.com",
					Role = UserRole.Admin,
					State = "Global",
					PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123")
				});
				context.SaveChanges(); 
			}
			
			if (!context.Categories.Any())
			{
				context.Categories.AddRange(
					new EventCategory { Name = "Conference", Description = "Professional conferences and large gatherings" },
					new EventCategory { Name = "Meetup", Description = "Casual community gatherings" },
					new EventCategory { Name = "Workshop", Description = "Hands-on interactive sessions" }
				);
				context.SaveChanges();
			}
		}
	}
}