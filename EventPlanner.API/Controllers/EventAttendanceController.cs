using Microsoft.AspNetCore.Http;
using EventPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventPlanner.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class EventAttendanceController : ControllerBase
	{
		private readonly IUserService _attendanceService;
		private readonly IEventService _eventService;
		public EventAttendanceController(IUserService attendanceService, IEventService eventService)
		{
			_attendanceService = attendanceService;
			_eventService = eventService;
		}

		[HttpPost("{id}/join")]
		public async Task<IActionResult> JoinEvent(int id)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

			var result = await _attendanceService.JoinEventAsync(id, userId);
			if (result == "Joined")
			{
				return Ok(new { message = "Successfully joined the free event." });
			}

			return Ok(new { checkoutUrl = result });
		}

		[HttpPost("{id}/cancel")]
		public async Task<IActionResult> CancelAttendance(int id)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
				return Unauthorized("User ID not found in token.");

			await _attendanceService.CancelAttendanceAsync(id, userId);

			return Ok(new { message = "Successfully canceled attendance." });
		}

		
		[HttpGet("{eventId}/attendees")]
		public async Task<IActionResult> GetAttendees(int eventId)
		{
			var attendees = await _eventService.GetEventAttendeesAsync(eventId);
			return Ok(attendees);
		}
	}
}