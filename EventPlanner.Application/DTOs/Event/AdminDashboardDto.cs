using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventPlanner.Application.DTOs.Auth; 
using EventPlanner.Application.DTOs.Event; 

namespace EventPlanner.Application.DTOs.Event
{
	
	public class AdminManagerDetailsDto
	{
		public int ManagerId { get; set; }
		public string FullName { get; set; } = string.Empty;
		public IEnumerable<EventDto> CreatedEvents { get; set; } = new List<EventDto>();
		public IEnumerable<EventDto> JoinedEvents { get; set; } = new List<EventDto>();
	}

	public class AdminUserDetailsDto
	{
		public int UserId { get; set; }
		public string FullName { get; set; } = string.Empty;
		public IEnumerable<EventDto> JoinedEvents { get; set; } = new List<EventDto>();
		public IEnumerable<EventDto> CancelledEvents { get; set; } = new List<EventDto>();
	}

	public class AdminEventStatsDto
	{
		public EventDto EventDetails { get; set; }
		public int AttendeesCount { get; set; }
		public int Capacity { get; set; }
		public double OccupancyRate { get; set; }
		public int CancellationsCount { get; set; }
	}
}