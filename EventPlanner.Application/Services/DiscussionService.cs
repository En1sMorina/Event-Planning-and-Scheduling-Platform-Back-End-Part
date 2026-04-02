using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventPlanner.Application.DTOs.Event;
using EventPlanner.Application.Interfaces;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Application.Services
{
	public class DiscussionService : IDiscussionService
	{
		private readonly IApplicationDbContext _context;
		private readonly INotificationService _notif;

		public DiscussionService(IApplicationDbContext context, INotificationService notif)
		{
			_context = context;
			_notif = notif;
		}

		public async Task PostMessageAsync(int eventId, int userId, string message)
		{
			var evt = await _context.Events
				.Include(e => e.Attendees)
				.FirstOrDefaultAsync(e => e.Id == eventId);

			if (evt == null) throw new KeyNotFoundException("Event not found");

			var discussion = new Discussion { EventId = eventId, UserId = userId, Message = message, CreatedAt = DateTime.UtcNow };
			_context.Discussions.Add(discussion);
			await _context.SaveChangesAsync();

			var userIdsToNotify = evt.Attendees
				.Where(a => !a.IsCancelled && a.UserId != userId)
				.Select(a => a.UserId)
				.ToList();

			if (evt.CreatorUserId != userId && !userIdsToNotify.Contains(evt.CreatorUserId))
			{
				userIdsToNotify.Add(evt.CreatorUserId);
			}

			foreach (var idToNotify in userIdsToNotify)
			{
				await _notif.CreateNotificationAsync(
					idToNotify,
					$"New message in {evt.Title}",
					"Someone posted a new comment in the discussion.",
					NotificationType.NewDiscussion,
					eventId);
			}
		}

		public async Task<IEnumerable<DiscussionDto>> GetDiscussionsAsync(int eventId)
		{
			return await _context.Discussions
				.Where(d => d.EventId == eventId)
				.Select(d => new DiscussionDto
				{
					Id = d.Id,
					UserId = d.UserId,
					Message = d.Message,
					SenderName = d.User.FullName,
					CreatedAt = d.CreatedAt,
					IsModerated = d.IsModerated
				})
				.OrderBy(d => d.CreatedAt) 
				.ToListAsync();
		}

		public async Task EditMessageAsync(int messageId, int userId, string newMessage)
		{
			var d = await _context.Discussions.FindAsync(messageId);
			if (d == null) throw new KeyNotFoundException("Message not found.");

			if (d.UserId != userId) throw new UnauthorizedAccessException("You can only edit your own messages.");
			if (d.IsModerated) throw new InvalidOperationException("You cannot edit a message that an Admin has moderated.");

			d.Message = newMessage;
			await _context.SaveChangesAsync();
		}
		public async Task DeleteMessageAsync(int messageId, int userId)
		{
			var d = await _context.Discussions.FindAsync(messageId);
			if (d != null)
			{
				if (d.UserId != userId) throw new UnauthorizedAccessException("You can only delete your own messages.");

				_context.Discussions.Remove(d);
				await _context.SaveChangesAsync();
			}
		}
		public async Task ModerateMessageAsync(int messageId, int adminId)
		{
			var d = await _context.Discussions.FindAsync(messageId);
			if (d != null)
			{
				d.IsModerated = true;
				d.Message = "[This message was removed by an Administrator]";
				await _context.SaveChangesAsync();
			}
		}
	}
}