using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.EntityFrameworkCore; 
using Microsoft.Extensions.Configuration;
using EventPlanner.Application.Services;
using EventPlanner.Application.DTOs.Auth;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;
using EventPlanner.Application.Interfaces;

namespace EventPlanner.Test
{
	public class AuthServiceTests
	{
		private readonly Mock<IApplicationDbContext> _mockContext;
		private readonly Mock<IConfiguration> _mockConfig;
		private readonly AuthService _service;

		public AuthServiceTests()
		{
			_mockContext = new Mock<IApplicationDbContext>();
			_mockConfig = new Mock<IConfiguration>();

			_mockConfig.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyThatIsAtLeast32BytesLongForHmacSha512!");
			_mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
			_mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

			_service = new AuthService(_mockContext.Object, _mockConfig.Object);
		}

		[Fact]
		public async Task RegisterAsync_AdminRole_ThrowsException()
		{
			var dto = new RegisterDto { Role = UserRole.Admin };
			await Assert.ThrowsAsync<Exception>(() => _service.RegisterAsync(dto));
		}

		[Fact]
		public async Task RegisterAsync_EmailExists_ThrowsException()
		{
			// Setup a fake DB with an existing user
			var users = new List<User> { new User { Email = "test@test.com" } };
			_mockContext.Setup(c => c.Users).ReturnsDbSet(users);

			var dto = new RegisterDto { Email = "test@test.com", Role = UserRole.User };
			var ex = await Assert.ThrowsAsync<Exception>(() => _service.RegisterAsync(dto));
			Assert.Equal("Email exists.", ex.Message);
		}

		[Fact]
		public async Task RegisterAsync_UsernameExists_ThrowsException()
		{
			var users = new List<User> { new User { Username = "johndoe" } };
			_mockContext.Setup(c => c.Users).ReturnsDbSet(users);

			var dto = new RegisterDto { Email = "new@test.com", Username = "johndoe", Role = UserRole.User };
			var ex = await Assert.ThrowsAsync<Exception>(() => _service.RegisterAsync(dto));
			Assert.Equal("Username taken.", ex.Message);
		}

		[Fact]
		public async Task RegisterAsync_ValidData_SavesUserAndReturnsToken()
		{
			var users = new List<User>(); // Empty DB
			_mockContext.Setup(c => c.Users).ReturnsDbSet(users);

			var dto = new RegisterDto
			{
				FullName = "John Doe",
				Email = "new@test.com",
				Username = "johndoe",
				Password = "Password123!",
				Role = UserRole.User,
				State = "KS"
			};

			var result = await _service.RegisterAsync(dto);

			Assert.NotNull(result);
			Assert.NotNull(result.Token);
			_mockContext.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Once);
			_mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
		}

		[Fact]
		public async Task LoginAsync_InvalidEmail_ThrowsException()
		{
			var users = new List<User>(); 
			_mockContext.Setup(c => c.Users).ReturnsDbSet(users);

			var dto = new LoginDto { Email = "wrong@test.com", Password = "password" };
			await Assert.ThrowsAsync<Exception>(() => _service.LoginAsync(dto));
		}

		[Fact]
		public async Task LoginAsync_InvalidPassword_ThrowsException()
		{
			var users = new List<User>
			{
				new User { Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealPassword") }
			};
			_mockContext.Setup(c => c.Users).ReturnsDbSet(users);

			var dto = new LoginDto { Email = "test@test.com", Password = "WrongPassword" };
			var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoginAsync(dto));
			Assert.Equal("Invalid credentials.", ex.Message);
		}

		[Fact]
		public async Task LoginAsync_ValidCredentials_ReturnsToken()
		{
			var users = new List<User>
			{
				new User
				{
					Id = 1,
					Email = "test@test.com",
					PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealPassword"),
					Role = UserRole.User,
					State = "KS",
					IsActive = true 
                }
			};
			_mockContext.Setup(c => c.Users).ReturnsDbSet(users);

			var dto = new LoginDto { Email = "test@test.com", Password = "RealPassword" };
			var result = await _service.LoginAsync(dto);

			Assert.NotNull(result);
			Assert.NotNull(result.Token);
		}

		[Fact]
		public async Task ForgotPasswordAsync_UserExists_SetsTokenAndReturnsIt()
		{
			var users = new List<User> { new User { Email = "test@test.com" } };
			_mockContext.Setup(c => c.Users).ReturnsDbSet(users);

			var result = await _service.ForgotPasswordAsync("test@test.com");

			Assert.NotNull(result);
			_mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
		}

		[Fact]
		public async Task ResetPasswordAsync_ValidToken_ResetsPassword()
		{
			var users = new List<User>
			{
				new User
				{
					PasswordResetToken = "ValidToken",
					ResetTokenExpires = DateTime.UtcNow.AddMinutes(30)
				}
			};
			_mockContext.Setup(c => c.Users).ReturnsDbSet(users);

			var dto = new ResetPasswordDto { Token = "ValidToken", NewPassword = "NewPassword123!" };
			var result = await _service.ResetPasswordAsync(dto);

			Assert.True(result);
			_mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
		}
	}
}