using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventPlanner.Application.Interfaces;
using Stripe.Checkout;

namespace EventPlanner.Application.Services
{
	public class PaymentService : IPaymentService
	{
		public async Task<string> CreateCheckoutSessionAsync(int eventId, int userId, string eventTitle, decimal price)
		{
			var domain = "http://localhost:3000";

			var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string> { "card" },
				LineItems = new List<SessionLineItemOptions>
				{
					new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmountDecimal = price * 100,
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = $"Ticket for {eventTitle}",
							},
						},
						Quantity = 1,
					},
				},
				Mode = "payment",

				Metadata = new Dictionary<string, string>
				{
					{ "eventId", eventId.ToString() },
					{ "userId", userId.ToString() }
				},

				SuccessUrl = domain + $"/event/{eventId}?success=true",
				CancelUrl = domain + $"/event/{eventId}?canceled=true",
			};

			var service = new SessionService();
			Session session = await service.CreateAsync(options);

			return session.Url; 
		}
	}
}