using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventPlanner.Application.DTOs.Auth;
using EventPlanner.Application.Interfaces;
using EventPlanner.Core.Entities;
using EventPlanner.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventPlanner.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly IApplicationDbContext _context;
		private readonly IConfiguration _config;

		public AuthService(IApplicationDbContext context, IConfiguration config)
		{
			_context = context;
			_config = config;
		}

		public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
		{
			if (dto.Role == UserRole.Admin) throw new Exception("Admin registration is not allowed.");
			if (await _context.Users.AnyAsync(u => u.Email == dto.Email)) throw new Exception("Email exists.");
			if (await _context.Users.AnyAsync(u => u.Username == dto.Username)) throw new Exception("Username taken.");

			var user = new User
			{
				FullName = dto.FullName,
				Email = dto.Email,
				Username = dto.Username,
				Role = dto.Role,
				State = dto.State,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
			};
			_context.Users.Add(user);
			await _context.SaveChangesAsync();
			return GenerateToken(user);
		}

		public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
			if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) throw new Exception("Invalid credentials.");
			if (!user.IsActive) throw new Exception("Account deactivated.");
			return GenerateToken(user);
		}

		private AuthResponseDto GenerateToken(User user)
		{
			var claims = new[] {
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, user.Role.ToString()),
				new Claim("State", user.State)
			};
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
			var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims, expires: DateTime.Now.AddDays(1), signingCredentials: creds);
			return new AuthResponseDto { Token = new JwtSecurityTokenHandler().WriteToken(token), Role = user.Role.ToString(), UserId = user.Id };
		}

		public async Task<string?> ForgotPasswordAsync(string email)
		{

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null) return null;

			user.PasswordResetToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
			user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

			await _context.SaveChangesAsync();

			return user.PasswordResetToken;
		}

		public async Task<bool> ResetPasswordAsync(ResetPasswordDto request)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

			if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
			{
				return false;
			}

			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

			user.PasswordResetToken = null;
			user.ResetTokenExpires = null;

			await _context.SaveChangesAsync();

			return true;
		}
	}
}
