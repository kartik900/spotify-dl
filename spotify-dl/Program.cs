using Newtonsoft.Json;
using spotify_dl.models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace spotify_dl
{
    internal class Program
    {
        const string spotifyAuthURL = "https://accounts.spotify.com/api/token";
        const string spotifyBaseURL = "https://api.spotify.com/v1/";
        static async Task Main(string[] args)
        {
            string userId = "oe3gsexewf8tom82dhq8qevqe";

            string? jwtToken = string.Empty;

            var userPlaylistData = new UserPlaylists();
            using (var client = new HttpClient())
            {
                if (string.IsNullOrEmpty(jwtToken)) jwtToken = await GenerateToken(client);
                userPlaylistData = await GetUserPlaylists(userId, client);
                var allTrackIds = await GetTrackIDs(userPlaylistData.Items, client);
                string newPlaylistId = await CreatePlaylist();
                AddTracksToPlaylist(allTrackIds,newPlaylistId);
            }


            Console.ReadLine();
        }

        private static async Task<string> CreatePlaylist()
        {
            HttpResponseMessage response;
            string newPlaylistId = string.Empty;
            using (var client = new HttpClient())
            {
                string refreshToken = await RefreshAccessToken(client);
                var newPlaylistRequestObject = new Dictionary<string, string>();
                newPlaylistRequestObject.Add("name", "spot-dl");
                newPlaylistRequestObject.Add("description", "spot-DL");
                newPlaylistRequestObject.Add("public", "false");

                var newPlaylistRequestObjectJson = JsonConvert.SerializeObject(newPlaylistRequestObject);
                string t = spotifyBaseURL + "users/oe3gsexewf8tom82dhq8qevqe/playlists";
                response = await client.PostAsync(t, new StringContent(newPlaylistRequestObjectJson));
            }
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var createPlaylistResponse = JsonConvert.DeserializeObject<CreatePlaylistResponse>(jsonResponse);
                newPlaylistId = createPlaylistResponse.Id;
            }
            return newPlaylistId;


        }

        private static async void AddTracksToPlaylist(List<string> tracks, string newPlaylistId)
        {
            tracks = tracks.Distinct().ToList();
            var subTrackLists = Split<string>(tracks);
            using (var client = new HttpClient())
            {
                string refreshToken = await RefreshAccessToken(client);
                foreach (var subTrackList in subTrackLists)
                {
                    UploadTrackList trackList = new UploadTrackList() { uris = subTrackList };
                    var trackListJson = JsonConvert.SerializeObject(trackList);
                    var response = await client.PostAsync(spotifyBaseURL + "playlists/"+newPlaylistId+"/tracks", new StringContent(trackListJson));
                    if (!response.IsSuccessStatusCode)
                        return;
                }
            }
        }


        public static List<List<T>> Split<T>(IList<T> source)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 100)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        private static async Task<List<string>> GetTrackIDs(List<Playlist> playlists, HttpClient client)
        {
            List<string> trackURIs = new();

            foreach (var playlist in playlists)
            {
                PlaylistData? playlistData = new();
                var response = await client.GetAsync(playlist?.Tracks?.Href);
                if (response.IsSuccessStatusCode)
                {
                    playlistData = await response.Content.ReadFromJsonAsync<PlaylistData>();

                    if (playlistData?.Items?.Count > 0)
                        foreach (var trackItem in playlistData.Items)
                            trackURIs.Add(trackItem?.Track?.Uri);

                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await GenerateToken(client);
                    return await GetTrackIDs(playlists, client);
                }
            }
            return trackURIs;
        }

        private static async Task<UserPlaylists?> GetUserPlaylists(string userId, HttpClient client)
        {

            var response = await client.GetAsync(spotifyBaseURL + "users/oe3gsexewf8tom82dhq8qevqe/playlists?limit=50");
            try
            {
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<UserPlaylists>();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await GenerateToken(client);
                    return await GetUserPlaylists(userId, client);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        private static async Task<string?> GenerateToken(HttpClient client)
        {
            string? jwtToken = string.Empty;
            HttpResponseMessage response;

            string clientId = "f6fb8360f5374a8d8c99961b2aad6428";
            string clientSecret = "b160a342f56e43b2afa2181d606306ac";

            string header = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", header);

            var form = new List<KeyValuePair<string?, string?>>{
                        new KeyValuePair<string?, string?>("grant_type", "client_credentials")
                };

            response = await client.PostAsync(spotifyAuthURL, new FormUrlEncodedContent(form));
            using (response)
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch
                {
                    Console.WriteLine("\nAUTH ERROR");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                jwtToken = jsonObject?.access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                return jwtToken;
            }
        }

        private static async Task<string?> RefreshAccessToken(HttpClient client)
        {
            string? jwtToken = string.Empty;
            HttpResponseMessage response;
            string clientId = "f6fb8360f5374a8d8c99961b2aad6428";
            string clientSecret = "b160a342f56e43b2afa2181d606306ac";

            string header = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", header);

            string refreshToken = "AQAXPcK4L0woG-1gt78UH4Emc9FPP0VJBecaCZqPTvY4Xyk-5rl-fds5lWSDOL5bvRRzu4tU8kTS1YHVg7hGflhOPt9gJnU5fgxLE2pwPJJ43rKqNVqEybkfA9HewW-DFdY";
            var form = new List<KeyValuePair<string?, string?>>{
                        new KeyValuePair<string?, string?>("grant_type", "refresh_token"),
                        new KeyValuePair<string?, string?>("refresh_token", refreshToken)
                };

            response = await client.PostAsync(spotifyAuthURL, new FormUrlEncodedContent(form));
            using (response)
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch
                {
                    Console.WriteLine("\nAUTH ERROR");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                jwtToken = jsonObject?.access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                return jwtToken;
            }
        }
    }
}