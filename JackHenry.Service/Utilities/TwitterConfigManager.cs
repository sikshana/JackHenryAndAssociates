using Microsoft.Extensions.Configuration;
using Tweetinvi.Models;
using Tweetinvi;

namespace JackHenry.Service.Utilities
{
    public class TwitterConfigManager : ITwitterConfigManager
    {
        private readonly IConfiguration _configuration;
        public TwitterConfigManager(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public string APIKey => _configuration["TwitterCredential:APIKey"];
        public string APIKeySecret => _configuration["TwitterCredential:APIKeySecret"];

        public string BearerToken => _configuration["TwitterCredential:BearerToken"];
        public string AccessToken => _configuration["TwitterCredential:AccessToken"];
        public string AccessTokenSecret => _configuration["TwitterCredential:AccessTokenSecret"];
      
        public ITwitterClient GetTwitterUserClient()
        {           
            ITwitterClient userClient = new TwitterClient(APIKey,APIKeySecret,AccessToken,AccessTokenSecret);
            return userClient;
        }

        public ITwitterClient GetTwitterAppClient()
        {
            var appCredentials = new ConsumerOnlyCredentials(APIKey, APIKeySecret)
            {
                BearerToken = BearerToken 
            };

            var appClient = new TwitterClient(appCredentials);
            return appClient;
        }
    }
}
