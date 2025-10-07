using Bogus;
using MusicWebApp.Models;

namespace MusicWebApp.Services
{
    public class SongGeneratorEn : ISongGenerator
    {
        public string Locale() => "en";
        public List<SongPreview> Generate(Range range, ulong seed)
        {
            int count = range.End.Value - range.Start.Value;
            var items = new List<SongPreview>(count);

            //int intSeed = (int)((seed >> 32) ^ (seed & 0xFFFFFFFF));
            int intSeed = seed.GetHashCode();

            int songIndex = 0;
            var Faker = new Faker<SongPreview>()
                .StrictMode(true)
                .UseSeed(intSeed)
                .RuleFor(s => s.Index, f => songIndex++)
                .RuleFor(s => s.Title, f => ComposeTitle(f))
                .RuleFor(s => s.Artist, f => ComposeArtist(f))
                .RuleFor(s => s.Album, f => ComposeAlbum(f))
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

        private static readonly string[] Adjectives =
        [
            "lonely","broken","golden","silent","wild","faded","heavy","quiet","blazing","gentle",
            "midnight","crimson","shallow","endless","hollow","brave","secret","lonely","urban","ancient",
            "electric","blue","cold","warm","sweet","bitter","roaring","flying","neon","distant",
            "soft","lucky","empty","burning","steady","frozen","free","slow","rapid","bright"
        ];

        private static readonly string[] Nouns =
        [
            "heart","road","dream","night","city","river","sky","fire","light","shadow",
            "memory","ocean","wind","stone","voice","eyes","home","train","star","garden",
            "machine","mirror","signal","paper","glass","garden","highway","island","hill","sun",
            "moon","rain","wave","door","song","bottle","key","bridge","garden","frame",
            "letter","street","child","ghost","flame","ticket","clock","pulse","secret","garden"
        ];

        private static readonly string[] Verbs =
        [
            "chasing","finding","losing","breaking","holding","waiting","running","falling","rising","searching",
            "calling","swimming","fighting","dancing","singing","remembering","forgetting","craving","turning","walking"
        ];

        private static readonly string[] Places =
        [
            "London","Paris","Tokyo","New York","Berlin","Sydney","Venice","Rome","Nashville","Memphis",
            "Harbor","Station","Garden","Tower","Bridge","Club","Cafe","Basement","Rooftop","Studio"
        ];

        private static readonly string[] BandSuffix =
        [
            "Trio","Quartet","Collective","Band","Orchestra","Ensemble","Project","Crew","Express","Syndicate"
        ];

        private static readonly string[] AlbumModifiers =
        [
            "Deluxe","Acoustic","Sessions","Live","Remixes","EP","Volume", "Vol.","Greatest Hits","B-sides",
            "Anthology","Revisited","Reimagined","Unplugged","Demo","Collector's Edition","Expanded"
        ];


        // Helper: create many believable title patterns
        private static string ComposeTitle(Faker f)
        {
            var p = f.Random.Int(0, 8);
            switch (p)
            {
                case 0:
                    return $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))}";
                case 1:
                    return $"{Cap(f.PickRandom(Verbs))} the {Cap(f.PickRandom(Nouns))}";
                case 2:
                    return $"{Cap(f.PickRandom(Nouns))} of {Cap(f.PickRandom(Places))}";
                case 3:
                    return $"The {Cap(f.PickRandom(Nouns))}";
                case 4:
                    return $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))} ({f.Random.Number(1,99)})";
                case 5:
                    return $"{Cap(f.PickRandom(Nouns))} & {Cap(f.PickRandom(Nouns))}";
                case 6:
                    return $"{Cap(f.PickRandom(Places))} Nights";
                case 7:
                    return $"{f.Commerce.ProductAdjective()} {Cap(f.PickRandom(Nouns))}";
                default:
                    return $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))}";
            }
        }

        private static string ComposeArtist(Faker f)
        {
            var p = f.Random.Int(0, 6);
            switch (p)
            {
                case 0:
                    return $"{Cap(f.Name.FirstName())} {Cap(f.Name.LastName())}";
                case 1:
                    return $"The {CapPlural(f.PickRandom(Nouns))}";
                case 2:
                    return $"{Cap(f.Name.FirstName())} & The {Cap(f.PickRandom(BandSuffix))}";
                case 3:
                    return $"DJ {Cap(f.Name.FirstName())}";
                case 4:
                    return $"{Cap(f.Name.FirstName())} feat. {Cap(f.Name.FirstName())}";
                case 5:
                    return $"{Cap(f.Name.FirstName())}";
                default:
                    return $"{Cap(f.Name.FirstName())} {Cap(f.Name.LastName())}";
            }
        }

        private static string ComposeAlbum(Faker f)
        {
            if (f.Random.Bool(0.3f))
            {
                return "Single";
            }

            var p = f.Random.Int(0, 7);
            switch (p)
            {
                case 0:
                    return $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))}";
                case 1:
                    return $"{Cap(f.PickRandom(Nouns))} ({f.Random.Number(1990, 2025)})";
                case 2:
                    return $"{Cap(f.PickRandom(Places))} — Live";
                case 3:
                    return $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))} {f.Random.Number(1,9)}";
                case 4:
                    return $"{Cap(f.PickRandom(Nouns))}: {Cap(f.PickRandom(AlbumModifiers))}";
                case 5:
                    return $"{Cap(f.Name.FirstName())} {Cap(f.Name.LastName())} — {Cap(f.PickRandom(AlbumModifiers))}";
                case 6:
                    return $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))} ({f.PickRandom(AlbumModifiers)})";
                default:
                    return $"{Cap(f.PickRandom(Nouns))}";
            }
        }

        private static string Cap(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim();
            return char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s[1..] : "");
        }

        private static string CapPlural(string s)
        {
            var baseWord = Cap(s);
            if (baseWord.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                || baseWord.EndsWith("x", StringComparison.OrdinalIgnoreCase)
                || baseWord.EndsWith("z", StringComparison.OrdinalIgnoreCase)
                || baseWord.EndsWith("ch", StringComparison.OrdinalIgnoreCase)
                || baseWord.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
                return baseWord + "es";

            if (baseWord.EndsWith("y", StringComparison.OrdinalIgnoreCase) && baseWord.Length > 1 &&
                !"aeiou".Contains(char.ToLowerInvariant(baseWord[baseWord.Length - 2])))
                return string.Concat(baseWord.AsSpan(0, baseWord.Length - 1), "ies");

            return baseWord + "s";
        }
    }
}
