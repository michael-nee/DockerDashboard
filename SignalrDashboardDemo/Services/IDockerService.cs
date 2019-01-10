using SignalrDashboardDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignalrDashboardDemo.Services
{
    public interface IDockerService
    {
        Task<IEnumerable<ContainerViewModel>> HostContainers();

        Task<ContainerViewModel> ContainerDetails(string containerId);

        Task StreamContainerStats(string containerId, CancellationToken token);

        Task StreamContainerLogs(string containerId, CancellationToken token);

    }
}
