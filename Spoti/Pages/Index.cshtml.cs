using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spoti;
using SpotifyAPI.Web;

namespace Spoti.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SpotifyClientBuilder _spotifyClientBuilder;

        public List<FullAlbum> NewReleases { get; set; }

        public IndexModel(SpotifyClientBuilder spotifyClientBuilder)
        {
            _spotifyClientBuilder = spotifyClientBuilder;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
              
                return RedirectToPage("/Login");
            }

            try
            {
                var spotify = await _spotifyClientBuilder.BuildClient();

                var newReleasesRequest = new NewReleasesRequest
                {
                    Limit = 20
                };
                var response = await spotify.Browse.GetNewReleases(newReleasesRequest);
                NewReleases = response.Albums.Items.Select(album => spotify.Albums.Get(album.Id).Result).ToList();
            }
            catch (InvalidOperationException ex)
            {
                
            }

            return Page();
        }
    }


}


