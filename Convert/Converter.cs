using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xabe.FFmpeg;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using System.Data;

namespace Project.Convert
{

    public interface IConverter
    {

        bool SupportsConversion(string sourceExtension, string targetExtension);

        void Convert(string source, string target);
    }

    public class Converter : IConverter
    {
       
        

        public static readonly Converter Instance = new();


        public void Convert(string[] args, string source, string target)
        {
            
                //var source = args[0] + ".mp4";
                //var target = args[1] + ".mp3";
               
            //Source Dateipfad
                var inputFile = @"C:\Videos\Allein.mp4"; 
            //Output Dateipfad
                var outputFile = @"C:\Videos\output.mp3";
                var mp3out = "";
               
                var ffmpegProcess = new Process();
            //Einstellungen 
                ffmpegProcess.StartInfo.UseShellExecute = false;
                ffmpegProcess.StartInfo.RedirectStandardInput = true;
                ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                ffmpegProcess.StartInfo.RedirectStandardError = true;
                ffmpegProcess.StartInfo.CreateNoWindow = true;
            //ffmpeg.exe Pfad
                ffmpegProcess.StartInfo.FileName = @"C:\ffmpeg.exe";
            //ffmpeq konvertierungs command
                ffmpegProcess.StartInfo.Arguments = " -i " + inputFile + " -vn -f mp3 -ab 320k " + outputFile;  //source & target
                ffmpegProcess.Start();
                ffmpegProcess.StandardOutput.ReadToEnd();
                mp3out = ffmpegProcess.StandardError.ReadToEnd();
                ffmpegProcess.WaitForExit();
            //ffmpeg schließen nach Prozessende
                if (!ffmpegProcess.HasExited)
                {
                    ffmpegProcess.Kill();
                }
               // Console.WriteLine(mp3out);

        }


        public void Convert(string source, string target)
        {
            throw new NotImplementedException();
        }

        public bool SupportsConversion(string sourceExtension, string targetExtension)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Wenn das zu mp3 zu konvertierende Format nicht unterstützt wird.
    /// </summary>
    public class UnsupportedFileConversion : Exception
    {

    }

    /// <summary>
    /// Wenn bei der Konvertierung irgendwas schief lief.
    /// </summary>
    public class ConversionException : Exception
    {

    }
}
