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
        private readonly SpotifyClientBuilder _spotifyClientBuilder;
        private readonly ILogger<RecommendationsModel> _logger;

        public RecommendationsModel(SpotifyClientBuilder spotifyClientBuilder, ILogger<RecommendationsModel> logger)
        {
            _spotifyClientBuilder = spotifyClientBuilder;
            _logger = logger;
        }

        public IList<FullTrack> RecommendedTracks { get; set; }

        public async Task OnGet()
        {
            var spotify = await _spotifyClientBuilder.BuildClient();

            // Fetch user's top tracks as seed data
            var topTracks = await spotify.Personalization.GetTopTracks();
            _logger.LogInformation("Top Tracks: {TopTracks}", topTracks.Items.Select(t => t.Name));

            // Use the seed data to generate recommendations
            var seedTrackIds = topTracks.Items.Select(track => track.Id).Take(5).ToList();
            _logger.LogInformation("Seed Track IDs: {SeedTrackIds}", seedTrackIds);

            var recommendationsRequest = new RecommendationsRequest
            {
                Limit = 20
            };
            foreach (var seedTrackId in seedTrackIds)
            {
                recommendationsRequest.SeedTracks.Add(seedTrackId);
            }

            var recommendations = await spotify.Browse.GetRecommendations(recommendationsRequest);
            _logger.LogInformation("Recommendations: {Recommendations}", recommendations.Tracks.Select(t => t.Name));

            var trackIds = recommendations.Tracks.Select(t => t.Id).ToList();
            _logger.LogInformation("Recommended Track IDs: {TrackIds}", trackIds);

            var tracksResponse = await spotify.Tracks.GetSeveral(new TracksRequest(trackIds));
            RecommendedTracks = tracksResponse.Tracks;
        }
    }
}
