using System;
using System.IO;
using System.Globalization;
using Players;

class Program
{
    static SortedSet<Player> collection = new SortedSet<Player>(new PlayerIdComparer());
    static string dataFile = "";
    static DateTime Inittime;
    static void Main(string[] args)
    {
        try
        {
            Inittime = DateTime.Now;
            // Загружаем файл с данными
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
                dataFile = args[0];
            else
            {
                string env = Environment.GetEnvironmentVariable("DATA_FILE");
                if (!string.IsNullOrWhiteSpace(env))
                    dataFile = env;
            }
            if (string.IsNullOrWhiteSpace(dataFile))
            {
                Console.Write("Введите имя файла для загрузки/сохранения данных (players.csv) ");
                dataFile = Console.ReadLine();
            }
            if (File.Exists(dataFile))
            {
                LoadFromCsv(dataFile);
            }
            else
            {
                Console.WriteLine($"Файл {dataFile} не найден");
            }
            Interactive();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void Interactive()
    {
        Console.WriteLine("\nВведите команду (help - список команд)");
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine();
            if (input == null) continue;
            input = input.Trim();
            if (input == "") continue;
            // Возможность выполнять набор команд из файла
            try
            {
                if (input.StartsWith("execute_script"))
                {
                    string[] parts = SplitArgs(input);
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Укажите имя файла: execute_script file_name");
                        continue;
                    }
                    ExecuteScript(parts[1]);
                    continue;
                }
                var tokens = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                string cmd = tokens[0].ToLowerInvariant();
                string arg = tokens.Length > 1 ? tokens[1] : "";
                switch (cmd)
                {
                    case "help":
                        PrintHelp();
                        break;
                    case "info":
                        PrintInfo();
                        break;
                    case "show":
                        ShowAll();
                        break;
                    case "insert":
                        InsertInteractive();
                        break;
                    case "update":
                        UpdateInteractive(arg);
                        break;
                    case "remove_key":
                        RemoveById(arg);
                        break;
                    case "clear":
                        collection.Clear();
                        Console.WriteLine("Коллекция очищена");
                        break;
                    case "save":
                        SaveToCsv(dataFile);
                        Console.WriteLine("Коллекция сохранена");
                        break;
                    case "exit":
                        Console.WriteLine("Выход");
                        return;
                    case "print_ascending":
                        PrintAscending();
                        break;
                    case "print_descending":
                        PrintDescending();
                        break;
                    default:
                        Console.WriteLine("Неизвестная команда");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка выполнения команды " + ex.Message);
            }
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("Доступные команды:");
        Console.WriteLine("help — вывести справку");
        Console.WriteLine("info — информация о коллекции");
        Console.WriteLine("show — вывести все элементы");
        Console.WriteLine("insert — добавить новый элемент (интерактивно)");
        Console.WriteLine("update id — обновить элемент по id (интерактивно)");
        Console.WriteLine("remove_key id — удалить элемент по id");
        Console.WriteLine("clear — очистить коллекцию");
        Console.WriteLine("save — сохранить коллекцию в файл");
        Console.WriteLine("execute_script file_name — выполнить команды из файла");
        Console.WriteLine("print_ascending — вывести элементы по возрастанию (по PlayerID)");
        Console.WriteLine("print_descending — вывести элементы по убыванию");
        Console.WriteLine("exit — выйти");
    }

    static void PrintInfo()
    {
        Console.WriteLine($"Тип коллекции: SortedSet<Player>");
        Console.WriteLine($"Дата инициализации: {Inittime}");
        Console.WriteLine($"Количество элементов: {collection.Count}");
    }

    static void ShowAll()
    {
        if (collection.Count == 0)
        {
            Console.WriteLine("коллекция пуста");
            return;
        }
        foreach (var p in collection)
            Console.WriteLine(p.ToString());
    }

    static void InsertInteractive()
    {
        Player p = ReadPlayerFromConsole(interactive: true);
        if (collection.Any(pl => pl.PlayerID == p.PlayerID))
        {
            Console.WriteLine($"Игрок с ID {p.PlayerID} уже существует");
            return;
        }
        collection.Add(p);
        Console.WriteLine($"Добавлен игрок с ID {p.PlayerID}");
    }

    static void UpdateInteractive(string arg)
    {
        if (!int.TryParse(arg, out int id))
        {
            Console.WriteLine("Некорректный id");
            return;
        }
        var existing = collection.FirstOrDefault(pl => pl.PlayerID == id);
        if (existing == null)
        {
            Console.WriteLine($"Игрок с id {id} не найден");
            return;
        }
        Console.WriteLine("Введите новые значения для полей");
        Player newP = ReadPlayerFromConsole(interactive: true, allowSkip: true, template: existing);
        collection.Remove(existing);
        collection.Add(newP);
        Console.WriteLine($"Игрок {id} обновлён");
    }

    static void RemoveById(string arg)
    {
        if (!int.TryParse(arg, out int id))
        {
            Console.WriteLine("Некорректный id");
            return;
        }
        var existing = collection.FirstOrDefault(pl => pl.PlayerID == id);
        if (existing == null)
        {
            Console.WriteLine($"Игрок с id {id} не найден");
            return;
        }
        collection.Remove(existing);
        Console.WriteLine($"Игрок {id} удалён");
    }

    static void PrintAscending()
    {
        foreach (var p in collection)
            Console.WriteLine(p.ToString());
    }

    static void PrintDescending()
    {
        var list = collection.Reverse().ToList();
        foreach (var p in list)
            Console.WriteLine(p.ToString());
    }

    static void LoadFromCsv(string filename)
    {
        using (var sr = new StreamReader(filename))
        {
            string header = sr.ReadLine();
            if (header == null) return;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue; // Пропускаем пустые строки
                var parts = ParseCsvLine(line);
                try
                {
                    Player p = PlayerFromCsv(parts);
                    Player.EnsureNextId(p.PlayerID); // Обеспечиваем уникальность ID
                    collection.Add(p);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при парсинге строки CSV: {ex.Message}");
                }
            }
        }
    }

    static void SaveToCsv(string filename)
    {
        using (var sw = new StreamWriter(filename, false))
        {
            sw.WriteLine("PlayerID,Name,Team,Health,Money,Kills,Deaths,Status,Role,PosX,PosY," +
                "Country,Language,Experience,Accuracy,Level");
            foreach (var p in collection)
            {
                // Запись данных каждого игрока
                sw.WriteLine($"{p.PlayerID},{EscapeCsv(p.Name)},{EscapeCsv(p.Team)},{p.Health}," +
                    $"{p.Money},{p.Kills},{p.Deaths},{p.Status},{p.Role},{p.Position.X.ToString(CultureInfo.InvariantCulture)}," +
                    $"{p.Position.Y.ToString(CultureInfo.InvariantCulture)},{EscapeCsv(p.Country)},{EscapeCsv(p.Language)}," +
                    $"{p.Experience},{p.Accuracy.ToString(CultureInfo.InvariantCulture)},{p.Level}");
            }
        }
    }

    static Player PlayerFromCsv(string[] parts)
    {
        Player p = new Player();
        p.PlayerID = int.Parse(parts[0]);
        p.Name = parts[1];
        p.Team = parts[2];
        p.Health = int.Parse(parts[3]);
        p.Money = int.Parse(parts[4]);
        p.Kills = int.Parse(parts[5]);
        p.Deaths = int.Parse(parts[6]);
        p.Status = Enum.TryParse(parts[7], out PlayerStatus st) ? st : PlayerStatus.Idle;
        p.Role = Enum.TryParse(parts[8], out PlayerRole rl) ? rl : PlayerRole.Assault;
        p.Position = new PlayerPosition(float.Parse(parts[9], CultureInfo.InvariantCulture),
            float.Parse(parts[10], CultureInfo.InvariantCulture));
        p.Country = parts[11];
        p.Language = parts[12];
        p.Experience = int.Parse(parts[13]);
        p.Accuracy = float.Parse(parts[14], CultureInfo.InvariantCulture);
        p.Level = int.Parse(parts[15]);
        return p;
    }
    // Экранирование строк для CSV
    static string EscapeCsv(string s)
    {
        if (s == null) return "";
        if (s.Contains(",") || s.Contains("\""))
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        return s;
    }
    // Разбор строки CSV на части
    static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        string cur = "";
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    cur += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(cur);
                cur = "";
            }
            else
            {
                cur += c;
            }
        }
        result.Add(cur);
        return result.ToArray();
    }
    // Чтение Player из консоли
    static Player ReadPlayerFromConsole(bool interactive, bool allowSkip = false, Player template = null)
    {
        Player p = template != null ? ClonePlayer(template) : new Player();
        if (allowSkip && template != null)
        {
            // Сохраняем PlayerID
            p.PlayerID = template.PlayerID;
        }

        // В интерактивном режиме запрашиваем поля
        if (interactive)
        {
            string name = Prompt($"Name ({p.Name}): ", allowSkip, p.Name);
            if (!string.IsNullOrWhiteSpace(name)) p.Name = name;

            string team = Prompt($"Team ({p.Team}): ", allowSkip, p.Team);
            if (!string.IsNullOrWhiteSpace(team)) p.Team = team;

            p.Health = PromptInt($"Health ({p.Health}): ", allowSkip, p.Health, min: 0, max: 1000);

            p.Money = PromptInt($"Money ({p.Money}): ", allowSkip, p.Money, min: 0);

            p.Kills = PromptInt($"Kills ({p.Kills}): ", allowSkip, p.Kills, min: 0);

            p.Deaths = PromptInt($"Deaths ({p.Deaths}): ", allowSkip, p.Deaths, min: 0);

            Console.WriteLine("Status options: " + string.Join(", ", Enum.GetNames(typeof(PlayerStatus))));
            string statusStr = Prompt($"Status ({p.Status}): ", allowSkip, p.Status.ToString());
            if (!string.IsNullOrWhiteSpace(statusStr) && Enum.TryParse<PlayerStatus>(statusStr, out var st)) p.Status = st;

            Console.WriteLine("Role options: " + string.Join(", ", Enum.GetNames(typeof(PlayerRole))));
            string roleStr = Prompt($"Role ({p.Role}): ", allowSkip, p.Role.ToString());
            if (!string.IsNullOrWhiteSpace(roleStr) && Enum.TryParse<PlayerRole>(roleStr, out var rl)) p.Role = rl;

            p.Position = new PlayerPosition(
                PromptFloat($"PosX ({p.Position.X}): ", allowSkip, p.Position.X),
                PromptFloat($"PosY ({p.Position.Y}): ", allowSkip, p.Position.Y)
            );

            string country = Prompt($"Country ({p.Country}): ", allowSkip, p.Country);
            if (!string.IsNullOrWhiteSpace(country)) p.Country = country;

            string lang = Prompt($"Language ({p.Language}): ", allowSkip, p.Language);
            if (!string.IsNullOrWhiteSpace(lang)) p.Language = lang;

            p.Experience = PromptInt($"Experience ({p.Experience}): ", allowSkip, p.Experience, min: 0);

            p.Accuracy = PromptFloat($"Accuracy ({p.Accuracy}): ", allowSkip, p.Accuracy, min: 0, max: 100);

            p.Level = PromptInt($"Level ({p.Level}): ", allowSkip, p.Level, min: 1);
        }
        return p;
    }
    // Создание копии Player
    static Player ClonePlayer(Player src)
    {
        return new Player
        {
            PlayerID = src.PlayerID,
            Name = src.Name,
            Team = src.Team,
            Health = src.Health,
            Money = src.Money,
            Kills = src.Kills,
            Deaths = src.Deaths,
            Status = src.Status,
            Role = src.Role,
            Position = src.Position,
            Country = src.Country,
            Language = src.Language,
            Experience = src.Experience,
            Accuracy = src.Accuracy,
            Level = src.Level
        };
    }
    // Запрос строки
    static string Prompt(string promptText, bool allowSkip, string defaultValue)
    {
        Console.Write(promptText);
        string input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            if (allowSkip) return null;
            return defaultValue;
        }
        return input;
    }
    // Запрос целого числа
    static int PromptInt(string promptText, bool allowSkip, int defaultValue, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            Console.Write(promptText);
            string s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s))
            {
                if (allowSkip) return defaultValue;
                return defaultValue;
            }
            if (int.TryParse(s, out int v) && v >= min && v <= max) return v;
            Console.WriteLine("Некорректное целое значение");
        }
    }
    // Запрос числа с плавающей точкой
    static float PromptFloat(string promptText, bool allowSkip, float defaultValue, float min = float.MinValue, float max = float.MaxValue)
    {
        while (true)
        {
            Console.Write(promptText);
            string s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s))
            {
                if (allowSkip) return defaultValue;
                return defaultValue;
            }
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) && v >= min && v <= max) return v;
            Console.WriteLine("Некорректное число");
        }
    }
    // Выполнение строк из файла
    static void ExecuteScript(string filename)
    {
        var lines = File.ReadAllLines(filename);
        foreach (var raw in lines)
        {
            string line = raw.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue; // Пропускаем комментарии и пустые строки
            Console.WriteLine($"> {line}");
            var parts = SplitArgs(line);
            string command = parts[0].ToLowerInvariant();
            string arg = parts.Length > 1 ? parts[1] : "";
            try
            {
                switch (command)
                {
                    case "show": ShowAll(); break;
                    case "info": PrintInfo(); break;
                    case "clear": collection.Clear(); break;
                    case "save": SaveToCsv(dataFile); break;
                    case "remove_key":
                        if (int.TryParse(arg, out int rid)) { var ex = collection.FirstOrDefault(x => x.PlayerID == rid); if (ex != null) collection.Remove(ex); }
                        break;
                    case "insert":
                        var partsCsv = ParseCsvLine(arg);
                        Player p = PlayerFromCsv(partsCsv);
                        collection.Add(p);
                        Player.EnsureNextId(p.PlayerID);
                        break;
                    default:
                        Console.WriteLine($"Неизвестная команда в файле: {command}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при выполнении строки в файле: " + ex.Message);
            }
        }
        Console.WriteLine($"Сценарий {filename} выполнен");
    }
    // Разделитель команд и аргумента
    static string[] SplitArgs(string input)
    {
        var idx = input.IndexOf(' ');
        if (idx < 0) return new[] { input };
        var cmd = input.Substring(0, idx);
        var rest = input.Substring(idx + 1).Trim();
        return new[] { cmd, rest };
    }
}