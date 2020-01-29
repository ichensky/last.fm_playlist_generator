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
        private static string _errorLogPath;

        static async Task<List<string>> CrawleUrls(string tag) {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");

            var list = new List<string>();

            for (int i = 1; i < 21; i++)
            {
                Console.WriteLine($"processing last.fm page: {i}");
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
                Console.WriteLine($"Downloading file:{i}");
                var command = $"torify youtube-dl --download-archive ../downloaded.txt --no-post-overwrites --extract-audio --audio-format mp3 {url}";
                ExecuteCommand($"bash.exe -c \"{command}\"");
                Thread.Sleep(5);
            }
        }
        static async Task Main(string[] args)
        {
            var tag = "classic+rock";
            var path = @$"C:\Users\IChensky\Desktop\last.fm\tag\{tag}";
            bool crawleList = false;


            _errorLogPath = Path.Combine(path, "log.error.txt");
            File.WriteAllText(_errorLogPath,"");

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
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            if (!string.IsNullOrEmpty(error))
            {
                File.AppendAllText(_errorLogPath,error + Environment.NewLine);
            }
            process.Close();
        }

    }
}
