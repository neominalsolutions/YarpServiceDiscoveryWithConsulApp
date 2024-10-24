using Consul;
using Microsoft.Extensions.Primitives;
using System.Threading;
using Yarp.ReverseProxy.Configuration;


namespace Gateway.Services
{
  public class ConsulProxyConfig : IProxyConfig
  {
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    public IReadOnlyList<Yarp.ReverseProxy.Configuration.RouteConfig> Routes { get; }

    public IReadOnlyList<ClusterConfig> Clusters { get; }

    public IChangeToken ChangeToken { get; }

    public ConsulProxyConfig(IReadOnlyList<Yarp.ReverseProxy.Configuration.RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
      Routes = routes;
      Clusters = clusters;
      ChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
    }
  }

  public class ConsulProxyConfigProvider : IProxyConfigProvider
  {
    private readonly IConsulClient consulClient;

    public ConsulProxyConfigProvider(IConsulClient consulClient)
    {
      this.consulClient = consulClient;
    }

    public IProxyConfig GetConfig()
    {


      var routes = new List<Yarp.ReverseProxy.Configuration.RouteConfig>();

      var clusters = new List<Yarp.ReverseProxy.Configuration.ClusterConfig>();

      // Consul'dan tüm hizmetleri al
      var services = consulClient.Agent.Services().Result;
      foreach (var service in services.Response)
      {
        var serviceId = service.Value.ID;
        var serviceName = service.Value.Service;
        var serviceAddress = service.Value.Address;
        var servicePort = service.Value.Port;

        // YARP yapılandırmasına ekle

        routes.Add(new Yarp.ReverseProxy.Configuration.RouteConfig
        {
          RouteId = serviceId,
          ClusterId = serviceName,
          Match = new RouteMatch
          {
            Path = $"/{serviceName}/{{**catch-all}}"
          }
        });

        var destinationAddress = $"https://{serviceAddress}:{servicePort}";

        clusters.Add(new ClusterConfig
        {
          ClusterId = serviceName,
          Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                {
                    { serviceId, new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = destinationAddress } }
                }
        });


      }

      return new ConsulProxyConfig(routes, clusters);
    }


  }
}
