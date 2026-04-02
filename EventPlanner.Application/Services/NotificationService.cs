using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EventPlanner.Application.DTOs.Notification;
using EventPlanner.Application.Interfaces;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Application.Services
{
	public class NotificationService : INotificationService
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;

		public NotificationService(IApplicationDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, int? eventId = null)
		{
			var notif = new Notification { UserId = userId, Title = title, Message = message, Type = type, RelatedEventId = eventId };
			_context.Notifications.Add(notif);
			await _context.SaveChangesAsync();
		}

		public async Task NotifyAllAttendeesAsync(int eventId, string title, string message, NotificationType type)
		{
			var attendees = await _context.Attendees
				.Where(a => a.EventId == eventId && !a.IsCancelled)
				.Select(a => a.UserId).ToListAsync();

			if (attendees.Any())
			{
				var notifications = attendees.Select(uid => new Notification { UserId = uid, Title = title, Message = message, Type = type, RelatedEventId = eventId });
				await _context.Notifications.AddRangeAsync(notifications);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId)
		{
			var notifs = await _context.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync();
			return _mapper.Map<IEnumerable<NotificationDto>>(notifs);
		}

		public async Task MarkAsReadAsync(int notificationId, int userId)
		{
			var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
			if (notif != null) { notif.IsRead = true; await _context.SaveChangesAsync(); }
		}
	}
}
