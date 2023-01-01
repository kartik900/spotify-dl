using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;

namespace spotify_dl
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string spotifyAuthURL = "https://accounts.spotify.com/api/token";
            const string spotifyBaseURL = "https://api.spotify.com/v1/";

            using (var client = new HttpClient())
            {
                string jwtToken = string.Empty;
                HttpResponseMessage response;

                if (!CheckTokenIsValid(jwtToken))
                {
                    string clientId = "hoven";
                    string clientSecret = "1234";

                    string header = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", header);

                    response = await client.GetAsync(spotifyAuthURL);
                    using (response)
                    {
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch
                        {
                            Console.WriteLine("\nAUTH ERROR");
                            return;
                        }

                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                        jwtToken = jsonObject.access_token;

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    }
                }




            }





            Console.WriteLine("Hello, World!");





            Console.ReadLine();
        }

        private static long GetTokenExpirationTime(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals("exp")).Value;
            var ticks = long.Parse(tokenExp);
            return ticks;
        }

        private static bool CheckTokenIsValid(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            var tokenTicks = GetTokenExpirationTime(token);
            var tokenDate = DateTimeOffset.FromUnixTimeSeconds(tokenTicks).UtcDateTime;

            var now = DateTime.Now.ToUniversalTime();

            return tokenDate >= now;
        }

    }
}