using MusicWebApp.Models;

namespace MusicWebApp.Services
{
    public class LikeGenerator
    {
        public List<Like> Generate(Range range, double input)
        {
            int count = range.End.Value - range.Start.Value;
            var likes = new List<Like>(count);

            //int intSeed = (int)((seed >> 32) ^ (seed & 0xFFFFFFFF));
            //int intSeed = seed.GetHashCode();
            int intSeed = 123; //hardcoded for independence

            var rnd = new Random(intSeed);

            double fraction = input - (int)input;
            int whole = (int)input;

            for (int i = 0; i < range.End.Value; i++)
            {
                var value = rnd.NextDouble() >= fraction ? whole : whole + 1;
                if (i >= range.Start.Value)
                {
                    likes.Add(new(i, value));
                }
            }

            return likes;
        }
    }
}
