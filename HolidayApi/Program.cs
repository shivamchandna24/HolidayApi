using HolidayApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


builder.Services.AddDbContext<HolidayContext>(options =>
                                              options.UseSqlServer(
                                              builder.Configuration.GetConnectionString("DefaultConnection"),
                                              sqlOptions => sqlOptions.EnableRetryOnFailure(
                                              maxRetryCount: 5,              // number of retries
                                              maxRetryDelay: TimeSpan.FromSeconds(10), // delay between retries
                                              errorNumbersToAdd: null)       // use default transient errors
      ));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
