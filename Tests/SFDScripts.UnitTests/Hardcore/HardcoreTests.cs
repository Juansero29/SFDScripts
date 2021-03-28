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
            //GameMock = new Mock<IGame>();
            //GameMock.Setup(m => m.GetSingleObjectByCustomId(It.IsAny<string>())).Returns(() => new IObjectDestroyTargets()());
            HardcoreGameInstance = new Hardcore(GameMock.Object);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var hardcore = new Hardcore();
            Assert.IsNotNull(hardcore);
        }
    }
}
