using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EventPlanner.Application.DTOs.Event
{
	public class LocalizedEventsDto
	{
		public IEnumerable<EventDto> SameStateEvents { get; set; } = new List<EventDto>();
		public IEnumerable<EventDto> OtherStateEvents { get; set; } = new List<EventDto>();
	}
}
