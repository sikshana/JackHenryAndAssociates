using Microsoft.Extensions.Configuration;
using Tweetinvi;

namespace JackHenry.Service.Utilities
{
    public interface ITwitterConfigManager
    {
        string APIKey { get; }
        string APIKeySecret { get; }
        string BearerToken { get; }
        string AccessToken { get; }
        string AccessTokenSecret { get; }      
        ITwitterClient GetTwitterUserClient();
        ITwitterClient GetTwitterAppClient();
    }
}
