using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vantex.Technician.Service.Entities;

namespace Vantex.Technician.Service.Tests.Integration
{
    public abstract class TestControllerBase
    {

        protected readonly TestServer _server;
        protected readonly HttpClient _client;

        protected TestControllerBase()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            _server = new TestServer(new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        protected async Task<string> GetValidToken()
        {
            const string probeId = "50020A60134999";//"50020A4422B781";
            const string userPass = "123456";
            var response = await _client.PostAsJsonAsync("api/users/authenticate", new User
            {
                ProbeId = probeId,
                Password = userPass
            });
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadAsAsync<User>();
            return user.Token;
        }

    }
}
