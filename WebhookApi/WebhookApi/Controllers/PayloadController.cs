using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using WebhookApi.Models;

namespace WebhookApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayloadController : ControllerBase
    {
        [HttpPost]
        public void Post(GitHubPayLoad payload)
        {
            //var objectInJson = JsonConvert.SerializeObject(payload);
            //Console.WriteLine(objectInJson.ToString());
            string pullRequestUrl = payload.Pull_request.Review_comments_url;
            
            ProcessRequestAsync(pullRequestUrl).Wait();
        }

        private static async System.Threading.Tasks.Task ProcessRequestAsync(string pullRequestUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.github.com");
                var token = "3ed13f5c2b46856c7ea284de4ee4fdeea1f777b8";
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppName", "1.0"));
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);

                var response = await client.GetAsync(new Uri(pullRequestUrl).LocalPath);
                string data = "";
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var comments = JsonConvert.DeserializeObject<IEnumerable<CommentPayload>>(responseString).ToList<CommentPayload>();
                    comments.ForEach(c => data += c.User.Login + "\n" + c.Body + "\n");
                    data += $"Total Comments: {comments.Count}\n\n";
                }
                else
                {
                    data = $"Error In Fetching Data fro api call {response.ReasonPhrase}";
                    data += $"Total Comments: 0\n\n";
                }
                System.IO.File.WriteAllText(@"C:\Users\rmittal\Desktop\TAVISCA\WebhookApi\WebhookApi\response.txt", data);
            }

        }
    }
}
