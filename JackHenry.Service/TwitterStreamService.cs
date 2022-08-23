using JackHenry.Service.Utilities;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Streaming.V2;
using Tweetinvi.Exceptions;
using Tweetinvi.Models.V2;
using Tweetinvi.Events;
using System.Linq;
using SystemTimer = System.Timers;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using JackHenry.Service.Models;
using Tweetinvi.Core.Models;

namespace JackHenry.Service
{
    public class TwitterStreamService : ITwitterStreamService
    {        
        private readonly ITwitterConfigManager _twitterConfigManager;
        private List<TweetV2> tweetList = new List<TweetV2>();
        private SystemTimer.Timer _timer = new SystemTimer.Timer();

        public TwitterStreamService(ITwitterConfigManager twitterConfigManager)
        {
            _twitterConfigManager = twitterConfigManager ?? throw new ArgumentNullException();
        }

        public async Task<bool> IsUserAuthenticated()
        {
            ITwitterClient userClient = _twitterConfigManager.GetTwitterUserClient();
            IAuthenticatedUser user = await userClient.Users.GetAuthenticatedUserAsync();
            return user != null;
        }

        public async Task AnalyzeTweets()
        {
            List<string> tweets = new List<string>();
            try
            {              
                ITwitterClient appClient = _twitterConfigManager.GetTwitterAppClient();
                ISampleStreamV2 sampleStreamV2 = appClient.StreamsV2.CreateSampleStream();

                sampleStreamV2.TweetReceived += async (object? s, Tweetinvi.Events.V2.TweetV2ReceivedEventArgs e) => 
                                                       await AddTweetsToInMemory(s, e);
                
                await SetTimer();
                await sampleStreamV2.StartAsync();                
            }
            catch (TwitterException)
            {
                throw;
            }
            catch (TwitterAuthException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;

            }
            finally
            {
                StopAndDisposeTimer();
            }
        }

        private async Task SetTimer()
        {
           await Task.Run(() =>
            {
                _timer = new SystemTimer.Timer(1000);
                _timer.Elapsed += async(object? s, SystemTimer.ElapsedEventArgs e) => 
                                                                        await SendTweetsOnTimeElapsedAsync(s,e);               
                _timer.AutoReset = true;
                _timer.Enabled = true;
            });            
        }
        private async Task SendTweetsOnTimeElapsedAsync(object? sender, SystemTimer.ElapsedEventArgs e)
        {
            await Task.Run(() =>
            {               
                DateTimeOffset currentOffset = e.SignalTime;
                IEnumerable<TweetV2> tweetsDuringInterval = tweetList.Where(x => x.CreatedAt < currentOffset);

                TweetStat stat = GetTweetStat(tweetsDuringInterval);
                if (stat != null)
                {
                    stat.TweetTime = currentOffset;
                    WriteToConsole(stat);

                    int countToRemove = tweetList.Count > stat.NoOfTweets ? stat.NoOfTweets : tweetList.Count;
                    tweetList.RemoveRange(0, countToRemove);
                }
                
            });
        }

        private  TweetStat GetTweetStat(IEnumerable<TweetV2> tweets)
        {
            TweetStat tweetStat = new TweetStat();
            tweetStat.NoOfTweets = tweets.Count();
            tweetStat.Top10HashTags = GetTop10HashTags(tweets);
            tweetStat.Languages = GetLanguagesUsed(tweets);
            return tweetStat;
        }

        private string GetTop10HashTags(IEnumerable<TweetV2> tweets)
        {            
            var hashTags = tweets.Where(x => x.Entities != null && x.Entities.Hashtags != null)
                                               .SelectMany(x => x.Entities.Hashtags);

            var top10HashTagsList = hashTags?.GroupBy(h => h.Tag)
                                             .Select(x => new { HashTag = x.Key, Count = x.Count() })
                                             .OrderByDescending(o => o.Count)
                                             .Take(10)
                                             .Select(x => x.HashTag);

            return top10HashTagsList == null ? string.Empty : string.Join(",", top10HashTagsList);
        }

        private List<TweetLanguage>? GetLanguagesUsed(IEnumerable<TweetV2> tweets)
        {
             var languages = tweets.Select(x => x.Lang)?
                                            .GroupBy(l => l)
                                             .Select(x => new { Lang = x.Key, Count = x.Count() })
                                             .OrderByDescending(o => o.Count)                                             
                                             .Select(x => new TweetLanguage 
                                                           { 
                                                                LanguageUsed = x.Lang,
                                                                Count= x.Count
                                                           })?.ToList();
            return languages;
           
        }

        private void WriteToConsole(TweetStat tweetStat)
        {
            if (tweetStat == null) return;

            Console.WriteLine("//////////////////////////////////////////////////////////////////////////////////////////////////////");
            Console.WriteLine($"Tweets Information :: {tweetStat.TweetTime.ToString("0:HH:mm:ss.fff")}");
            Console.WriteLine($"No. Of Tweets received :: {tweetStat.TweetTime.ToString("0:HH:mm:ss.fff")} :: {tweetStat.NoOfTweets}");
            Console.WriteLine($"Top 10 HashTag trendings :: {tweetStat.Top10HashTags}");
            
            if (tweetStat.Languages != null)
            {
                foreach (var l in tweetStat.Languages)
                {
                    Console.WriteLine($"Language Used: {l.LanguageUsed}, No. of Tweets: {l.Count}");
                }
            }            
           
        }

        private async Task AddTweetsToInMemory(object? sender, Tweetinvi.Events.V2.TweetV2ReceivedEventArgs e)
        {
            await Task.Run(() =>
            {
                tweetList.Add(e.Tweet);
            });
        }

        public void StopAndDisposeTimer()
        {
            if(_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }            
        }
    }    
}