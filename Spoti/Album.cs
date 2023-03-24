namespace Spoti
{
    public class Album
    {
        public int Id { get; set; }
        public string SpotifyAlbumId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string ImageUrl { get; set; }
        public double Rating { get; set; }
    }
}
