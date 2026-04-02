using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlanner.Application.DTOs.Auth
{
	public class AuthResponseDto { 
	public string Token { get; set; }
	public string Role { get; set; }
	public int UserId { get; set; } 
	
	}
}