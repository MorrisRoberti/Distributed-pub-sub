using Registry.Business;
using Registry.Repository;
using Registry.Business.Abstractions;
using Registry.Business.Kafka;
using Registry.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Identity.ClientHttp;
var builder = WebApplication.CreateBuilder(args);

// Register the service for the controllers
builder.Services.AddControllers();
// Registers the DbContext as a scoped service (in this way it can be injected).
// It tells the application to look for a "ConnectionString" section in the appsettings.json and get the value for the key "SubscriptionDbContext".
// Actually it takes the variable from the Docker environment in the docker-compose
builder.Services.AddDbContext<SubscriptionDbContext>(options => options.UseSqlServer("name=ConnectionStrings:SubscriptionDbContext"));
// Adds support for swagger
builder.Services.AddSwaggerGen();

// Injection of the IdentityHttpClient as a service
builder.Services.AddHttpClient<IdentityClientHttp>(client =>
{
    // Reads the Service url to which the client makes the calls inside the configuration environment (in this case docker-compose -> appsettings.json)
    // then sets this URI of the HttpClient that will be injected in our IdentityClientHttp, 
    // in this way we only have to specify the path of the endpoint in the IdentityClientHttp class
    var identityUrl = builder.Configuration["IdentityService:Url"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(identityUrl);
});

// Instantiation scoped because I need them only for the current HTTP request
builder.Services.AddScoped<IBusiness, Business>();
builder.Services.AddScoped<IRepository, Repository>();

// Registers the ProducerServiceWithSubscription for Kafka as a Singleton for dependency injection 
// this means that the Producer is called by some other component
builder.Services.AddSingleton<ProducerServiceWithSubscription>();
// Registers the OutBoxWorker as a BackgroundService, this means that no other components calls it 
// but it is launched on startup
builder.Services.AddHostedService<OutboxWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // I ask the DI system to get the DbContext
    var context = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
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
