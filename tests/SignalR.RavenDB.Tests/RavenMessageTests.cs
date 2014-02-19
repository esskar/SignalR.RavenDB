using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client.Embedded;

namespace SignalR.RavenDB.Tests
{
    [TestClass]
    public class RavenMessageTests
    {
        private EmbeddableDocumentStore _documentStore;

        [TestInitialize]
        public void Initialize()
        {
            _documentStore = new EmbeddableDocumentStore();
            _documentStore.Initialize();
        }

        [TestCleanup]
        public void Release()
        {
            _documentStore.Dispose();
        }

        [TestMethod]
        public void ToLongId()
        {
            AssertToLongId("RavenMessages/1", 1);
            AssertToLongId("RavenMessages/5", 5);
            AssertToLongId("RavenMessages/456", 456);
            AssertToLongId("RavenMessages/13489723067230", 13489723067230);
            AssertToLongId("RavenMessages/Prefix/1", 1);
            AssertToLongId("RavenMessages/Prefix/5", 5);
            AssertToLongId("RavenMessages/Prefix/456", 456);
            AssertToLongId("RavenMessages/Prefix/13489723067230", 13489723067230);
        }

        [TestMethod]
        public void StoreAndRetrieve()
        {
            var msg = new RavenMessage {StreamIndex = 1234};
            using (var session = _documentStore.OpenSession())
            {
                session.Store(msg);
                Assert.IsNotNull(msg.Id, "MessageId is null after store.");
                Assert.AreNotEqual(string.Empty, msg.Id, "MessageId is empty after store.");
                session.SaveChanges();
            }
            using (var session = _documentStore.OpenSession())
            {
                var loadedMsg = session.Load<RavenMessage>(msg.Id);
                Assert.AreEqual(msg.Id, loadedMsg.Id, "Id mismatch.");
                Assert.AreEqual(msg.StreamIndex, loadedMsg.StreamIndex, "StreamIndex mismatch.");
            }
        }

        private static void AssertToLongId(string id, ulong expected)
        {
            var msg = new RavenMessage { Id = id };
            Assert.AreEqual(expected, msg.ToLongId(), "AssertToLongId failed with id '{0}'", id);
        }
    }
}
