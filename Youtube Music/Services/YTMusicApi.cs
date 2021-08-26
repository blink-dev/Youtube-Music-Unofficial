using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Youtube_Music.Extensions;
using Youtube_Music.Models;

namespace Youtube_Music.Services
{
    public class YTMusicApi
    {
        public enum FilterType
        {
            All,
            Songs,
            Videos,
            Albums,
            Playlist
        }

        private static string GetFilterType(FilterType type)
        {
            return type switch
            {
                FilterType.All => "",
                FilterType.Songs => "EgWKAQIIAWoKEAMQBBAJEAUQCg%3D%3D",
                FilterType.Videos => "EgWKAQIQAWoKEAMQBBAJEAUQCg%3D%3D",
                FilterType.Albums => "",
                FilterType.Playlist => "",
                _ => "",
            };
        }

        public string continuation = "";

        const string PARAMS = "?alt=json&key=AIzaSyC9XL3ZjWddXya6X74dJoCTL-WEYFDNX30";
        const string BASE_URL = "https://music.youtube.com/youtubei/v1/";

        readonly HttpClient client = new();

        public YTMusicApi()
        {
            dynamic headers = JsonConvert.DeserializeObject(File.ReadAllText(Environment.CurrentDirectory + "/headers.json"));
            foreach (var item in headers)
            {
                if (item.Name == "Content-Type") continue;
                client.DefaultRequestHeaders.Add(item.Name, item.Value.Value);
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> Browse(string browseId, string find, int findIndex, int findCount = 0)
        {
            var context = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Environment.CurrentDirectory + "/context.json"));
            context.Add("browseId", browseId);
            HttpContent content = new StringContent(context.ToString());
            var response = await client.PostAsync($"{BASE_URL}browse{PARAMS}", content);
            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var obj = await new StreamReader(responseStream).ReadToEndAsync().ConfigureAwait(false);
            var arr = JObject.Parse(obj);
            var randomIds = arr.FindTokens(find, findCount);
            return randomIds[findIndex].Value<string>();
        }

