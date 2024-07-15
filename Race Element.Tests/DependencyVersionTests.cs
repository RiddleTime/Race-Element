using System.Reflection;

namespace RaceElement.Tests
{
    public class DependencyVersionTests
    {

        /// <summary>
        /// Due to a missing method in a more recent version of the Websocket.Client package, the latest version we can currently use is 5.0.0.0, this test enforces that.
        /// </summary>
        [Fact]
        public void EnsureWebsocketClientPackageToBeCorrectVersion()
        {
            Assert.Equal("5.0.0.0", Assembly.GetAssembly(typeof(Websocket.Client.IWebsocketClient)).GetName().Version.ToString());
        }
    }
}