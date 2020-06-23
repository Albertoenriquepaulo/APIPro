using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vantex.Technician.Service.Entities;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Vantex.Technician.Service.Tests.Integration
{
    [TestClass]
    class TestPickUpsController : TestControllerBase
    {
        [TestMethod]
        public async Task TestUnauthorizedGet()
        {
            // Act
            var response = await _client.GetAsync("api/pickups/3");

            // Assert
            Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TestAuthorizedGet()
        {
            // Arrange
            var token = await GetValidToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            // Act
            var response = await _client.GetAsync("api/pickups/3");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [TestMethod]
        public async Task TestGetIsArray()
        {
            // Arrange
            var token = await GetValidToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            // Act
            var response = await _client.GetAsync("api/pickups/3");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<PickUp[]>(content);
            Assert.IsNotNull(list);
        }
    }
}
