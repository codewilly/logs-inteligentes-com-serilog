using API;
using API.Enrichers;
using API.Middlewares;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, cfg) =>
{
    cfg.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
       .WriteTo.Console()
       .Enrich.With<TraceIdEnricher>();
});

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<LogMiddleware>();

var app = builder.Build();

app.UseServiceActivator();

app.UseMiddleware<LogMiddleware>();

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
