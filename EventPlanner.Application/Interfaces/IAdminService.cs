using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventPlanner.Application.DTOs.Event;
using EventPlanner.Application.DTOs.Auth;
using EventPlanner.Core.Entities;

namespace EventPlanner.Application.Interfaces
{
	public interface IAdminService
	{
	
		Task<IEnumerable<EventDto>> GetAllEventsAsync(string? status);
		Task<IEnumerable<UserDto>> GetAllUsersAsync(string role);
		Task<IEnumerable<EventDto>> GetDailyEventsAsync(DateTime date);

		Task<AdminManagerDetailsDto> GetManagerDetailsAsync(int managerId);
		Task<AdminUserDetailsDto> GetUserDetailsAsync(int userId);
		Task<IEnumerable<AdminEventStatsDto>> GetEventStatisticsAsync(int? eventId);
	}
}