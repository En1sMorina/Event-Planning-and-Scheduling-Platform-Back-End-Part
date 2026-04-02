using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlanner.Application.DTOs.Event
{
	public class EventDto
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Location { get; set; }
		public DateTime StartDate { get; set; }
		public int CurrentAttendeesCount { get; set; }
		public string Status { get; set; }
		public string CategoryName { get; set; }
		public string CreatorName { get; set; }
		public int CreatorUserId { get; set; }
		public decimal Price { get; set; }
	}
}