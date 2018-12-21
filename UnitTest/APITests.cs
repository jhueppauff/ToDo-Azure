using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using API;
using System.Threading.Tasks;
using FluentAssertions;

namespace UnitTest
{
    [TestClass]
    public class ApiTests
    {
        private TestServer api;

        [TestInitialize]
        public void Initialize()
        {
            this.api = new TestServer(WebHost
                .CreateDefaultBuilder()
                .UseStartup<Startup>());
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.api.Dispose();
        }

        [TestMethod]
        public async Task Value_Action_Should_Return_Value()
        {
            var response = await this.api.CreateRequest("/api/v1/values").GetAsync();
            var data = await response.Content.ReadAsStringAsync();

            data.Should().NotBeNullOrWhiteSpace();
        }
    }
}
