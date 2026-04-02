using Microsoft.AspNetCore.Http;
using EventPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventPlanner.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class DiscussionsController : ControllerBase
	{
		private readonly IDiscussionService _discussionService;

		public DiscussionsController(IDiscussionService discussionService)
		{
			_discussionService = discussionService;
		}

		[HttpGet("{eventId}")]
		public async Task<IActionResult> GetDiscussions(int eventId)
		{
			var discussions = await _discussionService.GetDiscussionsAsync(eventId);
			return Ok(discussions);
		}

		[HttpPost("{eventId}")]
		public async Task<IActionResult> PostMessage(int eventId, [FromBody] string message)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

			await _discussionService.PostMessageAsync(eventId, userId, message);
			return Ok(new { message = "Message posted! Attendees have been notified." });
		}

		[HttpPut("{messageId}")]
		public async Task<IActionResult> EditMessage(int messageId, [FromBody] string newMessage)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

			try
			{
				await _discussionService.EditMessageAsync(messageId, userId, newMessage);
				return Ok(new { message = "Message updated successfully." });
			}
			catch (Exception ex) { return BadRequest(ex.Message); }
		}

		[HttpDelete("{messageId}")]
		public async Task<IActionResult> DeleteMessage(int messageId)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

			try
			{
				await _discussionService.DeleteMessageAsync(messageId, userId);
				return Ok(new { message = "Message deleted successfully." });
			}
			catch (Exception ex) { return BadRequest(ex.Message); }
		}
	
		[HttpPut("{messageId}/moderate")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ModerateMessage(int messageId)
		{
			var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(adminIdClaim, out int adminId)) return Unauthorized();

			await _discussionService.ModerateMessageAsync(messageId, adminId);
			return Ok(new { message = "Message moderated." });
		}
	}
}