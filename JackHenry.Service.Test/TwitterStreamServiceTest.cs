using JackHenry.Service.Utilities;
using Moq;
using Tweetinvi;
using Tweetinvi.Core.DTO;
using Tweetinvi.Core.Models;
using Tweetinvi.Models;
using Tweetinvi.Streaming.V2;
using Tweetinvi.Streams;
using Tweetinvi.Streams.Helpers;

namespace JackHenry.Service.Test
{
    public class TwitterStreamServiceTest
    {
        private Mock<ITwitterClient>? _twitterUserClientMock;
        private Mock<ITwitterClient>? _twitterAppClientMock;
        private Mock<ITwitterConfigManager>? _twitterConfigManagerMock;
        private Mock<ISampleStreamV2> _sampleStreamV2Mock;

        private ITwitterStreamService? _twitterStreamService;

        [SetUp]
        public void Setup()
        {
            _twitterConfigManagerMock = new Mock<ITwitterConfigManager>();
            _twitterUserClientMock = new Mock<ITwitterClient>();
            _twitterAppClientMock = new Mock<ITwitterClient>();
            _sampleStreamV2Mock = new Mock<ISampleStreamV2>();
        }

        [Test]
        public async Task UserAuthenticated_ValidUser()
        {
            IAuthenticatedUser authenticatedUser = new AuthenticatedUser(new UserDTO(), _twitterUserClientMock.Object);

            _twitterUserClientMock.Setup(x => x.Users.GetAuthenticatedUserAsync()).Returns(Task.FromResult(authenticatedUser));

            _twitterConfigManagerMock.Setup(x => x.GetTwitterUserClient()).Returns(_twitterUserClientMock.Object);
            _twitterStreamService = new TwitterStreamService(_twitterConfigManagerMock.Object);

            bool isAuthenticated = await _twitterStreamService.IsUserAuthenticated();
            Assert.True(isAuthenticated);
        }

        [Test]
        public async Task UserAuthenticated_NullUser()
        {            
            _twitterUserClientMock.Setup(x => x.Users.GetAuthenticatedUserAsync()).Returns(Task.FromResult( (IAuthenticatedUser?)null));

            _twitterConfigManagerMock.Setup(x => x.GetTwitterUserClient()).Returns(_twitterUserClientMock.Object);
            _twitterStreamService = new TwitterStreamService(_twitterConfigManagerMock.Object);

            bool isAuthenticated = await _twitterStreamService.IsUserAuthenticated();
            Assert.False(isAuthenticated);
        }

        [Test]
        public async Task AnalyzeTweets_Valid()
        {
            _twitterAppClientMock.Setup(x => x.StreamsV2.CreateSampleStream()).Returns(_sampleStreamV2Mock.Object);
            _twitterConfigManagerMock.Setup(x => x.GetTwitterAppClient()).Returns(_twitterAppClientMock.Object);
            _twitterStreamService = new TwitterStreamService(_twitterConfigManagerMock.Object);
            await _twitterStreamService.AnalyzeTweets();
            Mock.Get(_twitterAppClientMock.Object).Verify(x => x.StreamsV2.CreateSampleStream(), Times.Once);
        }

        [Test]
        public async Task AnalyzeTweets_Exception()
        {           
            _twitterConfigManagerMock.Setup(x => x.GetTwitterAppClient()).Returns(_twitterAppClientMock.Object);
            _twitterStreamService = new TwitterStreamService(_twitterConfigManagerMock.Object);          
            Assert.ThrowsAsync<NullReferenceException>(() =>  _twitterStreamService.AnalyzeTweets());
        }
    }
}