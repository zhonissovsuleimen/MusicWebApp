using Bogus;
using MusicWebApp.Models;

namespace MusicWebApp.Services
{
    public class SongGeneratorEn : ISongGenerator
    {
        public List<SongPreview> Generate(Range range, ulong seed)
        {
            int count = range.End.Value - range.Start.Value;
            var items = new List<SongPreview>(count);
            
            //int intSeed = (int)((seed >> 32) ^ (seed & 0xFFFFFFFF));
            int intSeed = seed.GetHashCode();

            var songIndex = 0;
            var Faker = new Faker<SongPreview>()
                .StrictMode(true)
                .UseSeed(intSeed)
                .RuleFor(s => s.Index, f => songIndex++)
                .RuleFor(s => s.Title, f => f.Lorem.Word())
                .RuleFor(s => s.Artist, f => f.Lorem.Word())
                .RuleFor(s => s.Album, f => f.Lorem.Word())
                .RuleFor(s => s.Genre, f => f.Music.Genre());

            for (int i = 0; i < range.End.Value; i++)
            {
                var song = Faker.Generate();

                if (i >= range.Start.Value)
                {
                    items.Add(song);
                }
            }

            return items;
        }
    }
}
