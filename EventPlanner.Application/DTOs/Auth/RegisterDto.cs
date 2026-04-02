using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventPlanner.Core.Enums;
namespace EventPlanner.Application.DTOs.Auth
{
	public class RegisterDto
	{
		public string FullName { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string State { get; set; }
		public UserRole Role { get; set; }
	}
}
