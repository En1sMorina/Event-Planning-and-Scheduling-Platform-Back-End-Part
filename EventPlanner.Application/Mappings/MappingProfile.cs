using System;
using AutoMapper;
using EventPlanner.Application.DTOs.Auth;
using EventPlanner.Application.DTOs.Event;
using EventPlanner.Application.DTOs.Notification;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;

namespace EventPlanner.Application.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<RegisterDto, User>();
			CreateMap<CreateEventDto, Event>();

			CreateMap<Event, EventDto>()
				.ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.State))
				.ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
				.ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator.FullName))

				.ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
					
					src.Status == EventStatus.Cancelled ? "Cancelled" :

					
					DateTime.UtcNow > src.EndDate ? "Completed" :


					DateTime.UtcNow >= src.StartDate && DateTime.UtcNow <= src.EndDate ? "Ongoing" :

					
					"Upcoming"
				));


			CreateMap<Notification, NotificationDto>();
			CreateMap<User, UserDto>();
		}
	}
}