namespace CareerHub.Api.DTOs;

public record LoginRrquest(
    string Username, //upon login request client sends username and password
    string Password
);