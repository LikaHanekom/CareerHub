using CareerHub.Api.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CareerHub.Api.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponse? Login(LoginRequest request)
    {
        // Validate credentials
        if (request.Username != "employer" ||
            request.Password != "password123")
        {
            return null;
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

        return new LoginResponse(tokenString);
    }
}