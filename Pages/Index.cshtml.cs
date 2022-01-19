using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace aad_app_demo.Pages
{
    public class DataModel
    {
        public string id {get;set;}
        public DateTime date {get;set;}
        public string record {get;set;}

        public DataModel()
        {
            id = Guid.NewGuid().ToString();
            date = DateTime.Now;
            record = "";
        }
    }
    
    public class IndexModel : PageModel
    {
        public string role {get;set;}
        private static CosmosClient client {get;set;}
        private static Container container {get;set;}
        public List<DataModel> data {get;set;}
        [BindProperty]
        public DataModel thisData {get;set;}
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _config;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _config = configuration;
            _logger = logger;
            role = "";
            client = new CosmosClient(configuration["CosmosDbConnectionString"]);
            container = client.GetContainer("demo","data");
            data = new List<DataModel>();
            thisData = new DataModel();
        }

        public void OnGet()
        {
            if(User.Identity.IsAuthenticated)
            {
                try {
                    role = User.Claims.ToArray().First(x=> x.Type.ToString() == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Value.ToString();
                }
                catch {
                    role = "";
                }
                data = container.GetItemLinqQueryable<DataModel>(true).Where(x => x.date > DateTime.Now.AddDays(-7)).OrderByDescending(y => y.date).ToList();
            }
        }

        public ActionResult OnPost()
        {
            container.CreateItemAsync<DataModel>(thisData);
            return RedirectToPage("Index");
        }
    }
}
