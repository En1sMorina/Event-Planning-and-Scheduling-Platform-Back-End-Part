using EventPlanner.Application.DTOs.Auth;
using EventPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanner.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _auth;

		public AuthController(IAuthService auth) => _auth = auth;

		[HttpPost("register")] public async Task<IActionResult> Register(RegisterDto dto) => Ok(await _auth.RegisterAsync(dto));
		[HttpPost("login")] public async Task<IActionResult> Login(LoginDto dto) => Ok(await _auth.LoginAsync(dto));

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword(string email)
		{
			var token = await _auth.ForgotPasswordAsync(email);

			if (token == null) return BadRequest("User not found.");

			return Ok(new { token });
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
		{
			var success = await _auth.ResetPasswordAsync(request);

			if (!success) return BadRequest("Invalid or expired token.");

			return Ok(new { message = "Password successfully reset!" });
		}
	}
}