using Bogus;
using MusicWebApp.Models;

namespace MusicWebApp.Services
{
    public class SongGeneratorRu : ISongGenerator
    {
        public string Locale() => "ru";

        public List<SongPreview> Generate(Range range, ulong seed)
        {
            int count = range.End.Value - range.Start.Value;
            var items = new List<SongPreview>(count);

            // stable hash to int
            int intSeed = seed.GetHashCode();

            int songIndex = 0;
            var faker = new Faker<SongPreview>("ru")
                .StrictMode(true)
                .UseSeed(intSeed)
                .RuleFor(s => s.Index, f => songIndex++)
                .RuleFor(s => s.Title, f => ComposeTitle(f))
                .RuleFor(s => s.Artist, f => ComposeArtist(f))
                .RuleFor(s => s.Album, f => ComposeAlbum(f))
                .RuleFor(s => s.Genre, f => f.PickRandom(Genres));

            for (int i = 0; i < range.End.Value; i++)
            {
                var song = faker.Generate();
                if (i >= range.Start.Value)
                {
                    items.Add(song);
                }
            }

            return items;
        }

        // Neuter-gender adjective forms
        private static readonly string[] Adjectives =
        [
            "туманное","тихое","золотое","дикое","серебряное","осколочное","нежное","ночное","сладкое","горькое",
            "холодное","тёплое","медленное","быстрое","яркое","старое","новое","тайное","дальнее","беспечное",
            "лунное","синее","кровавое","желанное","хрупкое","свободное","горящее","робкое","скрытое"
        ];

        // Neuter-gender nouns only
        private static readonly string[] Nouns =
        [
            "сердце","небо","солнце","зеркало","стекло","пламя","письмо","эхо","поле","море",
            "окно","золото","время","пространство","дыхание","молчание","движение","видение","счастье","чувство"
        ];

        // Neuter singular active/participle-like adjectives
        private static readonly string[] Verbs =
        [
            "гоняющееся","находящее","теряющееся","ломающееся","держащееся","ожидающее","бегущее","падающее","восстающее","ищущее",
            "зовущее","плывущее","сражающееся","танцующее","поющее","вспоминающее","забывающее","жаждущее","поворачивающее","шагающее"
        ];

        private static readonly string[] Places =
        [
            "Москвы","Санкт-Петербурга","Казани","Минска","Киева","Тбилиси","Алматы","Владивостока","Екатеринбурга","Новосибирска",
            "Пристани","Вокзала","Садов","Башни","Моста","Клуба","Кафе","Подвала","Крыш","Студий"
        ];

        private static readonly string[] BandSuffix =
        [
            "Трио","Квартет","Коллектив","Бэнд","Оркестр","Ансамбль","Проект","Крю","Экспресс","Синдикат"
        ];

        private static readonly string[] AlbumModifiers =
        [
            "Делюкс","Акустика","Сессии","Живой","Ремиксы","EP","Том","Том.","Лучшее",
            "Антология","Переиздание","Переосмыслено","Демо","Коллекционное издание","Расширенное"
        ];

        // Custom Russian genre list
        private static readonly string[] Genres =
        [
            "Поп","Поп-рок","Рок","Русский рок","Альтернатива","Инди","Гранж","Пост-рок","Пост-панк","Прог-рок",
            "Хард-рок","Метал","Хэви-метал","Пауэр-метал","Дум-метал","Дэт-метал","Блэк-метал","Фолк-метал","Ню-метал",
            "Панк","Хардкор","Эмо","Ска","Джаз","Свинг","Блюз","Соул","Фанк","R&B",
            "Хип-хоп","Рэп","Фонк","Трип-хоп","Дрилл","Трэп",
            "Электронная","Дэнс","Хаус","Дип-хаус","Техно","Транс","Прогрессив","Дабстеп","Драм-н-бейс","Брейкбит",
            "Синти-поп","Синтвейв","Ло-фай","Чилаут","Эмбиент","Нью-эйдж","IDM",
            "Фолк","Этника","Кантри","Шансон","Авторская песня","Эстрада",
            "Латино","Регги","Ска-панк","К-поп","J-pop","Саундтрек","Классическая","Неоклассика","Диско","Ретро"
        ];

        private static string ComposeTitle(Faker f)
        {
            var p = f.Random.Int(0, 7);
            return p switch
            {
                0 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))}",
                1 => $"{Cap(f.PickRandom(Verbs))} {Cap(f.PickRandom(Nouns))}",
                2 => $"{Cap(f.PickRandom(Nouns))} из {Cap(f.PickRandom(Places))}",
                3 => $"{Cap(f.PickRandom(Nouns))}",
                4 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))} {f.Random.Number(1, 99)}",
                5 => $"{Cap(f.PickRandom(Nouns))} и {Cap(f.PickRandom(Nouns))}",
                6 => $"Ночи {Cap(f.PickRandom(Places))}",
                _ => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))}",
            };
        }

        private static string ComposeArtist(Faker f)
        {
            // Avoid using pluralized nouns as band names to keep grammar simple
            var p = f.Random.Int(0, 5);
            return p switch
            {
                0 => $"{Cap(f.Name.FirstName())} {Cap(f.Name.LastName())}",
                1 => $"{Cap(f.Name.FirstName())} и {Cap(f.PickRandom(BandSuffix))}",
                2 => $"DJ {Cap(f.Name.FirstName())}",
                3 => $"{Cap(f.Name.FirstName())} feat. {Cap(f.Name.FirstName())}",
                4 => $"{Cap(f.Name.FirstName())}",
                _ => $"{Cap(f.Name.FirstName())} {Cap(f.Name.LastName())}",
            };
        }

        private static string ComposeAlbum(Faker f)
        {
            if (f.Random.Bool(0.3f))
            {
                return "Сингл";
            }

            var p = f.Random.Int(0, 7);
            return p switch
            {
                0 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))}",
                1 => $"{Cap(f.PickRandom(Nouns))} ({f.Random.Number(1990, 2025)})",
                2 => $"{Cap(f.PickRandom(Places))} — Лайв",
                3 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))} {f.Random.Number(1, 9)}",
                4 => $"{Cap(f.PickRandom(Nouns))}: {Cap(f.PickRandom(AlbumModifiers))}",
                5 => $"{Cap(f.Name.FirstName())} {Cap(f.Name.LastName())} — {Cap(f.PickRandom(AlbumModifiers))}",
                6 => $"{Cap(f.PickRandom(Adjectives))} {Cap(f.PickRandom(Nouns))} ({f.PickRandom(AlbumModifiers)})",
                _ => $"{Cap(f.PickRandom(Nouns))}",
            };
        }

        private static string Cap(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim();
            return char.ToUpper(s[0]) + (s.Length > 1 ? s[1..] : "");
        }

        private static string ToNeuter(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            var trimmed = s.Trim();
            var lower = trimmed.ToLowerInvariant();

            static string ReplaceTail(string src, int tail, string repl)
                => src[..^tail] + repl;

            if (lower.EndsWith("ое") || lower.EndsWith("ее"))
                return trimmed; // already neuter

            if (lower.EndsWith("ый") || lower.EndsWith("ой"))
                return ReplaceTail(trimmed, 2, "ое");

            if (lower.EndsWith("ий"))
                return ReplaceTail(trimmed, 2, "ее");

            if (lower.EndsWith("ая"))
                return ReplaceTail(trimmed, 2, "ое");

            if (lower.EndsWith("яя"))
                return ReplaceTail(trimmed, 2, "ее");

            // fallback: just add neutral ending if looks like bare stem
            return trimmed + "ое";
        }
    }
}
