using CareerHub.Api.DTOs;
using CareerHub.Api.Services;
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
     private readonly IAuthService _authService; //hold application config settigns
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    //Login Endpoint
    [HttpPost("login")]
    //actionResult = what the method will return
    public ActionResult<LoginResponse> Login(CareerHub.Api.DTOs.LoginRequest request)
    {

         var result = _authService.Login(request);
        //validation
        if(result is null)
        {
                return Unauthorized();
        }
        return Ok(result);  
        
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


