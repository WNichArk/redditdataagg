using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace RedditWorkerService.RedditService.Models
{
    public class RedditRequestHelper
    {
        private readonly IConfiguration Configuration;
        private static TokenResponse Token { get; set; }
        public RedditRequestHelper(IConfiguration config)
        {
            Configuration = config;
        }

        //Not super useful. Used to create a request to paste into browser for auth first time auth request from user. Generate secret.
        public string CreateBaseRequest(string duration, string redirecUri = "https://localhost:8080/index", string scope = "read", string clientId = "")
        {
            if(!string.IsNullOrEmpty(clientId))
            {
                clientId = Configuration.GetSection("RedditConfig:client_id").Value;
            }
            redirecUri = HttpUtility.UrlEncodeUnicode(redirecUri);
            var baseURL = $"https://www.reddit.com/api/v1/authorize?client_id={clientId}&response_type=code&" +
                $"state=stateString&redirect_uri={redirecUri}&duration={duration}&scope={scope}";
            Process.Start(new ProcessStartInfo($"{GetEdgePath()}", $"{baseURL}"));
            return baseURL;
        }

        public string GetNew(string subreddit)
        {
            var ret = "nothing here";
            try
            {
                var token = GetAccessToken(false);
                if (token == null)
                {
                    Console.WriteLine("No token.");
                    return ret;
                }
                var client = new RestClient();
                //var req = new RestRequest($"https://oauth.reddit.com/api/v1/scopes");
                //var req = new RestRequest($"https://oauth.reddit.com/api/v1/r/{subreddit}/random.json");
                var req = new RestRequest($"https://oauth.reddit.com/api/v1/r/{subreddit}/new.json");
                //var req = new RestRequest($"https://oauth.reddit.com/api/v1/me");
                req.AddParameter("limit", "20");
                req.AddParameter("count", "0");
                req.AddParameter("after", "t3_zrfyhb");
                req.AddHeader("Authorization", $"bearer {token.access_token}");
                req.AddHeader("User-Agent", "windows:CSharpFactFinding/v1 (by /u/wnredditdevprojects)");
                var res = client.Execute(req);
                Console.WriteLine(res.Content);
                return $"Content retrieved: {res.Content}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ret;
        }

        public TokenResponse GetAccessToken(bool forceRefresh, string redirectUri = "https://localhost:8080/index")
        {
            var client = new RestClient("https://www.reddit.com/api/v1/access_token");
            var req = new RestRequest("https://www.reddit.com/api/v1/access_token", Method.Post);

            if (CheckToken() != null && !forceRefresh)
            {
                return CheckToken();
            }
            var user = Configuration.GetSection("RedditConfig:client_id").Value;
            var pass = Configuration.GetSection("RedditConfig:client_secret").Value;
            var code = Configuration.GetSection("RedditConfig:current_request_code");

            var authString = Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes($"{user}:{pass}"));
            req.AddHeader("Authorization", $"Basic {authString}");
            req.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            req.AddBody($"grant_type=authorization_code&code={code}&redirect_uri={redirectUri}");
            var res = client.Execute(req);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var token = JsonConvert.DeserializeObject<TokenResponse>(res.Content);
                var json = JsonConvert.SerializeObject(token);
                File.WriteAllText(@".\token.json", json);
                return token;
            }
            //else if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            //{
            //    req.AddBody($"grant_type=refresh_token&refresh_token={code}");
            //    res = client.Execute(req);
            //    return JsonConvert.DeserializeObject<TokenResponse>(res.Content);
            //}
            else
            {
                return null;
            }
        }

        public TokenResponse CheckToken()
        {
            try
            {
                var token = File.ReadAllText(@".\token.json");
                Token = JsonConvert.DeserializeObject<TokenResponse>(token);
                return Token;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetEdgePath()
        {
            string lPath = null;
            try
            {
                var lTmp = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe", "", null);
                if (lTmp != null)
                    lPath = lTmp.ToString();
                else
                {
                    lTmp = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe", "", null);
                    if (lTmp != null)
                        lPath = lTmp.ToString();
                }
            }
            catch (Exception lEx)
            {
                //Logger.Error(lEx);
            }

            if (lPath == null)
            {
                //Logger.Warn("Chrome install path not found! Returning hardcoded path");
                lPath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            }

            return lPath;
        }
    }
}
