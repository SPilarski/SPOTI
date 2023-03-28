using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Spoti.Pages
{
    public class RecommendationsModel : PageModel
    {
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

        public async Task OnGet()
        {
            var spotify = await _spotifyClientBuilder.BuildClient();

            var topTracks = await spotify.Personalization.GetTopTracks();

            if (topTracks.Items.Count == 0)
            {
                NotEnoughListenedTracks = true;

                // Pobierz popularne albumy (przyk³ad z 20 popularnych albumów)
                var popularAlbums = await spotify.Browse.GetNewReleases(new NewReleasesRequest { Limit = 20 });
                RecommendedAlbums = popularAlbums.Albums.Items;

                return;
            }

            var seedTrackIds = topTracks.Items.Select(track => track.Id).Take(2).ToList();

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
