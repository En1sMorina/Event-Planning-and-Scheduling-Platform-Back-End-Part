using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EventPlanner.Application.DTOs.Event;
using EventPlanner.Application.Interfaces;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Application.Services
{
	public class UserService : IUserService
	{
		private readonly IApplicationDbContext _context;
		private readonly INotificationService _notif;
		private readonly IPaymentService _paymentService; 
		private readonly IMapper _mapper;

		public UserService(IApplicationDbContext context, INotificationService notif, IMapper mapper, IPaymentService paymentService)
		{

			_context = context;
			_notif = notif;
			_mapper = mapper;
			_paymentService = paymentService;
		}

		public async Task<string> JoinEventAsync(int eventId, int userId)
		{
			var evt = await _context.Events.FindAsync(eventId);
			if (evt == null) throw new Exception("Event not found");

			if (evt.CurrentAttendeesCount >= evt.CapacityLimit) throw new Exception("Full");

			var existing = await _context.Attendees.FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId);
			if (existing != null && !existing.IsCancelled) throw new Exception("Already joined");


			if (evt.Price > 0 && evt.CreatorUserId != userId)
			{
				string checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(eventId, userId, evt.Title, evt.Price);

				return checkoutUrl; 
			}

			if (existing != null)
			{
				existing.IsCancelled = false;
				existing.JoinedAt = DateTime.UtcNow;
				existing.CancelledAt = null;
			}
			else
			{
				_context.Attendees.Add(new EventAttendee { EventId = eventId, UserId = userId });
			}

			evt.CurrentAttendeesCount++;
			await _context.SaveChangesAsync();

			await _notif.CreateNotificationAsync(
				userId,
				"Reservation Completed",
				$"You have been accepted in the event '{evt.Title}'. Your reservation is now complete.",
				NotificationType.JoinConfirmation,
				eventId
			);

			return "Joined"; 
		}

		public async Task CancelAttendanceAsync(int eventId, int userId)
		{
			var att = await _context.Attendees
				.Include(a => a.Event)
				.FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId);

			if (att == null || att.IsCancelled) throw new Exception("Not joined");

			att.IsCancelled = true;
			att.CancelledAt = DateTime.UtcNow;
			att.Event.CurrentAttendeesCount--;

			await _context.SaveChangesAsync();

			await _notif.CreateNotificationAsync(
				userId,
				"Cancelled",
				$"You have successfully cancelled your reservation for {att.Event.Title}",
				NotificationType.CancellationNotice,
				eventId
			);
		}

		public async Task<IEnumerable<string>> GetAttendeesAsync(int eventId, int reqId, string role)
		{
			var evt = await _context.Events.FindAsync(eventId);
			if (role != "Admin" && evt.CreatorUserId != reqId) throw new Exception("Unauthorized");
			return await _context.Attendees
				.Where(a => a.EventId == eventId && !a.IsCancelled)
				.Select(a => a.User.FullName)
				.ToListAsync();
		}

		public async Task<IEnumerable<EventDto>> GetEventsUserJoinedAsync(int userId)
		{
			var attendedEvents = await _context.Attendees
				.Include(a => a.Event)             
					.ThenInclude(e => e.Category)  
				.Include(a => a.Event)
					.ThenInclude(e => e.Creator)  
				.Where(a => a.UserId == userId && !a.IsCancelled)
				.OrderByDescending(a => a.JoinedAt) 
				.Select(a => a.Event)              
				.ToListAsync();

			return _mapper.Map<IEnumerable<EventDto>>(attendedEvents);
		}

		public async Task<IEnumerable<EventDto>> GetEventsUserCanceledAsync(int userId)
		{
			var canceledEvents = await _context.Attendees
				.Include(a => a.Event)
					.ThenInclude(e => e.Category)
				.Include(a => a.Event)
					.ThenInclude(e => e.Creator)
				.Where(a => a.UserId == userId && a.IsCancelled)
				.OrderByDescending(a => a.JoinedAt)
				.Select(a => a.Event)
				.ToListAsync();

			return _mapper.Map<IEnumerable<EventDto>>(canceledEvents);
		}
	}
}