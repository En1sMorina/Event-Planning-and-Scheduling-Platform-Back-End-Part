using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventPlanner.Core.Enums;
namespace EventPlanner.Core.Entities
{
	public class Event
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string State { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int CapacityLimit { get; set; }
		public int CurrentAttendeesCount { get; set; }
		public EventStatus Status { get; set; } = EventStatus.Upcoming;
		public bool IsReminderSent { get; set; } = false;
		public int CategoryId { get; set; }
		public EventCategory Category { get; set; }
		public int CreatorUserId { get; set; }
		public User Creator { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
		public decimal Price { get; set; }
		public ICollection<EventAttendee> Attendees { get; set; }
		public ICollection<Discussion> Discussions { get; set; }
	}
}