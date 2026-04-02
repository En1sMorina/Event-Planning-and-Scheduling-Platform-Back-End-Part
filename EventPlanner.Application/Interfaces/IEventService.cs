using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventPlanner.Application.DTOs;
using EventPlanner.Application.DTOs.Event;
using EventPlanner.Core.Enums;
namespace EventPlanner.Application.Interfaces
{
	public interface IEventService
	{
		Task<LocalizedEventsDto> GetAllEventsAsync(int userId); 
		Task<EventDto> GetEventByIdAsync(int id);
		Task<EventDto> CreateEventAsync(CreateEventDto dto, int creatorId);
		Task UpdateEventAsync(int id, CreateEventDto dto, int userId, UserRole role);
		Task DeleteEventAsync(int id, int userId, UserRole role);
		Task<IEnumerable<EventDto>> SearchEventsAsync(string? title, string? state);
		Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerId);
		Task<IEnumerable<EventDto>> GetEventsByTimeAsync(string timeFilter);
		Task<IEnumerable<AttendeeDto>> GetEventAttendeesAsync(int eventId);
	}
}