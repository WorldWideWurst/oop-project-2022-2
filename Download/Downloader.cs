using System;
using System.Net;
using System.IO;
using System.Text;
using Project.Index;
using System;
using System.Diagnostics;
using System.ComponentModel;

// als Singelton umbauen
//Interface Implementieren
namespace Project.Download
{
    public class DownloadFile
    {
        public static bool Download(string src, string target)
        {
            //fileName: "Name"
            //fileType: "." + "fileType" 

            Process.Start("IExplore.exe", "www.northwindtraders.com");


            WebClient webclient = new WebClient();

            try
            {
                Console.WriteLine("Download wurde gestartet!");
                File.WriteAllBytes(target, webclient.DownloadData(src));
            }
            catch
            {
                Console.WriteLine("Es ist etwas beim Download schiefgelaufen!");
                return false;
            }

            Console.WriteLine("Die Datei: " + Path.GetFileName(src) + " wurde erfolgreich heruntergeladen!");
            return true;
        }

// Nullpointer einbauen!!!

        public static string[] ParseForDownloadables(string URL)
        {
            //looking for Pictures and downloads them

            WebClient webclient = new WebClient();

            string htmlstring = webclient.DownloadString(URL);
            char[] htmlchars = htmlstring.ToCharArray();

            string jpg = "";
            string temp = "";

            for (int i = 0; i < htmlchars.Length - 3; i++)
            {

                if (htmlchars[i] == 's')
                {

                    if (htmlchars[i + 1] == 'r' && htmlchars[i + 2] == 'c' && htmlchars[i + 3] == '=' && htmlchars[i + 4] == '"')
                    {

                        for (int j = i + 5; htmlchars[j] != '"'; j++)
                        {
                            jpg += htmlchars[j];
                        }
                    }

                    if (!temp.Contains(jpg) && (jpg.Contains(".jpg") || jpg.Contains(".png") || jpg.Contains(".svg")))
                    {
                        temp += ";" + jpg;
                    }
                    else
                    {
                        jpg = "####";
                    }
                    jpg = "";
                }
            }

            return temp.Split(";");
        }

    }

}