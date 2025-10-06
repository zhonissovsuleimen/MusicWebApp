namespace MusicWebApp.Models
{
    public record Like(
        int? Index,
        int? Value
    )
    {
        public Like() : this(null, null) { }
    }
}
