using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventPlanner.Application.DTOs.Event;

namespace EventPlanner.Application.Interfaces
{
	public interface IUserService
	{
		Task<string> JoinEventAsync(int eventId, int userId); 
		Task CancelAttendanceAsync(int eventId, int userId);
		Task<IEnumerable<string>> GetAttendeesAsync(int eventId, int requesterId, string role);
		Task<IEnumerable<EventDto>> GetEventsUserJoinedAsync(int userId);
		Task<IEnumerable<EventDto>> GetEventsUserCanceledAsync(int userId);
	}
}