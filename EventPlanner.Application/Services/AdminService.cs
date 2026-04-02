using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EventPlanner.Application.DTOs.Event; 
using EventPlanner.Application.DTOs.Auth;
using EventPlanner.Application.Interfaces;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums; 
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Application.Services
{
	public class AdminService : IAdminService
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;

		public AdminService(IApplicationDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}
		public async Task<IEnumerable<EventDto>> GetAllEventsAsync(string? status)
		{
			var query = _context.Events
				.Include(e => e.Category)
				.Include(e => e.Creator)
				.AsQueryable();

			if (!string.IsNullOrEmpty(status))
			{
				var allEvents = await query.ToListAsync();
				var mappedEvents = _mapper.Map<IEnumerable<EventDto>>(allEvents);
				return mappedEvents.Where(e => e.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
			}

			return _mapper.Map<IEnumerable<EventDto>>(await query.ToListAsync());
		}

		public async Task<IEnumerable<UserDto>> GetAllUsersAsync(string role)
		{
			var query = _context.Users.AsQueryable();

			if (!string.IsNullOrWhiteSpace(role))
			{
				var searchRole = role.Trim().ToLower();

				if (searchRole == "user")
				{
					query = query.Where(u => u.Role == UserRole.User);
				}
				else if (searchRole == "organizer" || searchRole == "manager" || searchRole == "eventmanager")
				{
					query = query.Where(u => u.Role == UserRole.EventManager);
				}
	
				else
				{
					return new List<UserDto>();
				}
			}

			var users = await query.ToListAsync();

			return _mapper.Map<IEnumerable<UserDto>>(users);
		}

		public async Task<IEnumerable<EventDto>> GetDailyEventsAsync(DateTime date)
		{
			var events = await _context.Events
				.Include(e => e.Category)
				.Include(e => e.Creator)
				.Where(e => e.StartDate.Date <= date.Date && e.EndDate.Date >= date.Date)
				.ToListAsync();

			return _mapper.Map<IEnumerable<EventDto>>(events);
		}

		public async Task<AdminManagerDetailsDto> GetManagerDetailsAsync(int managerId)
		{
			var manager = await _context.Users.FindAsync(managerId);

			if (manager == null)
				throw new KeyNotFoundException("User not found");

			if (manager.Role != UserRole.EventManager)
			{
				throw new Exception($"User '{manager.FullName}' (ID: {managerId}) is a simple '{manager.Role}', not a Manager.");
			}

			var created = await _context.Events
				.Include(e => e.Category).Include(e => e.Creator)
				.Where(e => e.CreatorUserId == managerId).ToListAsync();

			var joined = await _context.Attendees
				.Include(a => a.Event).ThenInclude(e => e.Category)
				.Include(a => a.Event).ThenInclude(e => e.Creator)
				.Where(a => a.UserId == managerId && !a.IsCancelled)
				.Select(a => a.Event).ToListAsync();

			return new AdminManagerDetailsDto
			{
				ManagerId = manager.Id,
				FullName = manager.FullName,
				CreatedEvents = _mapper.Map<IEnumerable<EventDto>>(created),
				JoinedEvents = _mapper.Map<IEnumerable<EventDto>>(joined)
			};
		}

		public async Task<AdminUserDetailsDto> GetUserDetailsAsync(int userId)
		{
			var user = await _context.Users.FindAsync(userId);

			if (user == null)
				throw new KeyNotFoundException("User not found");

			if (user.Role != UserRole.User)
			{
				throw new Exception($"User '{user.FullName}' (ID: {userId}) is a '{user.Role}', not a simple User.");
			}

			var allAttendance = await _context.Attendees
				.Include(a => a.Event).ThenInclude(e => e.Category)
				.Include(a => a.Event).ThenInclude(e => e.Creator)
				.Where(a => a.UserId == userId)
				.ToListAsync();

			return new AdminUserDetailsDto
			{
				UserId = user.Id,
				FullName = user.FullName,
				JoinedEvents = _mapper.Map<IEnumerable<EventDto>>(allAttendance.Where(a => !a.IsCancelled).Select(a => a.Event)),
				CancelledEvents = _mapper.Map<IEnumerable<EventDto>>(allAttendance.Where(a => a.IsCancelled).Select(a => a.Event))
			};
		}

		public async Task<IEnumerable<AdminEventStatsDto>> GetEventStatisticsAsync(int? eventId)
		{
			var query = _context.Events
				.Include(e => e.Category)
				.Include(e => e.Creator)
				.Include(e => e.Attendees) 
				.AsQueryable();

			if (eventId.HasValue)
			{
				query = query.Where(e => e.Id == eventId.Value);
			}

			var events = await query.ToListAsync();

			var statsList = new List<AdminEventStatsDto>();

			foreach (var evt in events)
			{
				var activeAttendees = evt.Attendees.Count(a => !a.IsCancelled);
				var cancelledAttendees = evt.Attendees.Count(a => a.IsCancelled);

				statsList.Add(new AdminEventStatsDto
				{
					EventDetails = _mapper.Map<EventDto>(evt),
					AttendeesCount = activeAttendees,
					Capacity = evt.CapacityLimit,
					CancellationsCount = cancelledAttendees,
					OccupancyRate = evt.CapacityLimit > 0
						? (double)activeAttendees / evt.CapacityLimit * 100
						: 0
				});
			}
			return statsList;
		}
		
	}
}