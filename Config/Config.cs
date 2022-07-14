using System;
using System.IO;

class Config
{
    String path;
    public Config(String path)
    {
        this.path = path;
        if (!File.Exists(path))
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            { 
                //first init
                sw.WriteLine("Config");
                sw.WriteLine("Donwloader");
                sw.WriteLine("path:");
                sw.WriteLine("speed:0");
                sw.WriteLine("default quality:0");
                sw.WriteLine("socket-timeout SECONDS:");
                sw.WriteLine("geo-bypass:0");
                sw.WriteLine("min-filesize SIZE:50k");
                sw.WriteLine("max-filesize SIZE:");
                sw.WriteLine("age-limit Years:");
                sw.WriteLine("limit:false");
                sw.WriteLine("limit-rate:50k");
                sw.WriteLine("retries:");
                sw.WriteLine("buffer-size:1024");
                sw.WriteLine("no-overwrites:true");
                sw.WriteLine(":");
                sw.WriteLine(":");
                sw.WriteLine(":");
                sw.WriteLine(":");
                sw.WriteLine(":");
                sw.WriteLine(":");
                sw.WriteLine(":");
                sw.WriteLine(":");
            }
        }
    }
    static void editconf(string vallue, string path, int line_to_edit)
    {
        string[] arrLine = File.ReadAllLines(path);
        arrLine[line_to_edit - 1] = arrLine[line_to_edit - 1].Split(":")[0] + ":" + vallue;
        File.WriteAllLines(path, arrLine); 
    }
    static String readconf(string path, int line_to_read)
    {
        string[] arrLine = File.ReadAllLines(path);
        return arrLine[line_to_read - 1];
    }



}