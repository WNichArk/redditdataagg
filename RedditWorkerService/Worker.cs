using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedditWorkerService.DataAccessLayer;
using RedditWorkerService.RedditService;
using RedditWorkerService.RedditService.Models;

namespace RedditWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly RedditContext _dbContext;
        public TokenResponse Token { get; set; }

        public Worker(ILogger<Worker> logger, IConfiguration config, RedditContext context)
        {
            _logger = logger;
            _configuration = config;
            _dbContext = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
                var getter = new RedditGetterService(_logger, _dbContext);
                var listingRequest = new ListingRequest()
                {
                    Subreddit = "spacstocks",
                    Type = "new"
                };
                getter.ArchiveNewest(listingRequest);
            }
        }
    }
}