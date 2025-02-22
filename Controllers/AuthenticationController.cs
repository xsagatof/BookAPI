using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookAPI.DTOs;
using BookAPI.Models;

namespace BookAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly UserManager<User> _userManager;
		private readonly IConfiguration _configuration;

		public AuthenticationController(UserManager<User> userManager, IConfiguration configuration)
		{
			_userManager = userManager;
			_configuration = configuration;
		}

		[HttpPost]
		[Route("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
		{
			var userExists = await _userManager.FindByNameAsync(registerDto.Username);

			if (userExists != null)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new DTOs.Response { Status = "Error", Message = "User already exists!" });
			}

			User appUser = new User()
			{
				UserName = registerDto.Username,
				Email = registerDto.Email,
				SecurityStamp = Guid.NewGuid().ToString()
			};

			var result = await _userManager.CreateAsync(appUser, registerDto.Password);

			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				return StatusCode(StatusCodes.Status500InternalServerError, new DTOs.Response
				{
					Status = "Error",
					Message = $"User creation failed! {errors}"
				});
			}


			return Ok(new DTOs.Response { Status = "Success", Message = "User created successfully" });
		}

		[AllowAnonymous]
		[HttpPost]
		[Route("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
		{
			var appUser = await _userManager.FindByNameAsync(loginDto.Username);

			//Generating token
			if (appUser != null && await _userManager.CheckPasswordAsync(appUser, loginDto.Password))
			{
				var authClaims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, loginDto.Username),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			};

				var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

				var token = new JwtSecurityToken(
					issuer: _configuration["JWT:Issuer"],
					audience: _configuration["JWT:Audience"],
					expires: DateTime.Now.AddHours(7),
					claims: authClaims,
					signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
				);

				return Ok(new
				{
					token = new JwtSecurityTokenHandler().WriteToken(token),
					expiration = token.ValidTo
				});
			}
			return Unauthorized();
		}
	}
}
