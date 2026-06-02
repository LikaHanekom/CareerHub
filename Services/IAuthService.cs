using CareerHub.Api.DTOs;

namespace CareerHub.Api.Services;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest request);
}