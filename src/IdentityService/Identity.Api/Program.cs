using Identity.Business;
using Identity.Business.Abstractions;
using Identity.Repository;
using Identity.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// ATTENTION: the DbContext is scoped for a series of reasons, mainly for efficiency because if it was Singleton
// we would have had a single instance that would have accumulated all the data of old requests.


// Registers the services for the controllers 
builder.Services.AddControllers();
// Registers the DbContext as a scoped service (in this way it can be injected).
// It tells the application to look for a "ConnectionString" section in the appsettings.json and get the value for the key "IdentityDbContext".
// Actually it takes the variable from the Docker environment in the docker-compose 
builder.Services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer("name=ConnectionStrings:IdentityDbContext"));
// Adds support for swagger
builder.Services.AddSwaggerGen();

// Instantiation scoped because I need them only for the current HTTP request
builder.Services.AddScoped<IBusiness, Business>();
builder.Services.AddScoped<IRepository, Repository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // I ask the DI system to get the DbContext
    var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    // This creates the database and the tables (registered in the context)
    // NOTE: this is ok for the first time but if the db changes (add or remove tables etc.) this will not re-create it
    context.Database.EnsureCreated();
}

// Middlewares
// Configuration of auto-generated swagger documentation to test the API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Maps the controllers: when a request arrives it looks for the classes with the [Route] attribute and sends them the request
app.MapControllers();

app.Run();
