using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            throw new NotImplementedException();
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
