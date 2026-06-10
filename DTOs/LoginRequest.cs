namespace CareerHub.Api.DTOs;

public record LoginRequest(
    string Username, //upon login request client sends username and password
    string Password
);