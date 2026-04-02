using EventPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventPlanner.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "EventManager")]
	public class EventManagersController : ControllerBase
	{
		private readonly IEventService _eventService;

		public EventManagersController(IEventService eventService)
		{
			_eventService = eventService;
		}

		[HttpGet("created-events")]
		public async Task<IActionResult> GetCreatedEvents()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
				return Unauthorized("User ID not found in token.");

			var dashboard = await _eventService.GetManagerDashboardAsync(userId);

			return Ok(new
			{
				CreatedEvents = dashboard.CreatedEvents,
				AttendeeDetails = dashboard.AttendeeDetails
			});
		}

		[HttpGet("joined-events")]
		public async Task<IActionResult> GetJoinedEvents()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
				return Unauthorized("User ID not found in token.");

			var dashboard = await _eventService.GetManagerDashboardAsync(userId);

			return Ok(dashboard.JoinedEvents);
		}
	}
}