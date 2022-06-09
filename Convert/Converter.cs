using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xabe.FFmpeg;
using System.Threading.Tasks;

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

        

        public void Convert(string source, string target)
        {
           /* static async Task RunConversion(Queue filesToConvert)
            {
                while (filesToConvert.TryDequeue(out FileInfo fileToConvert))
                {
                    //Save file to the same location with changed extension
                    string outputFileName = Path.ChangeExtension(fileToConvert.FullName, ".mp4");
                    await Conversion.ToMp4(fileToConvert.FullName, outputFileName).Start();
                    await Console.Out.WriteLineAsync($"Finished converion file [{fileToConvert.Name}]");
                }
            }*/

        }

        public bool SupportsConversion(string sourceExtension, string targetExtension)
        {
            throw new NotImplementedException();
        }
    }

    public class UnsupportedFileConversion : Exception
    {

    }

    public class ConversionException : Exception
    {

    }
}
