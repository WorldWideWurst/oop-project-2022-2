namespace Project.Download;

// verfasst von Louis R�disch
public class YTDLAPI {

    public string Path { get; set; }
    public int Quality { get; set; }

    public YTDLAPI (string path, int Quality){
        Path = path;
        this.Quality = Quality;
    }

}