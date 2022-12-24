using RedditWorkerService.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWorkerService.RedditService.Models
{
    public class ListingRequest
    {
        public string Subreddit { get; set; }
        public string Type { get; set; }
        public int Limit { get; set; }
        public int Count { get; set; }
        public string After { get; set; } = "";
        public string Before { get; set; } = "";
    }
}
