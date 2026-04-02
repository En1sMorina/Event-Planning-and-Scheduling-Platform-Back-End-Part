using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EventPlanner.Application.Interfaces;
using EventPlanner.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanner.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin")] 
	public class AdminController : ControllerBase
	{
		private readonly IAdminService _adminService;

		public AdminController(IAdminService adminService)
		{
			_adminService = adminService;
		}

	
		[HttpGet("events")]
		public async Task<IActionResult> GetAllEvents([FromQuery] string? status)
		{
			return Ok(await _adminService.GetAllEventsAsync(status));
		}

		[HttpGet("events/daily")]
		public async Task<IActionResult> GetDailyEvents([FromQuery] DateTime? date)
		{
			var queryDate = date ?? DateTime.UtcNow;
			return Ok(await _adminService.GetDailyEventsAsync(queryDate));
		}

		[HttpGet("users")]
		public async Task<IActionResult> GetAllUsers([FromQuery] string role = null)
		{
			return Ok(await _adminService.GetAllUsersAsync(role));
		}

		[HttpGet("managers/{id}")]
		public async Task<IActionResult> GetManagerDetails(int id)
		{
			return Ok(await _adminService.GetManagerDetailsAsync(id));
		}


		[HttpGet("users/{id}")]
		public async Task<IActionResult> GetUserDetails(int id)
		{
			return Ok(await _adminService.GetUserDetailsAsync(id));
		}

		[HttpGet("events/stats")]
		public async Task<IActionResult> GetEventStats([FromQuery] int? id = null)
		{
			var stats = await _adminService.GetEventStatisticsAsync(id);

			if (id.HasValue && !stats.Any())
			{
				return NotFound("Event not found");
			}

			return Ok(stats);
		}
	}
}