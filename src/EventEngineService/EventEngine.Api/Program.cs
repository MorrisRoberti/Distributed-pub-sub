using EventEngine.Business;
using EventEngine.Business.Kafka;
using EventEngine.Repository;
using EventEngine.Business.Abstractions;
using EventEngine.Repository.Abstractions;
using EventEngine.ClientHttp;
using EventEngine.ClientHttp.Abstractions;

using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddDbContext<EventEngineDbContext>(options => options.UseSqlServer("name=ConnectionStrings:EventEngineDbContext"));
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBusiness, Business>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddHostedService<SubscriptionConsumerWorker>();
builder.Services.AddHostedService<DispatchService>();

builder.Services.AddHttpClient("ClientHttp", client =>
{
    // if the client doesn't reply in 10 seconds close the connection
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IClientHttp, ClientHttp>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventEngineDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
