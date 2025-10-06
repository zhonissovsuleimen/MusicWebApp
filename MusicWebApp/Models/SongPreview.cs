using Bogus;

namespace MusicWebApp.Models
{
    public record SongPreview(
        int? Index,
        string? Title,
        string? Artist,
        string? Album,
        string? Genre
    )
    {
        public SongPreview() : this(null, null, null, null, null) { }
    }
}
