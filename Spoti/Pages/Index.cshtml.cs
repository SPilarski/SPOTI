using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spoti;
using SpotifyAPI.Web;

namespace Spoti.Pages
{
    public class IndexModel : PageModel
    {
        private const int LIMIT = 10;
        private readonly SpotifyClientBuilder _spotifyClientBuilder;

        public Paging<SimplePlaylist> Playlists { get; set; }

        public string Next { get; set; }
        public string Previous { get; set; }

        public IndexModel(SpotifyClientBuilder spotifyClientBuilder)
        {
            _spotifyClientBuilder = spotifyClientBuilder;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect to the login page or display an appropriate message
                return RedirectToPage("/Login");
            }

            try
            {
                var spotify = await _spotifyClientBuilder.BuildClient();

                int offset = int.TryParse(Request.Query["Offset"], out offset) ? offset : 0;
                var playlistRequest = new PlaylistCurrentUsersRequest
                {
                    Limit = LIMIT,
                    Offset = offset
                };
                Playlists = await spotify.Playlists.CurrentUsers(playlistRequest);

                if (Playlists.Next != null)
                {
                    Next = Url.Page("Index", new { Offset = offset + LIMIT });
                }
                if (Playlists.Previous != null)
                {
                    Previous = Url.Page("Index", values: new { Offset = offset - LIMIT });
                }
            }
            catch (InvalidOperationException ex)
            {
                // Handle the error by displaying a message or logging the exception
                // For example, you can set an ErrorMessage property and display it on the page
            }

            return Page();
        }
    }
}
