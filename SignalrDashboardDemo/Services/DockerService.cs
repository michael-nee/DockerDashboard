using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SignalrDashboardDemo.Hubs;
using SignalrDashboardDemo.Models;
using SignalrDashboardDemo.Options;

namespace SignalrDashboardDemo.Services
{
    public class DockerService : IDockerService, IDisposable
    {
        private readonly IHubContext<DockerHostHub> _hubContext;

        private DockerClient _client;
        private readonly IOptions<DockerOptions> _dockerOptions;

        public DockerService(IOptions<DockerOptions> dockerOptions, IHubContext<DockerHostHub> hubContext)
        {
            _dockerOptions = dockerOptions;
            _hubContext = hubContext;

            _client = new DockerClientConfiguration(
                new Uri("tcp://" + _dockerOptions.Value.ConnectionString))
                 .CreateClient();
        }


        public async Task<IEnumerable<ContainerViewModel>> HostContainers()
        {
            var containers = await _client.Containers.ListContainersAsync(
            new ContainersListParameters { Limit = 99999 });

            var list = new List<ContainerViewModel>();

            foreach(var container in containers)
            {
                list.Add(new ContainerViewModel
                {
                    Id = container.ID,
                    Name = container.Names.FirstOrDefault()
                });
            }

            return list;
        }

        public async Task<ContainerViewModel> ContainerDetails(string containerId)
        {
            var foundContainer = await GetContainer(containerId);

            var container = new ContainerViewModel
            {
                Id = foundContainer.ID,
                Name = foundContainer.Status
            };

            return container;
        }

        public async Task StreamContainerStats(string containerId, CancellationToken token)
        {
            var foundContainer = await GetContainer(containerId);

            CancellationTokenSource cancellation = new CancellationTokenSource();

            IProgress<ContainerStatsResponse> progress = new Progress<ContainerStatsResponse>(stats =>
            {
                //var json = JsonConvert.SerializeObject(stats);

                _hubContext.Clients.All.SendAsync("NewStat", this, stats);
            });

            await _client.Containers.GetContainerStatsAsync(containerId, new ContainerStatsParameters { Stream = true }, progress, token);
        }


        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task StreamContainerLogs(string containerId, CancellationToken token)
        {
            var foundContainer = await GetContainer(containerId);

            IProgress<string> progress = new Progress<string>(log =>
            {
                //var json = JsonConvert.SerializeObject(stats);

                _hubContext.Clients.All.SendAsync("NewLog", this, log);
            });

           await _client.Containers.GetArchiveFromContainerAsync(containerId, new ContainerLogsParameters(), token, progress: progress);
        }

        private async Task<ContainerListResponse> GetContainer(string containerId)
        {
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters { Limit = 99999 });

            var foundContainer = containers.FirstOrDefault(c => c.ID == containerId);
            if (foundContainer == null)
                throw new NullReferenceException(nameof(containerId));

            return foundContainer;
        }
    }
}
