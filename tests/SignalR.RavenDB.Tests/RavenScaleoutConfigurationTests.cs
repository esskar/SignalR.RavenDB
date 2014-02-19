using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client;

namespace SignalR.RavenDB.Tests
{
    [TestClass]
    public class RavenScaleoutConfigurationTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsForNullConnectionString()
        {
            Assert.IsNotNull(new RavenScaleoutConfiguration((string) null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsForEmptyConnectionString()
        {
            Assert.IsNotNull(new RavenScaleoutConfiguration(string.Empty));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsForNullConnectionFactory()
        {
            Assert.IsNotNull(new RavenScaleoutConfiguration((Func<IDocumentStore>)null));
        }

        [TestMethod]
        public void ConstructorSetsDefaultValues()
        {
            var cfg = new RavenScaleoutConfiguration("back_plane");
            Assert.AreEqual(TimeSpan.Zero, cfg.Expiration, "Expiration not set correctly.");
            Assert.AreEqual(TimeSpan.FromSeconds(2), cfg.ReconnectDelay, "ReconnectDelay not set correctly.");
        }
    }
}
