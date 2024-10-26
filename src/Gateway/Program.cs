using Consul;
using Gateway.Services;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddReverseProxy();

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
  //consulConfig.Address = new Uri("http://localhost:8500"); // local
  consulConfig.Address = new Uri("http://consul1:8500"); // Docker
}));



builder.Services.AddSingleton<IProxyConfigProvider,ConsulProxyConfigProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}



//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.Run();
