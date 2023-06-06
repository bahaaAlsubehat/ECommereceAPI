using Microsoft.EntityFrameworkCore;
using Ref.Lect4.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
.AddNewtonsoftJson(options =>
 options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
   );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<OnlineStoreContext>();
builder.Services.AddSwaggerGen();

// Added by me to solve 500 Internal Error
//builder.Services.AddDbContext<OnlineStoreContext>(options =>
//options.UseSqlServer("Server=DESKTOP-46F1969;Database=OnlineStore;Trusted_Connection=True;"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthorization();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization(); // Add it here
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.MapControllers();

app.Run();

