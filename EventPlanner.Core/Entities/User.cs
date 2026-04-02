using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventPlanner.Core.Enums;
namespace EventPlanner.Core.Entities
{
	public class User
	{
		public int Id { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string State { get; set; } = string.Empty;
		public UserRole Role { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public string? PasswordResetToken { get; set; }
		public DateTime? ResetTokenExpires { get; set; }

		public ICollection<Event> CreatedEvents { get; set; }
		public ICollection<EventAttendee> AttendedEvents { get; set; }
		public ICollection<Notification> Notifications { get; set; }
	}
}