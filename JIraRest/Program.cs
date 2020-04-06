using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Json.Net;
using Newtonsoft.Json;

namespace JIraRest
{
    class JIRA
    {
        //env variables
        public string jiraBaseUrl = "";

        public JIRA(string baseUrl)
        {
            jiraBaseUrl = baseUrl;
        }

        private async System.Threading.Tasks.Task<string> REQAsync(string Metod = "POST", string url = "", string body = "")
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(
                System.Text.ASCIIEncoding.ASCII.GetBytes(
                $"username:apiToken")));

                using (var request = new HttpRequestMessage(new HttpMethod(Metod), jiraBaseUrl + url))
                {
                    request.Headers.Add("Accept", "application/json");
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes("username:apiToken"));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    if (body != "")
                    {
                        //Console.WriteLine("REQUEST BODY:");
                        //Console.WriteLine(body + Environment.NewLine);
                        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                    }

                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    String responseString = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("RESPONSE:");
                    //Console.WriteLine(responseString);
                    return responseString;
                }
            }
        }

        public async System.Threading.Tasks.Task<string> CreateUser(string name, string password, string email, string displayName)
        {
            //CREATE USER
            String Body = "{";
            Body = Body + "\"name\":\"" + name + "\", ";
            Body = Body + "\"password\":\"" + password + "\",";
            Body = Body + "\"emailAddress\":\"" + email + "\",";
            Body = Body + "\"displayName\":\"" + displayName + "\"";
            Body = Body + "}";
            string response = await REQAsync("POST", "/api/2/user", Body);
            dynamic responseJson = JsonConvert.DeserializeObject(response);

            //return USER ID
            return responseJson.accountId;
        }

        public async System.Threading.Tasks.Task CreateUser(string name, string password, string email, string displayName, string organizationID)
        {
            string userID = await CreateUser(name, password, email, displayName);
            Console.WriteLine(await setUserOrganization(organizationID, name));
        }

        public async System.Threading.Tasks.Task<string> getUser(string username)
        {
            string response = await REQAsync("GET", "/api/2/user/search?username=" + username);
            return response;
        }

        public async System.Threading.Tasks.Task<string> CreateOrganization(string name)
        {
            String Body = "{";
            Body = Body + "\"name\":\"" + name + "\"";
            Body = Body + "}";
            string response = await REQAsync("POST", "/servicedeskapi/organization", Body);
            dynamic responseJson = JsonConvert.DeserializeObject(response);

            //Return new Organization ID
            return responseJson.id;
        }

        public async System.Threading.Tasks.Task CreateOrganization(string name, string serviceDeskID)
        {
            string organizationId = await CreateOrganization(name);
            Console.WriteLine(await setOrganizationServicedesk(serviceDeskID, organizationId));
        }

        public async System.Threading.Tasks.Task<string> CreateCustomer(string email, string displayName)
        {
            String Body = "{";
            Body = Body + "\"displayName\":\"" + displayName + "\",";
            Body = Body + "\"email\":\"" + email + "\"";
            Body = Body + "}";
            string response = await REQAsync("POST", "/servicedeskapi/customer", Body);
            return response;
        }

        public async System.Threading.Tasks.Task<string> GetServicediskList()
        {
            return await REQAsync("GET", "/servicedeskapi/servicedesk");
        }

        public async Task<string> getOrganizationList()
        {
            return await REQAsync("GET", "/servicedeskapi/organization");
        }

        public async Task<string> getOrganization(string organizationId="")
        {
            return await REQAsync("GET", "/servicedeskapi/organization/"+organizationId);
        }

        public async Task<string> setUserOrganization(string organizationId = "", string username = "")
        {
            dynamic user = JsonConvert.DeserializeObject(await getUser(username));

            String Body = "{";
            Body = Body + "\"accountIds\":[\"" + user[0].accountId + "\"]";
            Body = Body + "}";

            return await REQAsync("POST", "/servicedeskapi/organization/"+ organizationId + "/user", Body);
        }

        internal async Task<string> setOrganizationServicedesk(string serviceDeskId, string organizationId)
        {
            String Body = "{";
            Body = Body + "\"organizationId\":\"" + organizationId + "\"";
            Body = Body + "}";

            return await REQAsync("POST", url: "/servicedeskapi/servicedesk/" + serviceDeskId + "/organization", Body);
        }
    }
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {

            //JObject o1 = JObject.Parse(File.ReadAllText(@"c:\videogames.json"));

            // read JSON directly from a file
            //using (StreamReader file = File.OpenText(@"c:\videogames.json"))
            //using (JsonTextReader reader = new JsonTextReader(file))
            //{
            //    JObject o2 = (JObject)JToken.ReadFrom(reader);
            //}



            JIRA jiraAPI = new JIRA("https://host/rest/");
            await jiraAPI.CreateUser("jan", "Test", "jan@zzzzz.net", "jan", "1");







            return;
        }
    }
}
