using System.Security.Claims;
using EventPlanner.Application.DTOs.Event;
using EventPlanner.Application.Interfaces;
using EventPlanner.Application.Services;
using EventPlanner.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanner.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EventsController : ControllerBase
	{
		private readonly IEventService _service;
		public EventsController(IEventService service) => _service = service;
		
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Get()
		{
			var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized("User ID not found or invalid.");
			}

			return Ok(await _service.GetAllEventsAsync(userId));
		}

		[AllowAnonymous] 
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{ 
			var eventDetails = await _service.GetEventByIdAsync(id);

			if (eventDetails == null)
			{
				return NotFound("Event not found.");
			}

			return Ok(eventDetails);
		}

		[HttpPost]
		[Authorize(Roles = "EventManager,Admin")]
		public async Task<IActionResult> Create(CreateEventDto dto)
		{
			var uid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			return Ok(await _service.CreateEventAsync(dto, uid));
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "EventManager,Admin")]
		public async Task<IActionResult> Update(int id, CreateEventDto dto)
		{
			var uid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			var role = Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role).Value);
			await _service.UpdateEventAsync(id, dto, uid, role);
			return NoContent();
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "EventManager,Admin")]
		public async Task<IActionResult> Delete(int id)
		{
			var uid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			var role = Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role).Value);
			await _service.DeleteEventAsync(id, uid, role);
			return NoContent();
		}
		
		[HttpGet("search")]
		[AllowAnonymous]
		public async Task<IActionResult> Search([FromQuery] string? title, [FromQuery] string? state)
		{
			return Ok(await _service.SearchEventsAsync(title, state));
		}

		[HttpGet("filter")]
		[AllowAnonymous]
		public async Task<IActionResult> GetEventsByTime([FromQuery] string time)
		{
			var events = await _service.GetEventsByTimeAsync(time);
			return Ok(events);
		}

	}
}