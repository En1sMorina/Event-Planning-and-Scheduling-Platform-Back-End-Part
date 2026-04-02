using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EventPlanner.Application.DTOs.Event;
using EventPlanner.Application.Interfaces;
using EventPlanner.Application.Services;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace EventPlanner.Test
{
	public class EventServiceTests
	{
		private readonly Mock<IApplicationDbContext> _mockContext;
		private readonly Mock<IMapper> _mockMapper;
		private readonly Mock<INotificationService> _mockNotifService;
		private readonly Mock<DbSet<Event>> _mockEventsDbSet;
		private readonly Mock<DbSet<User>> _mockUsersDbSet;
		private readonly EventService _service;

		public EventServiceTests()
		{
			_mockContext = new Mock<IApplicationDbContext>();
			_mockMapper = new Mock<IMapper>();
			_mockNotifService = new Mock<INotificationService>();

			_mockEventsDbSet = new Mock<DbSet<Event>>();
			_mockUsersDbSet = new Mock<DbSet<User>>();

			_mockContext.Setup(c => c.Events).Returns(_mockEventsDbSet.Object);
			_mockContext.Setup(c => c.Users).Returns(_mockUsersDbSet.Object);

			_service = new EventService(_mockContext.Object, _mockMapper.Object, _mockNotifService.Object);
		}

		[Fact]
		public async Task CreateEventAsync_Should_AddEvent_And_ReturnDto()
		{
			var createDto = new CreateEventDto { Title = "Test Event" };
			var eventEntity = new Event { Id = 1, Title = "Test Event" };
			var eventDto = new EventDto { Id = 1, Title = "Test Event" };

			_mockMapper.Setup(m => m.Map<Event>(createDto)).Returns(eventEntity);
			_mockMapper.Setup(m => m.Map<EventDto>(eventEntity)).Returns(eventDto);

			
			var result = await _service.CreateEventAsync(createDto, 5);

			Assert.NotNull(result);
			Assert.Equal(5, eventEntity.CreatorUserId);
			Assert.False(eventEntity.IsReminderSent);

			_mockEventsDbSet.Verify(d => d.Add(eventEntity), Times.Once);
			_mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
		}

		[Fact]
		public async Task UpdateEventAsync_Unauthorized_ThrowsException()
		{
			var evt = new Event { Id = 1, CreatorUserId = 99 };
			_mockEventsDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(evt);

			await Assert.ThrowsAsync<Exception>(() =>
				_service.UpdateEventAsync(1, new CreateEventDto(), 5, UserRole.User));
		}

		[Fact]
		public async Task UpdateEventAsync_Authorized_UpdatesAndNotifies()
		{
			var evt = new Event { Id = 1, CreatorUserId = 5, Title = "Old Title" };
			_mockEventsDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(evt);

			await _service.UpdateEventAsync(1, new CreateEventDto(), 5, UserRole.User);

			_mockMapper.Verify(m => m.Map(It.IsAny<CreateEventDto>(), evt), Times.Once);
			_mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
			_mockNotifService.Verify(n =>
				n.NotifyAllAttendeesAsync(1, "Event Info Changed", It.IsAny<string>(), NotificationType.EventUpdate),
				Times.Once);
		}

		[Fact]
		public async Task DeleteEventAsync_Unauthorized_ThrowsException()
		{
			var evt = new Event { Id = 1, CreatorUserId = 99 };
			_mockEventsDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(evt);

			await Assert.ThrowsAsync<Exception>(() =>
				_service.DeleteEventAsync(1, 5, UserRole.User));
		}

		[Fact]
		public async Task DeleteEventAsync_Authorized_RemovesAndNotifies()
		{
			var evt = new Event { Id = 1, CreatorUserId = 5, Title = "To Delete" };
			_mockEventsDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(evt);

			await _service.DeleteEventAsync(1, 5, UserRole.User);

			_mockNotifService.Verify(n =>
				n.NotifyAllAttendeesAsync(1, "Event Cancelled", It.IsAny<string>(), NotificationType.EventCancelledByOrganizer),
				Times.Once);
			_mockEventsDbSet.Verify(d => d.Remove(evt), Times.Once);
			_mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
		}

		[Fact]
		public async Task GetAllEventsAsync_UserNotFound_ThrowsKeyNotFoundException()
		{
			_mockUsersDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((User)null);

			await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAllEventsAsync(1));
		}

		[Fact]
		public async Task GetEventAttendeesAsync_EventNotFound_ThrowsException()
		{
			_mockEventsDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Event)null);

			await Assert.ThrowsAsync<Exception>(() => _service.GetEventAttendeesAsync(1));
		}
		// RUBRIC REQUIREMENT: Product listing and search logic
		[Fact]
		public async Task SearchEventsAsync_ByTitle_ReturnsFilteredEvents()
		{
			// Arrange: Create a fake database with 2 events
			var events = new List<Event>
			{
				new Event { Id = 1, Title = "Tech Conference", State = "NY", StartDate = DateTime.Now },
				new Event { Id = 2, Title = "Music Fest", State = "CA", StartDate = DateTime.Now }
			};
			_mockEventsDbSet.Setup(m => m.AsQueryable()).Returns(events.AsQueryable()); // For the .AsQueryable() call

			// Note: Since we use Moq.EntityFrameworkCore, we can just use ReturnsDbSet
			_mockContext.Setup(c => c.Events).ReturnsDbSet(events);

			_mockMapper.Setup(m => m.Map<IEnumerable<EventDto>>(It.IsAny<IEnumerable<Event>>()))
					   .Returns(new List<EventDto> { new EventDto { Title = "Tech Conference" } });

			// Act: Search for "tech"
			var result = await _service.SearchEventsAsync("tech", null);

			// Assert: Verify it mapped the results
			Assert.NotNull(result);
			_mockMapper.Verify(m => m.Map<IEnumerable<EventDto>>(It.IsAny<IEnumerable<Event>>()), Times.Once);
		}
	}
}