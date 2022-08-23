namespace JackHenry.Service
{
    public interface ITwitterStreamService
    {
        Task<bool> IsUserAuthenticated();
        Task AnalyzeTweets();
        void StopAndDisposeTimer();
    }
}
