using DSharpPlus.Entities;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;

namespace BinLiteServer
{
    public static class Logger
    {
        private static string currentFile;
        private static readonly (string, ConsoleColor, DiscordColor)[] levels = new[]
        {
            ("TRACE", ConsoleColor.Gray, DiscordColor.Gray),
            ("DEBUG", ConsoleColor.Magenta, DiscordColor.Magenta),
            ("INFO", ConsoleColor.White, DiscordColor.White),
            ("WARN", ConsoleColor.Yellow, DiscordColor.Yellow),
            ("ERROR", ConsoleColor.Red, DiscordColor.Red),
            ("FATAL", ConsoleColor.DarkRed, DiscordColor.DarkRed),
        };
        //private static string DiscordToken;

        private static int consoleLevel;
        private static int fileLevel;
        //private static int discordLevel;

        public static DiscordChannel DiscordLogChannel { get; set; }

        public static bool Ready { get; private set; }

        public static ConcurrentQueue<(string type, object content, string file, int line, DateTime time)> LogQueue { get; private set; }
        public static bool IsLogging { get; private set; }

        public static void Init()
        {
            consoleLevel = levels.Select((l, index) => (l, index))
                .First(c => c.l.Item1 == Configuration.Get<string>("logLevel.console")).index;
            fileLevel = levels.Select((l, index) => (l, index))
                .First(c => c.l.Item1 == Configuration.Get<string>("logLevel.file")).index;
            //discordLevel = levels.Select((l, index) => (l, index))
            //    .First(c => c.l.Item1 == Configuration.Get<string>("logLevel.discord")).index;

            //DiscordToken = Configuration.Get<string>("discord.token");
            var baseLogs = Configuration.Get<string>("logPath");

            if (!Directory.Exists(baseLogs))
            {
                Directory.CreateDirectory(baseLogs);
            }

            var count = 0;
            foreach (var file in Directory.GetFiles(baseLogs))
            {
                if (Path.GetExtension(file) == ".zip") { continue; }
                var newFile = Path.GetFileName(file) + ".zip";
                var tempFolder = Directory.CreateDirectory(Path.Combine(baseLogs, "temp-" + Path.GetFileNameWithoutExtension(file)));
                File.Move(file, Path.Combine(tempFolder.FullName, Path.GetFileName(file)));
                ZipFile.CreateFromDirectory(tempFolder.FullName, Path.Combine(baseLogs, newFile));
                Directory.Delete(tempFolder.FullName, true);
                count++;
            }

            currentFile = Path.Combine($"{baseLogs}", $"{Snowflake.Generate()}.log");
            File.Create(currentFile).Dispose();

            LogQueue = new();
            new Thread(LogThread).Start();

            Info($"Logger online. Zipped {count} old logs.");
            Ready = true;
        }

        private static void LogThread()
        {
            while (true)
            {
                while (LogQueue.IsEmpty) { Thread.Sleep(1); }
                if (LogQueue.TryDequeue(out var r))
                {
                    IsLogging = true;
                    var (type, content, file, line, time) = r;

                    if (content.ToString()!.Contains("[DSharpPlus]") && type == "TRACE")
                    {
                        continue;
                    }

                    var level = levels.Select((l, index) => (l, index)).First(c => c.l.Item1 == type).index;

                    if (level >= consoleLevel)
                    {
                        Log_Console(type, content, file, line, time);
                    }

                    if (level >= fileLevel)
                    {
                        Log_File(type, content, file, line, time);
                    }

                    //if (level >= discordLevel)
                    //{
                    //    Log_Discord(type, content, file, line, time);
                    //}
                    IsLogging = false;
                }
            }
        }

        public static void Log(string type, object content)
        {
            var frame = new StackFrame(2, true);
            var file = frame.GetFileName()!;
            file = file is null ? "" : file.Contains('\\') ? file[(file.LastIndexOf(@"\") + 1)..] : file;
            var line = frame.GetFileLineNumber();

            LogQueue.Enqueue((type, content, file, line, DateTime.Now));
        }

        private static void Log_Console(string type, object content, string file, int line, DateTime time)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(time.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.ForegroundColor = levels.First(l => l.Item1 == type).Item2;
            Console.Write($" {type,-5} ");
            Console.Write($"{file + ":" + line,-30} - {content}\n\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Log_File(string type, object content, string file, int line, DateTime time)
        {
            var message = $"{time:yyyy-MM-dd HH:mm:ss} {type} {file}:{line} - {content}\n";
            File.AppendAllText(currentFile, message);
        }

        //private static void Log_Discord(string type, object content, string file, int line, DateTime time)
        //{
            //if (DiscordManager.Client is null || DiscordLogChannel is null) { return; }

            //var con = content.ToString()!.Replace(DiscordToken, "TOILETBOWL??");

            //var color = levels.First(l => l.Item1 == type).Item3;

            //DiscordLogChannel.SendMessageAsync(new DiscordEmbedBuilder()
            //    .WithColor(color)
            //    .WithTitle(type)
            //    .WithDescription(con)
            //    .AddField("File", file + ":" + line)
            //    .WithTimestamp(time)
            //    .Build());
        //}

        public static void Trace(object content) => Log(levels[0].Item1, content);
        public static void Debug(object content) => Log(levels[1].Item1, content);
        public static void Info(object content) => Log(levels[2].Item1, content);
        public static void Warn(object content) => Log(levels[3].Item1, content);
        public static void Error(object content) => Log(levels[4].Item1, content);
        public static void Fatal(object content) => Log(levels[5].Item1, content);
    }
}
