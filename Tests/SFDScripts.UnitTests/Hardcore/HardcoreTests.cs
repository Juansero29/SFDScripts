using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SFDGameScriptInterface;
using System;

namespace SFDScripts.UnitTests
{
    [TestClass]
    public class HardcoreTests
    {
        private Mock<IGame> GameMock;
        private Hardcore HardcoreGameInstance;

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var hardcore = new Hardcore();
            Assert.IsNotNull(hardcore);
        }

        [TestMethod]
        public void OnStartupTest()
        {
            var hardcore = new Hardcore();
            hardcore.OnStartup();
            Assert.IsNotNull(hardcore);
        }
    }
}
