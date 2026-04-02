using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventPlanner.Core.Enums;
namespace EventPlanner.Core.Entities
{
	public class Notification
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public NotificationType Type { get; set; }
		public bool IsRead { get; set; } = false;
		public int? RelatedEventId { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
