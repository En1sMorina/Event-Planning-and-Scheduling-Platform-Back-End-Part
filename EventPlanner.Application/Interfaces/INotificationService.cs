using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventPlanner.Application.DTOs.Notification;
using EventPlanner.Core.Enums;
namespace EventPlanner.Application.Interfaces
{
	public interface INotificationService
	{
		Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, int? eventId = null);
		Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);
		Task MarkAsReadAsync(int notificationId, int userId);
		Task NotifyAllAttendeesAsync(int eventId, string title, string message, NotificationType type);
	}
}
