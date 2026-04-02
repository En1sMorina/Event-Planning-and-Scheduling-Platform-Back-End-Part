using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlanner.Application.Interfaces
{
	public interface IPaymentService
	{
		Task<string> CreateCheckoutSessionAsync(int eventId, int userId, string eventTitle, decimal price);
	}
}
