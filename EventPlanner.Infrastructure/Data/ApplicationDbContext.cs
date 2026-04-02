using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventPlanner.Application.Interfaces;
using EventPlanner.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Infrastructure.Data
{
	public class ApplicationDbContext : DbContext, IApplicationDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Event> Events { get; set; }
		public DbSet<EventCategory> Categories { get; set; }
		public DbSet<EventAttendee> Attendees { get; set; }
		public DbSet<Discussion> Discussions { get; set; }
		public DbSet<Notification> Notifications { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
			modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
			modelBuilder.Entity<Event>().HasOne(e => e.Creator).WithMany(u => u.CreatedEvents).HasForeignKey(e => e.CreatorUserId).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<EventAttendee>().HasOne(ea => ea.Event).WithMany(e => e.Attendees).HasForeignKey(ea => ea.EventId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<EventAttendee>().HasOne(ea => ea.User).WithMany(u => u.AttendedEvents).HasForeignKey(ea => ea.UserId).OnDelete(DeleteBehavior.Restrict);
		
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Event>()
				.Property(e => e.Price)
				.HasColumnType("decimal(18,2)");
		}
	}
}
