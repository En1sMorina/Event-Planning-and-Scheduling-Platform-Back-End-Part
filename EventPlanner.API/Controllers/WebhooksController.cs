using EventPlanner.Application.Interfaces; 
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;      
using EventPlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EventPlanner.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class WebhooksController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _configuration;
		private readonly ILogger<WebhooksController> _logger;
		private readonly INotificationService _notif; 

		public WebhooksController(
			ApplicationDbContext context,
			IConfiguration configuration,
			ILogger<WebhooksController> logger,
			INotificationService notif) 
		{
			_context = context;
			_configuration = configuration;
			_logger = logger;
			_notif = notif; 
		}

		[HttpPost]
		public async Task<IActionResult> StripeWebhook()
		{
			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

			try
			{
				var stripeSignature = Request.Headers["Stripe-Signature"];
				var webhookSecret = _configuration["Stripe:WebhookSecret"];
				var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

				if (stripeEvent.Type == Stripe.EventTypes.CheckoutSessionCompleted)
				{
					_logger.LogWarning("\n\n=======================================================");
					_logger.LogWarning("[START] CHECKOUT COMPLETED CAUGHT!");

					var session = stripeEvent.Data.Object as Session;

					if (session.Metadata == null || session.Metadata.Count == 0)
					{
						_logger.LogWarning("ERROR: METADATA IS MISSING! PaymentService is running old code.");
						_logger.LogWarning("=======================================================\n");
						return Ok();
					}

					if (session.Metadata.TryGetValue("eventId", out var eventIdStr) &&
						session.Metadata.TryGetValue("userId", out var userIdStr))
					{
						int eventId = int.Parse(eventIdStr);
						int userId = int.Parse(userIdStr);
						_logger.LogWarning($"Found IDs -> Event: {eventId}, User: {userId}");

						var existing = await _context.Attendees.FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId);
						var evt = await _context.Events.FindAsync(eventId);
						bool changesMade = false;

						if (existing == null)
						{
							_logger.LogWarning("SCENARIO A: Adding brand new user to database...");
							_context.Attendees.Add(new EventAttendee { EventId = eventId, UserId = userId, JoinedAt = DateTime.UtcNow, IsCancelled = false });
							if (evt != null) evt.CurrentAttendeesCount++;
							changesMade = true;
						}
						else if (existing.IsCancelled)
						{
							_logger.LogWarning("SCENARIO B: Reactivating previously canceled user...");
							existing.IsCancelled = false;
							existing.JoinedAt = DateTime.UtcNow;
							existing.CancelledAt = null;
							if (evt != null) evt.CurrentAttendeesCount++;
							changesMade = true;
						}
						else
						{
							_logger.LogWarning("User is ALREADY active in the DB. Doing nothing.");
						}

						if (changesMade)
						{
							await _context.SaveChangesAsync();
							_logger.LogWarning("[SUCCESS] CHANGES SAVED TO DATABASE PERMANENTLY!");

						
							if (evt != null)
							{
								await _notif.CreateNotificationAsync(
									userId,
									"Payment & Reservation Completed",
									$"Your payment was successful and you are now officially registered for '{evt.Title}'.",
									NotificationType.JoinConfirmation,
									eventId
								);
								_logger.LogWarning("[SUCCESS] NOTIFICATION SENT TO USER!");
							}
						}
					}
					else
					{
						_logger.LogWarning("ERROR: Metadata exists, but the 'eventId' or 'userId' keys are missing.");
					}

					_logger.LogWarning("=======================================================\n");
				}

				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"CRITICAL ERROR: {ex.Message}");
				return Ok();
			}
		}
	}
}