using Registry.Business;
using Registry.Repository;
using Registry.Business.Abstractions;
using Registry.Business.Kafka;
using Registry.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Identity.ClientHttp;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddDbContext<SubscriptionDbContext>(options => options.UseSqlServer("name=ConnectionStrings:SubscriptionDbContext"));
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IdentityClientHttp>(client =>
{
    client.BaseAddress = new Uri("http://identity-service:8080");
});

builder.Services.AddScoped<IBusiness, Business>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddSingleton<ProducerServiceWithSubscription>();
builder.Services.AddHostedService<OutboxWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
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

