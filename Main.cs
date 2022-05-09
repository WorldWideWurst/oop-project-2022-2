using System;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Project.Index;


namespace Project
{
    public class DownloadFile
    {

        public static bool Download(string URL, string fileName)
        {
            //fileName: "Name"
            //fileType: "." + "fileType"

            WebClient webclient = new WebClient();
            var split = URL.Split(".");
            string fileType = "." + split[split.Length - 1];

            try
            {
                Console.WriteLine("Download wurde gestartet!");
                File.WriteAllBytes("temp\\" + fileName + fileType, webclient.DownloadData(URL));

            }

            catch
            {
                Console.WriteLine("Es ist etwas beim Download schiefgelaufen!");
                return false;
            }

            Console.WriteLine("Die Datei: " + fileName + fileType + " wurde erfolgreich heruntergeladen!");
            return true;
        }


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
                    if (jpg != "####")
                    {
                        Download(URL + jpg, "temp" + new Random().Next());
                    }
                    jpg = "";
                }
            }

            File.WriteAllText("temp\\html.html", htmlstring);

            return temp.Split(";");
        }

    }

}