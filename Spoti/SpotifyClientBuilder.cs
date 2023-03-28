using Microsoft.AspNetCore.Authentication;
using SpotifyAPI.Web;
using Microsoft.Extensions.Caching.Memory;


namespace Spoti
{

    public class SpotifyClientBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SpotifyClientConfig _spotifyClientConfig;
        private readonly IMemoryCache _memoryCache;

        public SpotifyClientBuilder(IHttpContextAccessor httpContextAccessor, SpotifyClientConfig spotifyClientConfig, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _spotifyClientConfig = spotifyClientConfig;
            _memoryCache = memoryCache;

        }

        public async Task<SpotifyClient> BuildClient()
        {
            var token = await _httpContextAccessor.HttpContext.GetTokenAsync("Spotify", "access_token");

            return new SpotifyClient(_spotifyClientConfig.WithToken(token));
        }
    }
}
