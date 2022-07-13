using System;
using System.Net;
using System.IO;
using System.Text;
using Project.Index;
using System;
using System.Diagnostics;
using System.ComponentModel;

// verfasst von Louis Rädisch

namespace Project.Download;
public class Youtubedl {

/*
URL: direkt link zum Video
conf: Settings
*/
 public static void Download(string URL, string conf)
        {
            var processInfo = new ProcessStartInfo("Download\\youtube-dl.exe", conf + URL); 
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }

 public static class BakeConfig
      {
        
        
        
        //return ;
      }






}