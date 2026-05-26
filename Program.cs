using CareerHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddSingleton< JobService >();
var app = builder.Build();

//Get jobs and return available jobs
app.MapGet("/jobs",async (JobService jobService) =>
{
    var jobs = await jobService.GetAllJobsAsync();
    return Results.Ok(jobs);
})
.WithName("GetAllJobs");

//Get jobs id
app.MapGet("/jobs/{id:int}", async (int id, JobService jobService) =>
{
    var job = await jobService.GetJobByIdAsync(id);
    if(job == null)
    {
        return Results.NotFound(new{Message = $"Job Listing with ID {id} not found"});
    }
    return Results.Ok(job);
})
.WithName("GetJobById");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
