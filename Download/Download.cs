using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Project.Download
{

    public class Download
    {
        public static readonly Download Instance = new Download();

        public readonly string YTDLExecutablePath = "Download\\youtube-dl.exe";
        public readonly string DataDownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.music_db\\download\\";
        public string DataDownloadTempPath => DataDownloadPath + "temp\\";

        private readonly string[] InfoOptions =
        {
            "--skip-download", // damit kein video heruntergeladen wird
            "--print-json", // daten werden im json format ausgegeben
            "--no-progress", // kein herunterlade-fortschritt wird ausgegeben
            "--write-thumbnail", // thumbnail wird heruntergeladen
            "-o {OutputDir}", // da wo der kram hin soll
        };

        private readonly string[] DownloadOptions =
        {
            "--extract-audio", // damit nur das audio heruntergeladen wird
            "--audio-format mp3", // mp3 format bitte
            "--audio-quality {AudioQuality}", // audio Qualität. 0 bestes, 9 schlechtestes
            "--add-metadata", // metadaten schreiben
            "--embed-thumbnail", // thumbnail hinzufügen
            "--no-continue", // zuvor heruntergeladenes wird nicht weitergemacht
            "--newline", // damit der fortschritt begutachtet werden kann
        };


        public readonly ObservableCollection<MusicDownload> Queue = new ObservableCollection<MusicDownload>();
        public readonly ObservableCollection<MusicDownload> Done = new ObservableCollection<MusicDownload>();
        public bool IsBusy { get; private set; }


        private Download()
        {
            if(!Directory.Exists(DataDownloadPath))
            {
                Directory.CreateDirectory(DataDownloadPath);
            }
        }

        public MusicDownload? GetMusicDownload(string address)
        {
            var processInfo = new ProcessStartInfo(YTDLExecutablePath);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            string args = new StringBuilder()
                .Append($"\"{address}\" ")
                .AppendJoin(" ", InfoOptions)
                .ToString();
            args = args.Replace("{OutputDir}", $"\"{DataDownloadPath}\"");
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
            return ParseMusicDownloadFromJSON(address, json);
        }

        private MusicDownload ParseMusicDownloadFromJSON(string address, JsonNode root)
        {
            var obj = root.AsObject();
            return new MusicDownload()
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

        public void EnqueueDownload(MusicDownload download)
        {
            Queue.Add(download);
            download.DownloadState = DownloadState.Enqueued;
        }

        public void DirectDownloadDownload(MusicDownload download)
        {
            IsBusy = true;

            var fileName = $"{DataDownloadPath}{download.Title}.mp3";

            var processInfo = new ProcessStartInfo(YTDLExecutablePath);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            string args = new StringBuilder()
                .Append($"\"{download.Source}\" ")
                .AppendJoin(" ", DownloadOptions)
                .ToString();
            args = args.Replace("{OutputDir}", $"\"{fileName}\"");
            args = args.Replace("{AudioQuality}", $"{5}");
            processInfo.Arguments = args;

            download.DownloadState = DownloadState.Downloading;
            
            using var process = Process.Start(processInfo);

            process.OutputDataReceived += (s, e) => 
            {
                if(e.Data == null) return;

                var match = new Regex("\\[download\\]\\s*(\\d+\\.\\d+)%").Match(e.Data);
                if(match.Success)
                {
                    float progress = float.Parse(match.Groups[1].Value) / 1000f;
                    Console.WriteLine(progress);
                    download.Progress = progress;
                }
            };
            process.BeginOutputReadLine();

            var errorOutput = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if(errorOutput.Length > 0)
            {
                download.FileLocation = fileName;
                download.DownloadState = DownloadState.Success;
            }
            else
            {
                download.DownloadState = DownloadState.Error;
            }

            IsBusy = false;
        }
    }

    public interface IDownloadable
    {
        public string Source { get; }


    }

    public enum DownloadState
    {
        Waiting,
        Enqueued,
        Downloading,
        Success,
        Error,
    }

    public class MusicDownload : IDownloadable
    {
        public string Source { set; get; }
        public string? Thumbnail { set; get; }
        public string Title { set; get; }
        public string Description { set; get; }
        public string Artist { set; get; }
        public DateTime UploadDate { get; set; }
        public TimeSpan Duration { set; get; }

        public DownloadState DownloadState
        {
            get => downloadState;
            set
            {
                downloadState = value;
                DownloadStateChanged?.Invoke(value);
            }
        }
        private DownloadState downloadState = DownloadState.Waiting;

        public string? FileLocation { set; get; } = null;
        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                ProgressChanged?.Invoke(value);
            }
        }
        private float progress = 0;

        public event Action<float> ProgressChanged;
        public event Action<DownloadState> DownloadStateChanged;

    }

}
