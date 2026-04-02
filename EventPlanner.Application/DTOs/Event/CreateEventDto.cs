using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlanner.Application.DTOs.Event
{
	public class CreateEventDto
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string State { get; set; }
		public string Address { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int CapacityLimit { get; set; }
		public int CategoryId { get; set; }
		public decimal Price { get; set; }
	}
}