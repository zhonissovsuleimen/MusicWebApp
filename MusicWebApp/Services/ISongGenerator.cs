using Bogus;
using MusicWebApp.Models;

namespace MusicWebApp.Services
{
    public interface ISongGenerator
    {
        public List<SongPreview> Generate(Range range, ulong seed);
    }
}
