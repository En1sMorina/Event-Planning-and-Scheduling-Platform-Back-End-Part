using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EventPlanner.Application.DTOs;
using EventPlanner.Application.DTOs.Event;
using EventPlanner.Application.Interfaces;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Application.Services
{
	public class EventService : IEventService
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly INotificationService _notifService;

		public EventService(IApplicationDbContext context, IMapper mapper, INotificationService notifService)
		{
			_context = context;
			_mapper = mapper;
			_notifService = notifService;
		}
		public async Task<LocalizedEventsDto> GetAllEventsAsync(int userId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null) throw new KeyNotFoundException("User not found");

			var userLocation = user.State ?? string.Empty; 

			var allEvents = await _context.Events
				.Include(e => e.Category)
				.Include(e => e.Creator)
				.OrderByDescending(e => e.StartDate)
				.ToListAsync();

			var mappedEvents = _mapper.Map<IEnumerable<EventDto>>(allEvents);

			return new LocalizedEventsDto
			{
				SameStateEvents = mappedEvents.Where(e =>
					!string.IsNullOrEmpty(e.Location) &&
					e.Location.Equals(userLocation, StringComparison.OrdinalIgnoreCase)),

				OtherStateEvents = mappedEvents.Where(e =>
					string.IsNullOrEmpty(e.Location) ||
					!e.Location.Equals(userLocation, StringComparison.OrdinalIgnoreCase))
			};
		}

		public async Task<EventDto> GetEventByIdAsync(int id)
		{
			var evt = await _context.Events.Include(e => e.Category).Include(e => e.Creator).FirstOrDefaultAsync(e => e.Id == id);
			return _mapper.Map<EventDto>(evt);
		}

		public async Task<EventDto> CreateEventAsync(CreateEventDto dto, int creatorId)
		{
			var evt = _mapper.Map<Event>(dto);
			evt.CreatorUserId = creatorId;
			evt.IsReminderSent = false; 
			_context.Events.Add(evt);
			await _context.SaveChangesAsync();
			return _mapper.Map<EventDto>(evt);
		}

		public async Task UpdateEventAsync(int id, CreateEventDto dto, int userId, UserRole role)
		{
			var evt = await _context.Events.FindAsync(id);
			if (role != UserRole.Admin && evt.CreatorUserId != userId) throw new Exception("Unauthorized");

			_mapper.Map(dto, evt);
			evt.UpdatedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			await _notifService.NotifyAllAttendeesAsync(id,
				"Event Info Changed",
				$"The infos of the event '{evt.Title}' have been changed.",
				NotificationType.EventUpdate);
		}

		public async Task DeleteEventAsync(int id, int userId, UserRole role)
		{
			var evt = await _context.Events.FindAsync(id);
			if (role != UserRole.Admin && evt.CreatorUserId != userId) throw new Exception("Unauthorized");

			await _notifService.NotifyAllAttendeesAsync(id,
				"Event Cancelled",
				$"The event '{evt.Title}' has been cancelled.",
				NotificationType.EventCancelledByOrganizer);

			_context.Events.Remove(evt);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<EventDto>> SearchEventsAsync(string? title, string? state)
		{
			var query = _context.Events
				.Include(e => e.Category)
				.Include(e => e.Creator)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(title))
			{
				string titleNorm = title.ToLower().Replace(" ", "");
				query = query.Where(e => e.Title.ToLower().Replace(" ", "").Contains(titleNorm));
			}

			if (!string.IsNullOrWhiteSpace(state))
			{
				string stateNorm = state.ToLower().Replace(" ", "");
				if (stateNorm.Contains("kosovo") || stateNorm.Contains("kosova"))
				{
					query = query.Where(e => e.State.ToLower().Replace(" ", "").Contains("kosovo") ||
											 e.State.ToLower().Replace(" ", "").Contains("kosova"));
				}
				else
				{
					query = query.Where(e => e.State.ToLower().Replace(" ", "").Contains(stateNorm));
				}
			}

			query = query.OrderBy(e => e.StartDate);
			var events = await query.ToListAsync();
			return _mapper.Map<IEnumerable<EventDto>>(events);
		}

		public async Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerId)
		{
			var dashboard = new ManagerDashboardDto();

			var createdEvents = await _context.Events
				.Include(e => e.Category)
				.Include(e => e.Creator)
				.Where(e => e.CreatorUserId == managerId)
				.OrderByDescending(e => e.StartDate)
				.ToListAsync();

			dashboard.CreatedEvents = _mapper.Map<IEnumerable<EventDto>>(createdEvents);

			var joinedEvents = await _context.Attendees
				.Include(a => a.Event).ThenInclude(e => e.Category)
				.Include(a => a.Event).ThenInclude(e => e.Creator)
				.Where(a => a.UserId == managerId && !a.IsCancelled)
				.Select(a => a.Event)
				.OrderByDescending(e => e.StartDate)
				.ToListAsync();

			dashboard.JoinedEvents = _mapper.Map<IEnumerable<EventDto>>(joinedEvents);

			var eventsWithAttendees = await _context.Events
				.Include(e => e.Attendees)
					.ThenInclude(a => a.User)
				.Where(e => e.CreatorUserId == managerId)
				.ToListAsync();

			var attendeeDetailsList = new List<EventAttendeeSummaryDto>();

			foreach (var evt in eventsWithAttendees)
			{
				var summary = new EventAttendeeSummaryDto
				{
					EventTitle = evt.Title,
					JoinedUsers = evt.Attendees
						.Where(a => !a.IsCancelled)
						.Select(a => a.User.FullName)
						.ToList(),
					CanceledUsers = evt.Attendees
						.Where(a => a.IsCancelled)
						.Select(a => a.User.FullName)
						.ToList()
				};

				if (summary.JoinedUsers.Any() || summary.CanceledUsers.Any())
				{
					attendeeDetailsList.Add(summary);
				}
			}

			
			dashboard.AttendeeDetails = attendeeDetailsList;

			return dashboard;
		}
		public async Task<IEnumerable<EventDto>> GetEventsByTimeAsync(string timeFilter)
		{
			var query = _context.Events
				.Include(e => e.Category)
				.Include(e => e.Creator)
				.AsQueryable();

			var today = DateTime.UtcNow.Date;

			int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
			var startOfWeek = today.AddDays(-diff);

			DateTime? startDate = null;
			DateTime? endDate = null;


			switch (timeFilter?.ToLower().Replace(" ", "_"))
			{
				case "yesterday": startDate = endDate = today.AddDays(-1); break;
				case "today": startDate = endDate = today; break;
				case "tomorrow": startDate = endDate = today.AddDays(1); break;

				case "last_week": startDate = startOfWeek.AddDays(-7); endDate = startOfWeek.AddDays(-1); break;
				case "this_week": startDate = startOfWeek; endDate = startOfWeek.AddDays(6); break;
				case "next_week": startDate = startOfWeek.AddDays(7); endDate = startOfWeek.AddDays(13); break;
				case "this_weekend": startDate = startOfWeek.AddDays(5); endDate = startOfWeek.AddDays(6); break;

				case "last_month": startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-1); endDate = startDate.Value.AddMonths(1).AddDays(-1); break;
				case "this_month": startDate = new DateTime(today.Year, today.Month, 1); endDate = startDate.Value.AddMonths(1).AddDays(-1); break;
				case "next_month": startDate = new DateTime(today.Year, today.Month, 1).AddMonths(1); endDate = startDate.Value.AddMonths(1).AddDays(-1); break;

				case "last_year": startDate = new DateTime(today.Year - 1, 1, 1); endDate = new DateTime(today.Year - 1, 12, 31); break;
				case "this_year": startDate = new DateTime(today.Year, 1, 1); endDate = new DateTime(today.Year, 12, 31); break;

				default: startDate = today; break; 
			}

			if (startDate.HasValue)
				query = query.Where(e => e.StartDate.Date >= startDate.Value);

			if (endDate.HasValue)
				query = query.Where(e => e.StartDate.Date <= endDate.Value);

			var events = await query.OrderBy(e => e.StartDate).ToListAsync();
			return _mapper.Map<IEnumerable<EventDto>>(events);
		}

		public async Task<IEnumerable<AttendeeDto>> GetEventAttendeesAsync(int eventId)
		{
			var evt = await _context.Events.FindAsync(eventId);
			if (evt == null) throw new Exception("Event not found");

			return await _context.Attendees
				.Where(a => a.EventId == eventId && !a.IsCancelled)
				.Select(a => new AttendeeDto
				{
					UserId = a.UserId,
					Email = a.User.FullName,
					JoinedAt = a.JoinedAt      
				})
				.ToListAsync();
		}
	}
}