using Microsoft.EntityFrameworkCore;
using RedditWorkerService.RedditService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWorkerService.DataAccessLayer
{
    public class RedditContext : DbContext
    {
        public DbSet<Subreddit> Subreddits { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<RedditRequest> RedditRequests { get; set; }
        private readonly IConfiguration _configuration;
        
        public RedditContext(IConfiguration config)
        {
            _configuration = config;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_configuration.GetSection("ConnectionStrings:default").Value);
        }
    }

    public class Subreddit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set;}
        public List<Post> Posts { get; set; } = new List<Post>();

        
    }

    public class Post
    {
        public int Id { get; set; }
        public string RedditFullname { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public string Flair { get; set; }
        public DateTime TimePosted { get; set; }
        public string RawResponse { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();

    }

    public class Comment
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public DateTime TimePosted { get; set; }
    }

    public class RedditRequest
    {
        public int Id { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string RequestType { get; set; }
        public string RequestUrl { get; set; }
        public string? RequestSubreddit { get; set; }
        public string? RedditAfter { get; set; }
        public DateTime TimeRequested { get; set; }

    }
}
