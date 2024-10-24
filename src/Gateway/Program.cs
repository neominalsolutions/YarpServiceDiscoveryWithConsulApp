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
  consulConfig.Address = new Uri("http://localhost:8500");
}));

builder.Services.AddSingleton<IProxyConfigProvider,ConsulProxyConfigProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}


// Uygulama down olurken temizle iþlemi
var consulClient = app.Services.GetRequiredService<IConsulClient>();
consulClient.Agent.ServiceDeregister("BasketService").Wait();
consulClient.Agent.ServiceDeregister("ProductService").Wait();

// uygulama stop edilirken silin
app.Lifetime.ApplicationStopping.Register(() =>
{
  consulClient.Agent.ServiceDeregister("BasketService").Wait();
  consulClient.Agent.ServiceDeregister("ProductService").Wait();
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.Run();
