using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Vantex.Technician.Service.Entities;

namespace Vantex.Technician.Service.Tests.Integration
{
    [TestClass]
    public class TestDeliveriesController : TestControllerBase
    {

        [TestMethod]
        public async Task TestUnauthorizedGet()
        {
            // Act
            var response = await _client.GetAsync("api/deliveries/99");

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
            var response = await _client.GetAsync("api/Deliveries/1");

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
            var response = await _client.GetAsync("api/deliveries/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<Delivery[]>(content);
            Assert.IsNotNull(list);
        }

    }
}
