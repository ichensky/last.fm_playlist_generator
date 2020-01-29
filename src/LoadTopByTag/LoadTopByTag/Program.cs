using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTopByTag
{
    class Program
    {
        static async Task<List<string>> CrawleUrls(string tag) {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");

            var list = new List<string>();

            for (int i = 1; i < 21; i++)
            {
                Log($"processing last.fm page: {i}");
                var text = await client.GetStringAsync($"https://www.last.fm/tag/{tag}/tracks?page={i}");
                var lines= Regex.Split(text, "\r\n|\r|\n");

                var yout = lines.Where(x => x.Contains("data-youtube-url"))
                    .Select(x => x.Split('"')[1]);
                list.AddRange(yout);
            }
            return list;
        }

        public static void LoadMp3(List<string> urls) {

            int i = 0;
            foreach (var url in urls)
            {
                i++;
                Log($"Downloading file:{i} / {url}");
                var command = $"torify youtube-dl --download-archive ../downloaded.txt --no-post-overwrites --extract-audio --audio-format mp3 {url}";
                ExecuteCommand($"-c '{command}'");
            }
        }
        static async Task Main(string[] args)
        {
            File.WriteAllText("../log.txt", string.Empty);

            var tag = "classic+rock";
            var path = @$"E:\private\music\last.fm\tag\{tag}";
            bool crawleList = false;


            List<string> urls;
            var urlsPath = Path.Combine(path, "urls.txt");
            if (crawleList)
            {
                urls = await CrawleUrls(tag);
                File.WriteAllLines(urlsPath, urls);
            }
            else {
                urls = File.ReadLines(urlsPath).ToList();
            }

            var rdir = Path.Combine(path,"mp3");
            if (!Directory.Exists(rdir))
            {
                Directory.CreateDirectory(rdir);
            }

            Directory.SetCurrentDirectory(rdir);
            LoadMp3(urls);

            Console.WriteLine("Hello World!");
        }


        static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("bash.exe", command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput=true,
                RedirectStandardError=true
            };

            using var process = Process.Start(processInfo);
            process.OutputDataReceived += (s, e) => Log(e.Data);
            process.ErrorDataReceived += (s, e) => Log(e.Data);
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        private static void Log(string str) {
            Console.WriteLine(str);
            File.AppendAllText("../log.txt",str+Environment.NewLine);
        }
    }
}
