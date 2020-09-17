using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SocialMediaReader.Controllers
{

    [Authorize]


    public class FacebookController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Facebook
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> Posts()
        {

            var currentClaims = await UserManager.GetClaimsAsync(HttpContext.User.Identity.GetUserId());
            var accestoken = currentClaims.FirstOrDefault(x => x.Type == "urn:tokens:facebook");
            if (accestoken == null)
            {
                return (new HttpStatusCodeResult(HttpStatusCode.NotFound, "Token not found"));
            }

            string url = String.Format("https://graph.facebook.com/me?fields=id,name,feed.limit(500){{link,height,width,type,message,story,created_time,id,target,full_picture}}&access_token={0}", accestoken.Value);
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = await reader.ReadToEndAsync();

                dynamic jsonObj = System.Web.Helpers.Json.Decode(result);
                Models.SocialMedia.Facebook.posts posts = new Models.SocialMedia.Facebook.posts(jsonObj);
                ViewBag.JSON = result;
                string json = JsonConvert.SerializeObject(jsonObj);
                string path = @"C:\Big Data Analytics\Social data mining Technique\Assignment\fbdata.json";
                using (TextWriter tw = new StreamWriter(path))
                {
                    //foreach (var p in json)
                    // {
                    tw.WriteLine(json);
                    // }

                }
                var connectionString = "mongodb+srv://test:test@cluster0-l9vdm.mongodb.net/test?retryWrites=true&w=majority";
                

                var client = new MongoClient(connectionString);

                var database = client.GetDatabase("SocialMediaR");

                string text = System.IO.File.ReadAllText(path);
                var collection = database.GetCollection<BsonDocument>("FacebookR");
                var aa = new BsonDocument
                        {
                            {"a","aa"},
                            {"b","bb"}

                        };
                await collection.InsertOneAsync(aa);
                client.DropDatabase("SocialMedia" +
                    "");
                MongoDB.Bson.BsonDocument docu
                    = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(result);
                await collection.InsertOneAsync(docu);


                return View(posts);

            }
                
            

        }
    }
}