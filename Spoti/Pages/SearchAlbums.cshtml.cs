using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spoti;
using SpotifyAPI.Web;

namespace Spoti.Pages
{
    public class SearchAlbumsModel : PageModel
    {
        private readonly SpotifyClientBuilder _spotifyClientBuilder;
        public PrivateUser Me { get; set; }
        public SearchAlbumsModel(SpotifyClientBuilder spotifyClientBuilder)
        {
            _spotifyClientBuilder = spotifyClientBuilder;
        }
        private async Task SetMe()
        {
            var spotify = await _spotifyClientBuilder.BuildClient();
            Me = await spotify.UserProfile.Current();
        }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }
        public IList<SimpleAlbum> SearchResults { get; set; } = new List<SimpleAlbum>();
        public IList<Album> RatedAlbums { get; set; }

        public async Task OnGetAsync()
        {
            await SetMe();
            await LoadRatedAlbums();
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var spotify = await _spotifyClientBuilder.BuildClient();
                var searchRequest = new SearchRequest(SearchRequest.Types.Album, SearchQuery)
                {
                    Limit = 50
                };
                var searchResponse = await spotify.Search.Item(searchRequest);
               
                SearchResults = searchResponse.Albums.Items.Where(a => a.AlbumType == "album").ToList();

            }
        }
        public async Task<IActionResult> OnPostSaveAlbumAsync(string spotifyAlbumId, string name, string artist, string imageUrl, int rating)
        {
            await SetMe();

            using (var db = new ApplicationDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == Me.Id);
                if (user == null)
                {
                    user = new User { UserId = Me.Id };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }

                var existingAlbum = await db.Albums.FirstOrDefaultAsync(a => a.SpotifyAlbumId == spotifyAlbumId && a.UserId == user.UserId);
                if (existingAlbum != null)
                {
                    return RedirectToPage(); 
                }

                var album = new Album
                {
                    SpotifyAlbumId = spotifyAlbumId,
                    UserId = user.UserId,
                    Name = name,
                    Artist = artist,
                    ImageUrl = imageUrl,
                    Rating = rating
                };

                db.Albums.Add(album);
                await db.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        private async Task LoadRatedAlbums()
        {
            using (var db = new ApplicationDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == Me.Id);
                if (user != null)
                {
                    RatedAlbums = await db.Albums.Where(a => a.UserId == user.UserId).ToListAsync();
                }
                else
                {
                    RatedAlbums = new List<Album>();
                }
            }
        }
    }
}
