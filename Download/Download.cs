using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

// verfasst von Louis Rädisch

namespace Project.Download
{

    public class Download
    {
        public static readonly Download Instance = new Download();
        //start ytdl
        public readonly string YTDLExecutablePath = "Download\\youtube-dl.exe";
        public readonly string DataDownloadPath = Data.Database.DataPath;
        public string MusicDownloadPath => DataDownloadPath + "\\music";
        public string ThumbnailDownloadPath => DataDownloadPath + "\\thumbnails";

        private readonly string[] InfoOptions =
        {
            "--skip-download", // damit kein video heruntergeladen wird
            "--print-json", // daten werden im json format ausgegeben
            "--no-progress", // kein herunterlade-fortschritt wird ausgegeben
            "--write-thumbnail", // thumbnail wird heruntergeladen
            "-o {Target}", // da wo der kram (das thumbnail!) hin soll
            //"-v",  // verbosity
        };

        private readonly string[] DownloadOptions =
        {
            "-o {Target}", // wo das gute zeugs hin soll
            "--extract-audio", // damit nur das audio heruntergeladen wird
            "--audio-format mp3", // mp3 format bitte
            "--audio-quality {AudioQuality}", // audio Qualität. 0 bestes, 9 schlechtestes
            "--add-metadata", // metadaten schreiben
            "--embed-thumbnail", // thumbnail hinzufügen
            "--no-continue", // zuvor heruntergeladenes wird nicht weitergemacht
            "--newline", // damit der fortschritt begutachtet werden kann
            // "--limit-rate {DownloadSpeedLimit}", // downloadrate limitieren
            //"-v",  // verbosity
        };

        private readonly Regex progressRegex = new Regex("\\[download\\]\\s*(\\d+\\.\\d+)%");


        private Download()
        {
            Directory.CreateDirectory(MusicDownloadPath);
            Directory.CreateDirectory(ThumbnailDownloadPath);
        }

        public MusicDownloadInfo? GetMusicDownloadInfo(string address)
        {
            var thumbnailTempFile = Path.GetTempFileName();

            var processInfo = new ProcessStartInfo(YTDLExecutablePath);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            string args = new StringBuilder()
                .Append($"\"{address}\" ")
                .AppendJoin(" ", InfoOptions)
                .ToString();
            args = args.Replace("{Target}", $"\"{thumbnailTempFile}\"");
            processInfo.Arguments = args;

            using var process = Process.Start(processInfo);


            //process.ErrorDataReceived += (s, e) => { Console.WriteLine("e> " + e.Data); };
            //process.BeginErrorReadLine();
            //process.OutputDataReceived += (s, e) => { Console.WriteLine("o> " + e.Data); };
            //process.BeginOutputReadLine();



            var jsonString = process.StandardOutput.ReadToEnd();
            var errorOutput = process.StandardError.ReadToEnd();
            if(errorOutput.Length > 0)
            {
                return null;
            }

            process.WaitForExit();

            if(process.ExitCode != 0)
            {
                return null;
            }

            var json = JsonNode.Parse(jsonString);

            var info = ParseMusicDownloadFromJSON(address, json);

            return info;
        }

        private MusicDownloadInfo ParseMusicDownloadFromJSON(string address, JsonNode root)
        {
            var obj = root.AsObject();
            return new MusicDownloadInfo()
            {
                Source = address,
                UploadDate = UploadDateToDateTime((string)obj["upload_date"]),
                Thumbnail = (string)obj["thumbnail"],
                Title = (string)obj["title"],
                Description = (string)obj["description"],
                Artist = (string)obj["uploader"],
                Duration = TimeSpan.FromSeconds((int)obj["duration"]),
            };
        }

        private DateTime UploadDateToDateTime(string ud)
        {
            int year = int.Parse(ud[0..4]);
            int month = int.Parse(ud[4..6]);
            int day = int.Parse(ud[6..8]);
            return new DateTime(year, month, day);
        }


        public void DownloadMusic(string source, string target, DownloadSettings settings, IObserver<float> progressObserver)
        {
            var processInfo = new ProcessStartInfo(YTDLExecutablePath);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            int quality = settings.Quality switch
            {
                QualitySetting.Lowest => 0,
                QualitySetting.Default => 5,
                QualitySetting.Best => 9,
            };

            string args = new StringBuilder()
                .Append($"\"{source}\" ")
                .AppendJoin(" ", DownloadOptions)
                .ToString();
            args = args.Replace("{Target}", $"\"{target}\"");
            args = args.Replace("{AudioQuality}", $"{quality}");
            args = args.Replace("{DownloadSpeedLimit}", $"{settings.DownloadSpeedLimit / 1024}K");
            processInfo.Arguments = args;
            
            using var process = Process.Start(processInfo);

            process.OutputDataReceived += (s, e) => 
            {
                if(e.Data == null) return;
                Console.WriteLine(e.Data);

                var match = progressRegex.Match(e.Data);
                if(match.Success)
                {
                    float progress = float.Parse(match.Groups[1].Value) / 1000f;
                    progressObserver.OnNext(progress);
                }
            };
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                Console.WriteLine(e.Data);
            };
            process.BeginErrorReadLine();

            process.WaitForExit();

            if(process.ExitCode == 0)
            {
                progressObserver.OnCompleted();
            }
            else
            {
                progressObserver.OnError(new Exception("FÖHLER"));
            }
        }
    }

    public enum QualitySetting
    {
        Lowest,
        Default,
        Best,
    }

    public class DownloadSettings
    {
        public QualitySetting Quality { get; set; }
        public int DownloadSpeedLimit { get; set; }
    }

    public class MusicDownloadInfo
    {
        public string Source { set; get; }
        public string? Thumbnail { set; get; }
        public string Title { set; get; }
        public string Description { set; get; }
        public string Artist { set; get; }
        public DateTime UploadDate { get; set; }
        public TimeSpan Duration { set; get; }
    }

}
