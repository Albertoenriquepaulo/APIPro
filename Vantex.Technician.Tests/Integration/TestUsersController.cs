using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using Vantex.Technician.Service;
using Vantex.Technician.Service.Entities;

namespace Vantex.Technician.Service.Tests.Integration
{
    [TestClass]
    public class TestUsersController : TestControllerBase
    {

        [TestMethod]
        public async Task TestValidAuth()
        {
            const string probeId = "50020A60134999";//"50020A4422B781";
            const string userPass = "123456";
            // Act
            var response = await _client.PostAsJsonAsync("api/users/authenticate", new User
            {
                ProbeId = probeId,
                Password = userPass
            });
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadAsAsync<User>();

            // Assert
            Assert.AreEqual(user.ProbeId, probeId);
            Assert.IsNotNull(user.Token);
        }

        [TestMethod]
        public async Task TestInvalidAuthWOnlyProbeID()
        {
            // Act
            var response = await _client.PostAsJsonAsync("Authenticate", new User
            {
                ProbeId = "234234234"
            });

            // Assert
            Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestInvalidAuthWProbeIDAndPass()
        {
            // Act
            var response = await _client.PostAsJsonAsync("Authenticate", new User
            {
                ProbeId = "50020A60134999",
                Password = "1234567"  //wrong password
            });

            // Assert
            Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestInvalidAuthWPassEmpty()
        {
            // Act
            var response = await _client.PostAsJsonAsync("Authenticate", new User
            {
                ProbeId = "50020A60134999",
                Password = ""
            });

            // Assert
            Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestInvalidAuthWProbeIDEmpty()
        {
            // Act
            var response = await _client.PostAsJsonAsync("Authenticate", new User
            {
                ProbeId = "",
                Password = "123456"
            });

            // Assert
            Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestInvalidAuthWProbeIDAndPassEmpty()
        {
            // Act
            var response = await _client.PostAsJsonAsync("Authenticate", new User
            {
                ProbeId = "",
                Password = ""
            });

            // Assert
            Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.NotFound);
        }
    }
}
