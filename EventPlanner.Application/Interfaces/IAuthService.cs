using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventPlanner.Application.DTOs.Auth;
namespace EventPlanner.Application.Interfaces
{
	public interface IAuthService
	{
		Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
		Task<AuthResponseDto> LoginAsync(LoginDto dto);
		Task<string?> ForgotPasswordAsync(string email);
		Task<bool> ResetPasswordAsync(ResetPasswordDto request);
	}
}