using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RedditWorkerService.DataAccessLayer;
using RedditWorkerService.RedditService.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWorkerService.RedditService
{
    public class RedditGetterService
    {
        private readonly ILogger _logger;
        private readonly RedditContext _dbContext;
        public RestClient restClient { get; set; } = new RestClient();
        public RedditGetterService(ILogger logger, RedditContext context)
        {
            _logger = logger;
            _dbContext = context;
        }

        public void ArchiveNewest(ListingRequest request)
        {
            var prevRequest = CheckLatestRequest(request);
            if(prevRequest != null && !string.IsNullOrEmpty(prevRequest.RedditAfter))
            {
                request.After = prevRequest.RedditAfter;
            };
            //Build Request URL
            var reqUrl = $"https://www.reddit.com/r/{request.Subreddit}/{request.Type}.json";
            reqUrl += (!string.IsNullOrEmpty(request.After)) ? $"?after={request.After}" : "";
            reqUrl += (!string.IsNullOrEmpty(request.Before)) ? $"&before={request.Before}" : "";
            reqUrl += (request.Count != 0) ? $"&count={request.Count}" : "";
            reqUrl += (request.Limit != 0) ? $"&limit={request.Limit}" : "";
            //Call Reddit
            var req = new RestRequest(reqUrl);
            var res = restClient.Execute(req);
            Console.WriteLine(res.Content);
            var obj = JsonConvert.DeserializeObject<SubredditDump>(res.Content);
            //Build historical request object
            var redditRequest = new RedditRequest()
            {
                Request = JsonConvert.SerializeObject(req),
                Response = JsonConvert.SerializeObject(res),
                RequestUrl = reqUrl,
                RedditAfter = obj.data.after, //most recent "After" from reddit response for paging
                TimeRequested = DateTime.Now,
                RequestSubreddit = request.Subreddit,
                RequestType = request.Type
            };
            MapDumpToDB(obj, redditRequest);
        }

        public RedditRequest CheckLatestRequest(ListingRequest request)
        {
            var existingRequest = _dbContext.RedditRequests.Where(a => a.RequestSubreddit == request.Subreddit && a.RequestType == request.Type).OrderByDescending(a => a.Id).FirstOrDefault();
            if (existingRequest != null)
            {
                return existingRequest;
            }
            else
            {
                return null;
            }
        }

        public bool MapDumpToDB(SubredditDump dump, RedditRequest request)
        {
            try
            {
                _dbContext.RedditRequests.Add(request);
                var sub = _dbContext.Subreddits.Where(a => a.Name.ToLower() == dump.data.children.First().data.subreddit).FirstOrDefault();
                if (sub == null)
                {
                    var subreddit = MapSubreddit(dump);
                    foreach (var item in dump.data.children)
                    {
                        var post = MapPost(item);
                        subreddit.Posts.Add(post);
                    }
                    _dbContext.Subreddits.Add(subreddit);
                }
                else
                {
                    foreach (var item in dump.data.children)
                    {
                        var post = MapPost(item);
                        sub.Posts.Add(post);
                    }
                }
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.ToString());
                return false;
            }
        }

        public Subreddit MapSubreddit(SubredditDump dump)
        {
            var sub = new Subreddit()
            {
                Name = dump.data.children.First().data.subreddit,
                Category = "none yet"
            };
            return sub;
        }

        public Post MapPost(Child post)
        {
            var retPost = new Post()
            {
                RedditFullname = post.data.name,
                Message = post.data.selftext,
                Title = post.data.title,
                Username = post.data.author,
                Flair = post.data.author_flair_text ?? "",
                TimePosted = DateTime.TryParse(post.data.created_utc.ToString(), out DateTime dt) ? dt : DateTime.Now,
                RawResponse = JsonConvert.SerializeObject(post)
            };
            return retPost;
        }



    }
}
