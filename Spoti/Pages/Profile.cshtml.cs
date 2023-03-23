using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Spoti;
using SpotifyAPI.Web;

namespace Spoti.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly SpotifyClientBuilder _spotifyClientBuilder;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(SpotifyClientBuilder spotifyClientBuilder, ILogger<ProfileModel> logger)
        {
            _spotifyClientBuilder = spotifyClientBuilder;
            _logger = logger;
        }

        public PrivateUser Me { get; set; }
        public IList<FullAlbum> LastFiveAlbums { get; set; }


        public async Task OnGet()
        {
            var spotify = await _spotifyClientBuilder.BuildClient();

            Me = await spotify.UserProfile.Current();

            var recentlyPlayedRequest = new PlayerRecentlyPlayedRequest
            {
                Limit = 50
            };
            var recentlyPlayed = await spotify.Player.GetRecentlyPlayed(recentlyPlayedRequest);

            // Extract unique albums
            var albumIds = new HashSet<string>();
            LastFiveAlbums = new List<FullAlbum>();
            foreach (var item in recentlyPlayed.Items)
            {
                if (albumIds.Add(item.Track.Album.Id))
                {
                    LastFiveAlbums.Add(await spotify.Albums.Get(item.Track.Album.Id));
                    if (LastFiveAlbums.Count >= 5)
                    {
                        break;
                    }
                }
            }
        }

        public async Task<IActionResult> OnPost()
        {
            await HttpContext.SignOutAsync();
            return Redirect("https://google.com");
        }
    }
}
