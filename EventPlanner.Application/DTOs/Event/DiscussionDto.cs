using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlanner.Application.DTOs.Event
{
	public class DiscussionDto
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Message { get; set; } = string.Empty;
		public string SenderName { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public bool IsModerated { get; set; }
	}
}
