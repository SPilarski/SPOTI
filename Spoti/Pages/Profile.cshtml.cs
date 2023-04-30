using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
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
        public IList<Album> RatedAlbums { get; set; }
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 60;
        public int TotalAlbums { get; set; }

        public ProfileModel(SpotifyClientBuilder spotifyClientBuilder, ILogger<ProfileModel> logger)
        {
            _spotifyClientBuilder = spotifyClientBuilder;
            _logger = logger;
        }

        public PrivateUser Me { get; set; }
        public IList<FullAlbum> LastFiveAlbums { get; set; }

        public async Task<IActionResult> OnGet(int? page)
        {
            await SetMe();
            await LoadRatedAlbums();
            await LoadAlbums();
            return Page(); 
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


        private async Task SetMe()
        {
            var spotify = await _spotifyClientBuilder.BuildClient();
            Me = await spotify.UserProfile.Current();
        }

        private async Task LoadAlbums()
        {
            var spotify = await _spotifyClientBuilder.BuildClient();

            var recentlyPlayedRequest = new PlayerRecentlyPlayedRequest
            {
                Limit = 50
            };
            var recentlyPlayed = await spotify.Player.GetRecentlyPlayed(recentlyPlayedRequest);

           
            var albumIds = new HashSet<string>();
            LastFiveAlbums = new List<FullAlbum>();
            int currentIndex = 0;
            foreach (var item in recentlyPlayed.Items)
            {
                if (item.Track != null && item.Track.Album != null && item.Track.Album.AlbumType == "album" && albumIds.Add(item.Track.Album.Id))
                {
                    if (currentIndex < PageSize)
                    {
                        LastFiveAlbums.Add(await spotify.Albums.Get(item.Track.Album.Id));
                    }
                    currentIndex++;
                    if (LastFiveAlbums.Count >= PageSize)
                    {
                        break;
                    }
                }
            }
        }


    }
}