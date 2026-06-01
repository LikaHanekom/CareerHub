using CareerHub.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;

namespace CareerHub.Api.Controllers;

[ApiController]//tells .NET this is a Web API controller. 
[Route("api/auth")] // URL path to get to this controll;er
public class AuthController : ControllerBase //.NETs built in controllerbase: gives controller access to standard API tools.
{
    private readonly IConfiguration _configuration; //hold application config settigns
    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    //Login Endpoint
    [HttpPost("login")]
    //actionResult = what the method will return
    public ActionResult<LoginResponse> Login(CareerHub.Api.DTOs.LoginRequest request)
    {
        //validation
        if(request.Username != "employer" || request.Password != "password123")
            {
                return Unauthorized();
            }
        
        //Create JWT Claims
        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,//defines who the token belongs to
                request.Username),

            new Claim(
                ClaimTypes.Role, //assigns a specific access role to the user.
                "Employer")
        };

        //Generate JWT
        //reads raw text secret from your appsettings.json
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes( //translates tect string to a raw array of bytes
                _configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials( //combines key with hashing algorithm
                key,
                SecurityAlgorithms.HmacSha256);//acts as digital signature

        //Create Token
        var token = new JwtSecurityToken(
            claims: claims, //user identity details
            expires: DateTime.UtcNow.AddHours(2), //2hours till expiration
            signingCredentials: credentials); //cryptographic signature

        //Convert Token
        var tokenString =
            new JwtSecurityTokenHandler()
                .WriteToken(token);//serialize the object into the standard string format

        //Return Token
        return Ok(
            new LoginResponse(tokenString));
    }   
    // me endpoint
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var username = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            Username = username,
            Role = role
        });
    }
}


