using EventPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventPlanner.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class NotificationsController : ControllerBase
	{
		private readonly INotificationService _notificationService;

		public NotificationsController(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		[HttpGet]
		public async Task<IActionResult> GetMyNotifications()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

			var userId = int.Parse(userIdClaim);

			var notifications = await _notificationService.GetUserNotificationsAsync(userId);

			return Ok(notifications);
		}

		[HttpPut("{id}/read")]
		public async Task<IActionResult> MarkAsRead(int id)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

			var userId = int.Parse(userIdClaim);

	
			await _notificationService.MarkAsReadAsync(id, userId);

			return Ok(new { message = "Notification marked as read" });
		}
	}
}