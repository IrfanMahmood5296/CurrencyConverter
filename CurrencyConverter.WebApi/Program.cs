using Currency.Application.Helpers.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCurrencyProviders();
builder.Services.AddIdentityServerConfig(builder.Configuration);
builder.Services.AddSwaggerWithAuth();
builder.Services.AddRedis(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Logging.AddFile();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIdentityServer();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers().RequireAuthorization("ApiScope");

app.Run();
