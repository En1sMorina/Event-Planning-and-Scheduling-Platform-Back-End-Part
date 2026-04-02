using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using EventPlanner.Application.DTOs.Event; // 👈 Required so it knows what DiscussionDto is

namespace EventPlanner.Application.Interfaces
{
	public interface IDiscussionService
	{
		Task PostMessageAsync(int eventId, int userId, string message);

		Task<IEnumerable<DiscussionDto>> GetDiscussionsAsync(int eventId);

		Task EditMessageAsync(int messageId, int userId, string newMessage);

		Task DeleteMessageAsync(int messageId, int userId);

		Task ModerateMessageAsync(int messageId, int adminId);
	}
}