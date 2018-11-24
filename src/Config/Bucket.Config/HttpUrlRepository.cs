﻿using Bucket.Config.Util;
using Bucket.LoadBalancer;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
namespace Bucket.Config
{
    public class HttpUrlRepository : IHttpUrlRepository
    {
        private readonly ConfigOptions _setting;
        private readonly IServiceProvider _serviceProvider;

        public HttpUrlRepository(ConfigOptions setting, IServiceProvider serviceProvider)
        {
            _setting = setting;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetApiUrl(long version)
        {
            string appId = _setting.AppId;
            string secret = _setting.AppSercet;

            var path = $"/configs/{_setting.AppId}/{_setting.NamespaceName}";

            var query = $"version={version}";

            var sign = $"appId={appId}&appSecret={secret}&namespaceName={_setting.NamespaceName}";

            var pathAndQuery = $"{path}?{query}&env={_setting.Env}&sign=" + SecureHelper.SHA256(sign);

            if (_setting.UseServiceDiscovery)
            {
                var _loadBalancerHouse = _serviceProvider.GetRequiredService<ILoadBalancerHouse>();
                var _balancer = await _loadBalancerHouse.Get(_setting.ServiceName, "RoundRobin");
                var HostAndPort = await _balancer.Lease();
                _setting.ServerUrl = $"{HostAndPort.ToUri()}";
            }

            return $"{_setting.ServerUrl.TrimEnd('/')}{pathAndQuery}";
        }
    }
}
