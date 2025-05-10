using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class AlertCheckerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertCheckerService> _logger;
    private readonly HttpClient _httpClient;

    public AlertCheckerService(IServiceScopeFactory scopeFactory, ILogger<AlertCheckerService> logger, HttpClient httpClient)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔁 AlertCheckerService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            try
            {
                // Define the API endpoint you want to call
                var url = "http://localhost:5077/Api/DeviceDataDetail/GetAlert"; // Adjust to your base URL

                // Call the GetAlert method via HttpClient
                var response = await _httpClient.GetAsync(url, stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ GetAlert executed successfully at {DateTime.Now}");
                }
                else
                {
                    _logger.LogError($"❌ GetAlert failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to call GetAlert.");
            }

            // Wait for 1 minute before the next call
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
