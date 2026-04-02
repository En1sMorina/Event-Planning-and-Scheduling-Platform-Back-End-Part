using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EventPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventPlanner.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class UserController : ControllerBase
	{
		private readonly IUserService _service;
		public UserController(IUserService service) => _service = service;

		[HttpGet("my-events")]
		public async Task<IActionResult> GetMyJoinedEvents()
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

			
			var events = await _service.GetEventsUserJoinedAsync(userId);

		
			return Ok(events);
		}

		[HttpGet("canceled-events")]
		public async Task<IActionResult> GetMyCanceledEvents()
		{
			var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized("User ID not found in token.");
			}

			var events = await _service.GetEventsUserCanceledAsync(userId);

			return Ok(events);
		}
	}
}