using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;

namespace EventPlanner.Application.DTOs.Event
{
	public class ManagerDashboardDto
	{

		public IEnumerable<EventDto> CreatedEvents { get; set; } = new List<EventDto>();

		public IEnumerable<EventDto> JoinedEvents { get; set; } = new List<EventDto>();

		public IEnumerable<EventAttendeeSummaryDto> AttendeeDetails { get; set; } = new List<EventAttendeeSummaryDto>();
	}

	public class EventAttendeeSummaryDto
	{
		public string EventTitle { get; set; } = string.Empty;
		public IEnumerable<string> JoinedUsers { get; set; } = new List<string>();
		public IEnumerable<string> CanceledUsers { get; set; } = new List<string>();
	}
}