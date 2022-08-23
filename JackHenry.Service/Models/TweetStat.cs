namespace JackHenry.Service.Models
{
    public class TweetStat
    {
        public DateTimeOffset TweetTime { get; set; }
        public int NoOfTweets { get; set; }
        public string? Top10HashTags { get; set; }
        public List<TweetLanguage>? Languages { get; set; }
    }

}
