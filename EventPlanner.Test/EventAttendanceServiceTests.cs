using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.EntityFrameworkCore;
using AutoMapper; // Added AutoMapper
using EventPlanner.Application.Services;
using EventPlanner.Application.Interfaces;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;

namespace EventPlanner.Test
{
	public class EventAttendanceServiceTests
	{
		private readonly Mock<IApplicationDbContext> _mockContext;
		private readonly Mock<IPaymentService> _mockPayment;
		private readonly Mock<INotificationService> _mockNotif;
		private readonly Mock<IMapper> _mockMapper; // Added Mapper Mock
		private readonly UserService _service;

		public EventAttendanceServiceTests()
		{
			// Initialize all 4 fake tools
			_mockContext = new Mock<IApplicationDbContext>();
			_mockPayment = new Mock<IPaymentService>();
			_mockNotif = new Mock<INotificationService>();
			_mockMapper = new Mock<IMapper>();

			// Pass all 4 tools in the exact order UserService expects them
			_service = new UserService(
				_mockContext.Object,
				_mockNotif.Object,
				_mockMapper.Object,
				_mockPayment.Object
			);
		}

		// RUBRIC REQUIREMENT: Validation rules (missing data)
		[Fact]
		public async Task JoinEventAsync_EventNotFound_ThrowsException()
		{
			_mockContext.Setup(c => c.Events.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Event)null);

			var ex = await Assert.ThrowsAsync<Exception>(() => _service.JoinEventAsync(1, 1));
			Assert.Equal("Event not found", ex.Message);
		}

		// RUBRIC REQUIREMENT: Validation rules (invalid quantity / sold out)
		[Fact]
		public async Task JoinEventAsync_EventFull_ThrowsException()
		{
			var evt = new Event { Id = 1, CapacityLimit = 10, CurrentAttendeesCount = 10 }; // Event is full!
			_mockContext.Setup(c => c.Events.FindAsync(It.IsAny<object[]>())).ReturnsAsync(evt);

			var ex = await Assert.ThrowsAsync<Exception>(() => _service.JoinEventAsync(1, 1));
			Assert.Equal("Full", ex.Message);
		}

		// RUBRIC REQUIREMENT: Validation rules (Duplicate order)
		[Fact]
		public async Task JoinEventAsync_AlreadyJoined_ThrowsException()
		{
			var evt = new Event { Id = 1, CapacityLimit = 10, CurrentAttendeesCount = 5 };
			_mockContext.Setup(c => c.Events.FindAsync(It.IsAny<object[]>())).ReturnsAsync(evt);

			var attendees = new List<EventAttendee> { new EventAttendee { EventId = 1, UserId = 1, IsCancelled = false } };
			_mockContext.Setup(c => c.Attendees).ReturnsDbSet(attendees);

			var ex = await Assert.ThrowsAsync<Exception>(() => _service.JoinEventAsync(1, 1));
			Assert.Equal("Already joined", ex.Message);
		}

		// RUBRIC REQUIREMENT: Order creation flow (Paid)
		[Fact]
		public async Task JoinEventAsync_PaidEvent_ReturnsCheckoutUrl()
		{
			var evt = new Event { Id = 1, CapacityLimit = 10, CurrentAttendeesCount = 5, Price = 50, CreatorUserId = 2, Title = "Paid Event" };
			_mockContext.Setup(c => c.Events.FindAsync(It.IsAny<object[]>())).ReturnsAsync(evt);
			_mockContext.Setup(c => c.Attendees).ReturnsDbSet(new List<EventAttendee>());

			// Mock the payment gateway URL generation
			_mockPayment.Setup(p => p.CreateCheckoutSessionAsync(1, 1, "Paid Event", 50))
						.ReturnsAsync("http://stripe.checkout.url");

			var result = await _service.JoinEventAsync(1, 1);

			Assert.Equal("http://stripe.checkout.url", result);
		}

		// RUBRIC REQUIREMENT: Order creation flow (Free)
		[Fact]
		public async Task JoinEventAsync_FreeEvent_SuccessfullyJoinsAndNotifies()
		{
			var evt = new Event { Id = 1, CapacityLimit = 10, CurrentAttendeesCount = 5, Price = 0, Title = "Free Event" };
			_mockContext.Setup(c => c.Events.FindAsync(It.IsAny<object[]>())).ReturnsAsync(evt);
			_mockContext.Setup(c => c.Attendees).ReturnsDbSet(new List<EventAttendee>());

			var result = await _service.JoinEventAsync(1, 1);

			Assert.Equal("Joined", result);
			Assert.Equal(6, evt.CurrentAttendeesCount); // Verify capacity went up
			_mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once); // Verify DB saved
			_mockNotif.Verify(n => n.CreateNotificationAsync(1, "Reservation Completed", It.IsAny<string>(), NotificationType.JoinConfirmation, 1), Times.Once); // Verify confirmation email/notification sent
		}
	}
}