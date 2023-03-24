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
        }

        public List<Album> UserAlbums { get; set; }

        public async Task<IActionResult> OnGetAsync()
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

                UserAlbums = await db.Albums
                    .Where(a => a.UserId == userId)
                    .ToListAsync();
            }

            return Page();
        }
    }
}
