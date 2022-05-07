using System;

namespace Project.Download
{

    public interface IDownloader
    {

        string SourceDomain { get; }

        void Download(string source, string destination);

    }

    


    public class YoutubeDownloader : IDownloader
    {   

        public static readonly YoutubeDownloader Instance = new();

        public string SourceDomain { get => "YouTube"; }

        public void Download(string source, string destination)
        {
            throw new NotImplementedException();
        }
    }

}
