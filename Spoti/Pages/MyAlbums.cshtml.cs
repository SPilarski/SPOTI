using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spoti;
using SpotifyAPI.Web;

namespace Spoti.Pages
{
    public class MyAlbumsModel : PageModel
    {
        private readonly SpotifyClientBuilder _spotifyClientBuilder;

        public MyAlbumsModel(SpotifyClientBuilder spotifyClientBuilder)
        {
            _spotifyClientBuilder = spotifyClientBuilder;
            UserAlbums = new List<Album>();
        }

        public List<Album> UserAlbums { get; set; }
        public string SortOrder { get; set; }
        public async Task<IActionResult> OnGetAsync(string sortOrder)
        {
            SortOrder = sortOrder ?? "";

            await LoadUserAlbums();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAlbumAsync(int albumId)
        {
            using (var db = new ApplicationDbContext())
            {
                var album = await db.Albums.FindAsync(albumId);
                if (album != null)
                {
                    db.Albums.Remove(album);
                    await db.SaveChangesAsync();
                }
            }

            await LoadUserAlbums();
            return Page();
        }

        public async Task<IActionResult> OnPostEditAlbumRatingAsync(int albumId, int rating)
        {
            using (var db = new ApplicationDbContext())
            {
                var album = await db.Albums.FindAsync(albumId);
                if (album != null)
                {
                    album.Rating = rating;
                    db.Entry(album).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }

            await LoadUserAlbums();
            return RedirectToPage(); 
        }

        private async Task LoadUserAlbums()
        {
            var spotify = await _spotifyClientBuilder.BuildClient();
            var me = await spotify.UserProfile.Current();
            var userId = me.Id;

            using (var db = new ApplicationDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    user = new User { UserId = userId };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }

                var albumsQuery = db.Albums.Where(a => a.UserId == userId);

                if (SortOrder == "desc")
                {
                    albumsQuery = albumsQuery.OrderByDescending(a => a.Rating);
                }
                else
                {
                    albumsQuery = albumsQuery.OrderBy(a => a.Rating);
                }

                var albumsWithoutImageUrl = await albumsQuery.ToListAsync();

                foreach (var album in albumsWithoutImageUrl)
                {
                    var albumInfo = await spotify.Albums.Get(album.SpotifyAlbumId);
                    album.ImageUrl = albumInfo.Images.FirstOrDefault()?.Url;
                }

                UserAlbums = albumsWithoutImageUrl;
            }
        }



    }
}
