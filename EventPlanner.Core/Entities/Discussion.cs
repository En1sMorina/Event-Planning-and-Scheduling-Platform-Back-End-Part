using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlanner.Core.Entities
{
	public class Discussion
	{
		public int Id { get; set; }
		public int EventId { get; set; }
		public Event Event { get; set; }
		public int UserId { get; set; }
		public User User { get; set; }
		public string Message { get; set; } = string.Empty;
		public bool IsModerated { get; set; } = false;
		public int? ModeratedByAdminId { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
