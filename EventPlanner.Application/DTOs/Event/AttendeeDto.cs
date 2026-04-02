using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlanner.Application.DTOs 
{
	public class AttendeeDto
	{
		public int UserId { get; set; } 
		public string Email { get; set; }
		public DateTime JoinedAt { get; set; }
	}
}
