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

// Allein Verfasst von Ben Briesemeister
namespace Project.Convert
{
    public interface IConverter
    {
        /// <summary>
        /// Ob dieser Converter eine KOnvertierung des einen Formats (angegeben in mp3 z.B.) in das andere unterstützt.
        /// </summary>
        /// <returns></returns>
        bool SupportsConversion(string sourceExtension, string targetExtension);

        /// <summary>
        /// Führt die eigentlich Konvertierung durch.
        /// Es soll von un zu dem Format entsprechend der Dateiendungen der Pfade konvertiert werden.
        /// </summary>
        void Convert(string source, string target);
    }

    /// <summary>
    /// Eine implementierung des IConverter interfaces.
    /// Benutzt ffmpeg um die Konvertierung umzusetzen.
    /// </summary>
    public class Converter : IConverter
    {
       
        /// <summary>
        /// Singleton Instanz der Klasse.
        /// </summary>
        public static readonly Converter Instance = new();

        private Converter() { }

        /// <summary>
        /// Temporäre statische funktion, die ffmpeg zum konvertieren Nutzt.
        /// </summary>
        public void Convert(string source, string target)
        {
           
               
               
            
                //var mp3out = "";
               
                var ffmpegProcess = new Process();
            //Einstellungen 
                ffmpegProcess.StartInfo.UseShellExecute = false;
                ffmpegProcess.StartInfo.RedirectStandardInput = true;
                ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                ffmpegProcess.StartInfo.RedirectStandardError = true;
                ffmpegProcess.StartInfo.CreateNoWindow = true;
            //ffmpeg.exe Pfad
                ffmpegProcess.StartInfo.FileName = @"Convert\ffmpeg";
            //ffmpeq konvertierungs command
            ffmpegProcess.StartInfo.Arguments = " -i \"" + source + "\" -vn \"" + target + "\"";  //source & target (Datei im falschen Format/ konvertierte mp3 Datei)
                ffmpegProcess.Start();
            
            ffmpegProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                Console.WriteLine(e.Data);
            };
            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                Console.WriteLine(e.Data);
            };
            ffmpegProcess.BeginErrorReadLine();
            ffmpegProcess.WaitForExit();
            //ffmpeg schließen nach Prozessende
                if (!ffmpegProcess.HasExited)
                {
                    ffmpegProcess.Kill();
                }
                if(ffmpegProcess.ExitCode != 0)
            {
                throw new ConversionException();
            }
                //Console.WriteLine(mp3out);

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
