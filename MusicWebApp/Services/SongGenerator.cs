using Bogus;
using MusicWebApp.Models;

namespace MusicWebApp.Services
{
    public class SongGenerator
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
                .RuleFor(o => o.Index, f => songIndex++)
                .RuleFor(o => o.Title, f => f.Lorem.Word())
                .RuleFor(o => o.Artist, f => f.Lorem.Word());

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
