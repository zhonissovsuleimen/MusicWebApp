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


        private static string ComposeTitle(Faker f)
        {
            var p = f.Random.Int(0, 8);
            return p switch
            {
                0 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))}",
                1 => $"{Cap(f.PickRandom(Verbs))} the {Cap(f.PickRandom(Nouns))}",
                2 => $"{Cap(f.PickRandom(Nouns))} of {Cap(f.PickRandom(Places))}",
                3 => $"The {Cap(f.PickRandom(Nouns))}",
                4 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))} {f.Random.Number(1, 99)}",
                5 => $"{Cap(f.PickRandom(Nouns))} & {Cap(f.PickRandom(Nouns))}",
                6 => $"{Cap(f.PickRandom(Places))} Nights",
                7 => $"{f.Commerce.ProductAdjective()} {Cap(f.PickRandom(Nouns))}",
                _ => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))}",
            };
        }

        private static string ComposeArtist(Faker f)
        {
            var p = f.Random.Int(0, 6);
            return p switch
            {
                0 => $"{Cap(f.Name.FirstName())} {Cap(f.Name.LastName())}",
                1 => $"The {CapPlural(f.PickRandom(Nouns))}",
                2 => $"{Cap(f.Name.FirstName())} & The {Cap(f.PickRandom(BandSuffix))}",
                3 => $"DJ {Cap(f.Name.FirstName())}",
                4 => $"{Cap(f.Name.FirstName())} feat. {Cap(f.Name.FirstName())}",
                5 => $"{Cap(f.Name.FirstName())}",
                _ => $"{Cap(f.Name.FirstName())} {Cap(f.Name.LastName())}",
            };
        }

        private static string ComposeAlbum(Faker f)
        {
            if (f.Random.Bool(0.3f))
            {
                return "Single";
            }

            var p = f.Random.Int(0, 7);
            return p switch
            {
                0 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))}",
                1 => $"{Cap(f.PickRandom(Nouns))} ({f.Random.Number(1990, 2025)})",
                2 => $"{Cap(f.PickRandom(Places))} - Live",
                3 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))} {f.Random.Number(1, 9)}",
                4 => $"{Cap(f.PickRandom(Nouns))}: {Cap(f.PickRandom(AlbumModifiers))}",
                5 => $"{Cap(f.Name.FirstName())} {Cap(f.Name.LastName())} - {Cap(f.PickRandom(AlbumModifiers))}",
                6 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))} ({f.PickRandom(AlbumModifiers)})",
                _ => $"{Cap(f.PickRandom(Nouns))}",
            };
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
