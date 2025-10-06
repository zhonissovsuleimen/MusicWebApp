using Bogus;

namespace MusicWebApp.Models
{
    public record SongPreview(
        int? Index,
        string? Title,
        string? Artist
    )
    {
        public SongPreview() : this(null, null, null) { }
    }
}
