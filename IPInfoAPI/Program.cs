using IPInfoAPI.Repositories;
using IPInfoAPI.Services;
using IPInfoLibrary;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IIPInfoProvider>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var apiKey = configuration["IPStack:ApiKey"];

    return new IPInfoProvider(apiKey);
});

builder.Services.AddDbContext<IPInfoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IIPDetailsRepository, IPDetailsRepository>();

builder.Services.AddSingleton<BatchProcessingService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BatchProcessingService>());


builder.Services.AddControllers();
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
