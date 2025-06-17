namespace RecommenderApp_API.Entities
{
    public enum MovieStatus
    {
        Watched,
        Ignored,
        Favorite
    }
    public class UserMovie
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int MovieId { get; set; }
        public MovieStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Movie Movie { get; set; } = null!;
    }
}
