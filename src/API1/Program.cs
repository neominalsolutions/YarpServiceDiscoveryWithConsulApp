using Consul;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// service discovery sunucusu
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
  consulConfig.Address = new Uri("http://localhost:8500"); // Local
  // consulConfig.Address = new Uri("http://consul1:8500"); // Docker Container Name üzerinden haberleþmelerini saðlamak. consul1 arkadaþ container_name denk gelir.
}));


var app = builder.Build();


var consulClient = app.Services.GetRequiredService<IConsulClient>();

app.Lifetime.ApplicationStarted.Register(() =>
{
  //var uri = new Uri("http://localhost:5001"); // Local
  var uri = new Uri("http://api1:5001"); // Docker => container_name
  var serviceName = "api1";
  var serviceId = "api1";

  var registration = new AgentServiceRegistration()
  {
    ID = serviceId,
    Name = serviceName,
    Address = $"{uri.Host}",
    Port = uri.Port,
    Tags = new[] { serviceName, serviceId }

  };

  consulClient.Agent.ServiceRegister(registration).Wait();
  Console.Out.WriteLine("Service Registered");
});

// uygulama down olduðunda servis register ettiði consul üzerinden kendini sildirmesi lazým.
app.Lifetime.ApplicationStopping.Register(() =>
{
  Console.Out.WriteLine("Service Deregistered");
  consulClient.Agent.ServiceDeregister("api1").Wait();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
