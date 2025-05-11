using Tutorial9.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IWarehouseRepository, SqlWarehouseRepository>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();