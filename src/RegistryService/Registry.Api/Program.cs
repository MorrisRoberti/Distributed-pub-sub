using Registry.Business;
using Registry.Repository;
using Registry.Business.Abstractions;
using Registry.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddDbContext<SubscriptionDbContext>(options => options.UseSqlServer("name=ConnectionStrings:SubscriptionDbContext"));
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBusiness, Business>();
builder.Services.AddScoped<IRepository, Repository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// 2. Importante: abilita l'autorizzazione (anche se vuota per ora)
app.UseAuthorization();

// 3. Mappa i Controller. Questo sostituisce app.MapGet(...)
app.MapControllers();

app.Run();

