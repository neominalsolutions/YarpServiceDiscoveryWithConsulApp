using Consul;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
  consulConfig.Address = new Uri("http://localhost:8500"); // Local
  // consulConfig.Address = new Uri("http://consul1:8500"); // Docker
}));

var app = builder.Build();

var consulClient = app.Services.GetRequiredService<IConsulClient>();


// uygulama ayaða kalktýðýnda

app.Lifetime.ApplicationStarted.Register(() =>
{
  var uri = new Uri("http://localhost:5002"); // Local
  // var uri = new Uri("http://api2:5002"); // Docker
  var serviceName = "api2";
  var serviceId = "api2";

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

app.Lifetime.ApplicationStopping.Register(() =>
{
  Console.Out.WriteLine("Service Deregister");
  consulClient.Agent.ServiceDeregister("api2").Wait();
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
