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

//v2
//var routes = new List<Yarp.ReverseProxy.Configuration.RouteConfig>();
//routes.Add(new Yarp.ReverseProxy.Configuration.RouteConfig
//{
 
//});

//var clusters = new List<Yarp.ReverseProxy.Configuration.ClusterConfig>();
//clusters.Add(new Yarp.ReverseProxy.Configuration.ClusterConfig
//{

//});

//builder.Services.AddReverseProxy().LoadFromMemory(routes,clusters);

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
  consulConfig.Address = new Uri("http://localhost:8500"); // Local
  // consulConfig.Address = new Uri("http://consul1:8500"); // Docker
}));



// route yapýlarý artýk buradan hizmet veriyor.
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
