using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SignalrDashboardDemo.Models;
using SignalrDashboardDemo.Services;

namespace SignalrDashboardDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDockerService _dockerService;

        private static CancellationTokenSource _statsToken;
        private static CancellationTokenSource _logsToken;

        public HomeController(IDockerService dockerService)
        {
            _dockerService = dockerService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dockerService.HostContainers());
        }

        [HttpGet]
        public async Task<IActionResult> Container([Required]string containerId)
        {
            var container = await _dockerService.ContainerDetails(containerId);

           return View(container);
        }

        [HttpPost]
        public IActionResult StartContainerStatsStreaming([Required]string containerId)
        {
            _statsToken = new CancellationTokenSource();
            _dockerService.StreamContainerStats(containerId, _statsToken.Token);
            return Ok();
        }

        [HttpPost]
        public IActionResult StopContainerStatsStreaming([Required]string containerId)
        {
            _statsToken.Cancel();
            return Ok();
        }

        [HttpPost]
        public IActionResult StartContainerLogsStreaming([Required]string containerId)
        {
            _logsToken = new CancellationTokenSource();
            _dockerService.StreamContainerLogs(containerId, _logsToken.Token);
            return Ok();
        }

        [HttpPost]
        public IActionResult StopContainerLogsStreaming([Required]string containerId)
        {
            _logsToken.Cancel();
            return Ok();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
