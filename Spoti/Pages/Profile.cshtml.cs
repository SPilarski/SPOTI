using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> OnGet()
        {
            await SetMe();
            await LoadLastFiveAlbums();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAlbumAsync(string spotifyAlbumId, string name, string artist, string imageUrl)
        {
            await SetMe(); // Add this line to set the `Me` property

            using (var db = new ApplicationDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == Me.Id);
                if (user == null)
                {
                    user = new User { UserId = Me.Id };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }

                var random = new Random();
                var rating = random.NextDouble() * 5; // Random rating between 0 and 5

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

        public async Task<IActionResult> OnPost()
        {
            await HttpContext.SignOutAsync();
            return Redirect("https://google.com");
        }

        private async Task SetMe()
        {
            var spotify = await _spotifyClientBuilder.BuildClient();
            Me = await spotify.UserProfile.Current();
        }

        private async Task LoadLastFiveAlbums()
        {
            var spotify = await _spotifyClientBuilder.BuildClient();

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
                    if (LastFiveAlbums.Count >= 20)
                    {
                        break;
                    }
                }
            }
        }
    }
}
