namespace Project.Download;
public class YTDLPAI {

    string path; 
    int Quality;

    public static YTDLAPI (string path, int Quality){
        this.path = path;
        this.Quality = Quality;

    }

    void setPath (string path){
        this.path = path;
    }

     string getPath(){
        return this.path;
    }
    
    void setQuality (int Quality){
        this.Quality = Quality;
    }

    int getQuality(){
        return this.Quality;
    }


}