        public async Task<string[]> GetSearchSuggestions(string input)
        {
            if (string.IsNullOrEmpty(input)) return Array.Empty<string>();
            var context = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Environment.CurrentDirectory + "/context.json"));
            context.Add("input", input);
            HttpContent content = new StringContent(context.ToString());
            var response = await client.PostAsync($"{BASE_URL}music/get_search_suggestions{PARAMS}", content);
            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var obj = await new StreamReader(responseStream).ReadToEndAsync().ConfigureAwait(false);
            var arr = JObject.Parse(obj);
            var contents = arr.SelectToken("contents");
            if (contents is null) return Array.Empty<string>();
            var cont = contents[0].SelectToken("searchSuggestionsSectionRenderer.contents");
            var suggestions = new List<string>();
            foreach (var sugg in cont)
            {
                var runs = sugg.SelectToken("searchSuggestionRenderer.suggestion.runs");
                string finaltext = string.Concat(runs.Values<string>("text"));
                suggestions.Add(finaltext);
            }
            return suggestions.ToArray();
        }

        public async Task<SearchShelf[]> Search(string input, FilterType filter = FilterType.All)
        {
            var context = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Environment.CurrentDirectory + "/context.json"));
            if (filter != FilterType.All) context.Add("params", GetFilterType(filter));
            context.Add("query", input);
            HttpContent content = new StringContent(context.ToString());
            var response = await client.PostAsync($"{BASE_URL}search{PARAMS}", content);
            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var obj = await new StreamReader(responseStream).ReadToEndAsync().ConfigureAwait(false);
            var arr = JObject.Parse(obj);
            //var cont = arr.SelectToken("contents.sectionListRenderer.contents");
            var cont = arr.SelectToken("contents.tabbedSearchResultsRenderer.tabs.[0].tabRenderer.content.sectionListRenderer.contents");

            List<SearchShelf> results = new();
            foreach (var sugg in cont)
            {
                var contents = sugg.SelectToken("musicShelfRenderer.contents");
                if (contents is null) continue;

                var shelfTitle = string.Concat(sugg.SelectToken("musicShelfRenderer.title.runs").Values<string>("text"));
                var videos = new List<YoutubeVideoInfo>();

                foreach (var item in contents)
                {
                    var thumb = item.SelectToken("musicResponsiveListItemRenderer.thumbnail.musicThumbnailRenderer.thumbnail.thumbnails")[0].Value<string>("url");
                    var columns = item.SelectToken("musicResponsiveListItemRenderer.flexColumns");

                    var videoIdColumn = columns[0].SelectToken("musicResponsiveListItemFlexColumnRenderer.text.runs")[0].SelectToken("navigationEndpoint.watchEndpoint");
                    if (videoIdColumn is null) continue;
                    var videoId = videoIdColumn.Value<string>("videoId");

                    var titleRuns = columns[0].SelectToken("musicResponsiveListItemFlexColumnRenderer.text.runs");
                    var title = string.Concat(titleRuns.Values<string>("text"));

                    var artistsRuns = columns[1].SelectToken("musicResponsiveListItemFlexColumnRenderer.text.runs");
                    var artists = string.Concat(artistsRuns.Values<string>("text").SkipLast(4));

                    var duration = string.Concat(artistsRuns.Values<string>("text").Last());

                    YoutubeVideoInfo videoInfo = new()
                    {
                        Artist = artists,
                        Title = title,
                        VideoId = videoId,
                        Duration = duration,
                        Thumbnail = new Uri(thumb)
                    };

                    videos.Add(videoInfo);
                }

                if (videos.Count.Equals(0)) continue;

                SearchShelf searchShelf = new()
                {
                    Title = shelfTitle,
                    Videos = videos.ToArray()
                };
                results.Add(searchShelf);
            }
            return results.ToArray();
        }

        public async Task<YoutubeVideoInfo[]> UpNext(string id, bool shuffle = false)
        {
            var context = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Environment.CurrentDirectory + "/context.json"));
            context.Add("params", "wAEB");
            context.Add("videoId", id);
            HttpContent content = new StringContent(context.ToString());
            var response = await client.PostAsync($"{BASE_URL}next{PARAMS}", content);
            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var obj = await new StreamReader(responseStream).ReadToEndAsync().ConfigureAwait(false);
            var arr = JObject.Parse(obj);
            var cont = arr.SelectToken("contents.singleColumnMusicWatchNextResultsRenderer.tabbedRenderer.watchNextTabbedResultsRenderer.tabs");
            if (cont is null) return Array.Empty<YoutubeVideoInfo>();

            List<YoutubeVideoInfo> results = new();

            JToken ctoken = cont[0].SelectToken("tabRenderer.content.musicQueueRenderer.content.playlistPanelRenderer.continuations");
            if (ctoken != null)
            {
                JToken continuations = ctoken[0].SelectToken("nextRadioContinuationData");
                if (continuations != null)
                {
                    continuation = continuations.Value<string>("continuation");
                }
            }
            var contents = cont[0].SelectToken("tabRenderer.content.musicQueueRenderer.content.playlistPanelRenderer.contents");

            foreach (var item in contents)
            {
                try
                {
                    var thumb = item.SelectToken("playlistPanelVideoRenderer.thumbnail.thumbnails")[0].Value<string>("url");

                    var titleRuns = item.SelectToken("playlistPanelVideoRenderer.title.runs");
                    var title = string.Concat(titleRuns.Values<string>("text"));

                    var artistsRuns = item.SelectToken("playlistPanelVideoRenderer.shortBylineText.runs");
                    var artists = string.Concat(artistsRuns.Values<string>("text"));

                    var durationColumn = item.SelectToken("playlistPanelVideoRenderer.lengthText.runs");
                    var duration = string.Concat(durationColumn.Values<string>("text"));

                    var endpoint = item.SelectToken("playlistPanelVideoRenderer.navigationEndpoint.watchEndpoint");

                    var index = endpoint.Value<int>("index");
                    var videoId = endpoint.Value<string>("videoId");
                    var playlistId = endpoint.Value<string>("playlistId");
                    var p = endpoint.Value<string>("params");
                    var playerParams = endpoint.Value<string>("playerParams");

                    YoutubeVideoInfo videoInfo = new()
                    {
                        Artist = artists,
                        Title = title,
                        VideoId = videoId,
                        Duration = duration,
                        Thumbnail = new Uri(thumb),
                        PlaylistId = playlistId,
                        Index = index,
                        Params = p,
                        PlayerParams = playerParams
                    };
                    results.Add(videoInfo);
                }
                catch (Exception)
                {
                    continue;
                }

            }
            if (shuffle)
            {
                return results.ToArray().Randomize();
            }
            return results.ToArray();
        }

        public async Task<YoutubeVideoInfo[]> UpNext(YoutubeVideoInfo info)
        {
            if (info is null) return Array.Empty<YoutubeVideoInfo>();
            var context = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Environment.CurrentDirectory + "/context.json"));
            context.Add("enablePersistentPlaylistPanel", true);
            context.Add("isAudioOnly", true);
            context.Add("continuation", continuation);
            context.Add("params", info.Params);
            context.Add("playerParams", info.PlayerParams);
            context.Add("playlistId", info.PlaylistId);
            context.Add("index", info.Index);
            context.Add("videoId", info.VideoId);
            HttpContent content = new StringContent(context.ToString());
            var response = await client.PostAsync($"{BASE_URL}next{PARAMS}", content);
            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var obj = await new StreamReader(responseStream).ReadToEndAsync().ConfigureAwait(false);
            var arr = JObject.Parse(obj);
            //var cont = arr.SelectToken("contents.singleColumnMusicWatchNextResultsRenderer.tabbedRenderer.watchNextTabbedResultsRenderer.tabs");
            var cont = arr.SelectToken("continuationContents");
            if (cont is null) return Array.Empty<YoutubeVideoInfo>();

            List<YoutubeVideoInfo> results = new();

            continuation = cont.SelectToken("playlistPanelContinuation.continuations")[0].SelectToken("nextRadioContinuationData").Value<string>("continuation");
            //var contents = cont[0].SelectToken("tabRenderer.content.musicQueueRenderer.content.playlistPanelRenderer.contents");
            var contents = cont.SelectToken("playlistPanelContinuation.contents");

            foreach (var item in contents)
            {
                var thumb = item.SelectToken("playlistPanelVideoRenderer.thumbnail.thumbnails")[0].Value<string>("url");

                var titleRuns = item.SelectToken("playlistPanelVideoRenderer.title.runs");
                var title = string.Concat(titleRuns.Values<string>("text"));

                var artistsRuns = item.SelectToken("playlistPanelVideoRenderer.shortBylineText.runs");
                var artists = string.Concat(artistsRuns.Values<string>("text"));

                var durationColumn = item.SelectToken("playlistPanelVideoRenderer.lengthText.runs");
                var duration = string.Concat(durationColumn.Values<string>("text"));

                var endpoint = item.SelectToken("playlistPanelVideoRenderer.navigationEndpoint.watchEndpoint");

                var index = endpoint.Value<int>("index");
                var videoId = endpoint.Value<string>("videoId");
                var playlistId = endpoint.Value<string>("playlistId");
                var p = endpoint.Value<string>("params");
                var playerParams = endpoint.Value<string>("playerParams");

                YoutubeVideoInfo videoInfo = new()
                {
                    Artist = artists,
                    Title = title,
                    VideoId = videoId,
                    Duration = duration,
                    Thumbnail = new Uri(thumb),
                    PlaylistId = playlistId,
                    Index = index,
                    Params = p,
                    PlayerParams = playerParams
                };
                results.Add(videoInfo);
            }
            return results.ToArray();
        }
    }
}
