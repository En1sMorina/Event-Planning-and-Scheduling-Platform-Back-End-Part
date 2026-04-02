using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventPlanner.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Application.Interfaces
{
	public interface IApplicationDbContext
	{
		DbSet<User> Users { get; }
		DbSet<Event> Events { get; }
		DbSet<EventCategory> Categories { get; }
		DbSet<EventAttendee> Attendees { get; }
		DbSet<Discussion> Discussions { get; }
		DbSet<Notification> Notifications { get; }
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
