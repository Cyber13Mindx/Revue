using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace Revue
{
    [TestFixture]
    public class RevueTests
    {
        private RestClient client;
        private static string createdId;
        private const string baseUrl = "https://d2925tksfvgq8c.cloudfront.net/";

        [OneTimeSetUp]
        public void Setup()
        {
            string accessToken = GetJwtToken("cyber13mindx@softuni.com", "Test12345!");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(accessToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Test, Order(1)]
        public void CreateRevue_ShouldReturnOk()
        {
            var revue = new
            {
                Title = "New Revue",
                Url = "",
                Description = "Amazing Performance"
            };

            var request = new RestRequest("/api/Revue/Create", Method.Post);
            request.AddJsonBody(revue);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void GetAllRevues_ShouldReturnList()
        {
            var request = new RestRequest("/api/Revue/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var revues = JsonSerializer.Deserialize<JsonElement[]>(response.Content);
            Assert.That(revues, Is.Not.Empty);

            var lastRevue = revues[^1];
            createdId = lastRevue.GetProperty("id").GetString();
        }

        [Test, Order(3)]
        public void EditLastRevue_ShouldReturnOk()
        {
            var editedRevue = new
            {
                Title = "Edited Revue",
                Url = "",
                Description = "Even More Amazing Performance"
            };

            var request = new RestRequest($"/api/Revue/Edit?revueid={createdId}", Method.Put);
            request.AddJsonBody(editedRevue);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Edited successfully"));
        }

        [Test, Order(4)]
        public void DeleteLastRevue_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Revue/Delete?revueid={createdId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("The revue is deleted!"));
        }

        [Test, Order(5)]
        public void CreateRevue_MissingRequiredFields_ShouldReturnBadRequest()
        {
            var incompleteRevue = new
            {
                Url = "https://example.com/revue"
            };

            var request = new RestRequest("/api/Revue/Create", Method.Post);
            request.AddJsonBody(incompleteRevue);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingRevue_ShouldReturnBadRequest()
        {
            var editedRevue = new
            {
                Title = "Non-Existent Revue",
                Url = "",
                Description = "Trying to edit something that doesn't exist"
            };

            var request = new RestRequest("/api/Revue/Edit?revueid=000", Method.Put);
            request.AddJsonBody(editedRevue);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("There is no such revue!"));
        }

        [Test, Order(7)]
        public void DeleteNonExistingRevue_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Revue/Delete?revueid=000", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("There is no such revue!"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}