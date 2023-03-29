using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Spoti.Pages
{
    public class RecommendationsModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;

        public bool NotEnoughListenedTracks { get; set; }
        private readonly SpotifyClientBuilder _spotifyClientBuilder;
        private readonly ILogger<RecommendationsModel> _logger;

        public RecommendationsModel(SpotifyClientBuilder spotifyClientBuilder, ILogger<RecommendationsModel> logger)
        {
            _spotifyClientBuilder = spotifyClientBuilder;
            _logger = logger;
        }

        public IList<FullTrack> RecommendedTracks { get; set; }
        public IList<SimpleAlbum> RecommendedAlbums { get; set; }

        public async Task OnGet(string recommendationType = "recent")
        {
            await GetRecommendations(recommendationType);
        }

        public async Task<IActionResult> OnGetCreatePlaylist(string recommendationType = "recent")
        {
            await GetRecommendations(recommendationType);

            var spotify = await _spotifyClientBuilder.BuildClient();
            var user = await spotify.UserProfile.Current();
            var playlistName = "Rekomendacje";

         
            var playlist = await spotify.Playlists.Create(user.Id, new PlaylistCreateRequest(playlistName));

     
            List<string> topRecommendedTracks;
            if (!NotEnoughListenedTracks)
            {
                topRecommendedTracks = RecommendedTracks.Take(15).Select(t => t.Uri).ToList();
            }
            else
            {
                topRecommendedTracks = new List<string>();
                int trackCount = 0;
                foreach (var album in RecommendedAlbums)
                {
                    var albumTracks = await spotify.Albums.GetTracks(album.Id);
                    var albumTrackUris = albumTracks.Items.Select(t => t.Uri).ToList();
                    topRecommendedTracks.AddRange(albumTrackUris);
                    trackCount += albumTrackUris.Count;

                    if (trackCount >= 15)
                    {
                        topRecommendedTracks = topRecommendedTracks.Take(15).ToList();
                        break;
                    }
                }
            }

          
            await spotify.Playlists.AddItems(playlist.Id, new PlaylistAddItemsRequest(topRecommendedTracks));

            return RedirectToPage("./Recommendations");
        }




        private async Task GetRecommendations(string recommendationType = "recent")
        {
            var spotify = await _spotifyClientBuilder.BuildClient();

            List<string> seedTrackIds;

            if (recommendationType == "top")
            {
                var topTracks = await spotify.Personalization.GetTopTracks();
                seedTrackIds = topTracks.Items.Select(track => track.Id).Take(2).ToList();
            }

            else
            {
                var recentlyPlayedTracks = await spotify.Player.GetRecentlyPlayed();
                seedTrackIds = recentlyPlayedTracks.Items.Select(item => item.Track.Id).Take(2).ToList();
            }

            if (seedTrackIds.Count == 0)
            {
                NotEnoughListenedTracks = true;

              
                var popularAlbums = await spotify.Browse.GetNewReleases(new NewReleasesRequest { Limit = 20 });
                RecommendedAlbums = popularAlbums.Albums.Items;

                return;
            }

            var recommendationsRequest = new RecommendationsRequest
            {
                Limit = 20
            };
            foreach (var seedTrackId in seedTrackIds)
            {
                recommendationsRequest.SeedTracks.Add(seedTrackId);
            }

            var recommendations = await spotify.Browse.GetRecommendations(recommendationsRequest);

            var trackIds = recommendations.Tracks.Select(t => t.Id).ToList();

            var tracksResponse = await spotify.Tracks.GetSeveral(new TracksRequest(trackIds));
            RecommendedTracks = tracksResponse.Tracks;

            RecommendedAlbums = tracksResponse.Tracks
                .Select(t => t.Album)
                .Where(a => a.TotalTracks > 1)
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .ToList();
        }
    }
}
