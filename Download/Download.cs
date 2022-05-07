using System;

namespace Project.Download
{

    public interface IDownloader
    {

        string SourceName { get; }

        void Download(string source, string destination);

    }


    public class YoutubeDownloader : IDownloader
    {   

        public static readonly YoutubeDownloader Instance = new();

        public string SourceName { get => "YouTube"; }

        public void Download(string source, string destination)
        {
            throw new NotImplementedException();
        }
    }

}
