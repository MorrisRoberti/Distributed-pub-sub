using EventEngine.Business;
using EventEngine.Business.Kafka;
using EventEngine.Repository;
using EventEngine.Business.Abstractions;
using EventEngine.Repository.Abstractions;
using EventEngine.ClientHttp;
using EventEngine.ClientHttp.Abstractions;

using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Register the service for the controllers
builder.Services.AddControllers();
// Registers the DbContext as a scoped service (in this way it can be injected).
// It tells the application to look for a "ConnectionString" section in the appsettings.json and get the value for the key "EventEngineDbContext".
// Actually it takes the variable from the Docker environment in the docker-compose
builder.Services.AddDbContext<EventEngineDbContext>(options => options.UseSqlServer("name=ConnectionStrings:EventEngineDbContext"));
// Adds support for swagger
builder.Services.AddSwaggerGen();

// Instantiation scoped because I need them only for the current HTTP request
builder.Services.AddScoped<IBusiness, Business>();
builder.Services.AddScoped<IRepository, Repository>();

// Registers the SubscriptionConsumerWorker and DispatchService as a BackgroundService, this means that no other components calls it 
// but it is launched on startup
builder.Services.AddHostedService<SubscriptionConsumerWorker>();
builder.Services.AddHostedService<DispatchService>();

// I changed the service registration into a Strongly Typed one
// The ClientHttp is going to be registered as Transient service
builder.Services.AddHttpClient<ClientHttp>(client =>
{
    // If the client doesn't reply in 10 seconds close the connection
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // I ask the DI system to get the DbContext
    var context = scope.ServiceProvider.GetRequiredService<EventEngineDbContext>();
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

