using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace System.IO
{
    public class StreamDecoder : IDisposable
    {

        public enum Endianness
        {
            Little,
            Big,
            Network = Big,
            Native = Little,
        }

        private Stream stream;
        public Endianness Endian { get; set; }

        public StreamDecoder(Stream stream, Endianness endian = Endianness.Native)
        {
            this.stream = stream;
            this.Endian = endian;
        }

        public long Length => stream.Length;

        public byte U8()
        {
            var b = stream.ReadByte(); 
            if (b < 0)
            {
                throw new EndOfStreamException();
            }
            else
            {
                return (byte)(b & 0xFF);
            }
        }

        public ushort U16BE() => (ushort)((U8() << 8) | U8());
        public ushort U16LE() => (ushort)(U8() | (U8() << 8));
        public ushort U16() => Endian == Endianness.Little ? U16LE() : U16BE();

        public uint U32BE() => (uint)((U8() << 24) | (U8() << 16) | (U8() << 8) | U8());
        public uint U32LE() => (uint)(U8() | (U8() << 8) | (U8() << 16) | (U8() << 24));
        public uint U32() => Endian == Endianness.Little ? U32LE() : U32BE();


        public byte[] Bytes(int n)
        {
            if(Length < n)
            {
                throw new EndOfStreamException();
            }

            byte[] bytes = new byte[n];
            stream.Read(bytes, 0, n);
            return bytes;
        }
        public int Bytes(byte[] buf, int offset, int count) => stream.Read(buf, offset, count);

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}

namespace Project.Data
{
    public interface IMusicLoader
    {
        bool SupportsExtension(string extension);

        Music Load(string path);
    }

    public class MusicLoader : IMusicLoader
    {

        public static readonly MusicLoader Instance = new MusicLoader();

        static MusicLoader()
        {
            Instance.Loaders.Add(MP3Loader.Instance);
        }

        public IList<IMusicLoader> Loaders { get; } = new List<IMusicLoader>();

        public bool SupportsExtension(string extension)
        {
            return Loaders.Any(loader => loader.SupportsExtension(extension));
        }

        public Music Load(string path)
        {
            var extension = Path.GetExtension(path).Substring(1);
            foreach(var loader in Loaders)
            {
                if(loader.SupportsExtension(extension))
                {
                    try
                    {
                        return loader.Load(path);
                    } 
                    catch(Exception ex) when (ex is UnsupportedMusicFormat || ex is NotImplementedException)
                    {
                        Console.WriteLine("Format " + extension + " wird (teilweise?) nicht unterstützt.");
                        continue;
                    }
                }
            }

            throw new UnsupportedMusicFormat();
        }
    }

    /// <summary>
    /// Zur referenz: das muss geparst werden https://id3.org/id3v2.4.0-structure
    /// </summary>
    public class MP3Loader : IMusicLoader
    {

        public class ID3v2Data
        {

        }


        public static readonly MP3Loader Instance = new MP3Loader();

        private MP3Loader() { }

        public bool SupportsExtension(string extension)
        {
            return extension == "mp3";
        }

        public Music Load(string path)
        {
            var reader = new StreamDecoder(new FileStream(path, FileMode.Open, FileAccess.Read));
            reader.Endian = StreamDecoder.Endianness.Big;

            var magic = Encoding.ASCII.GetString(reader.Bytes(3));
            if (magic != "ID3") throw new UnsupportedMusicFormat();

            var version = (hi: reader.U8(), lo: reader.U8());
            if(version.hi != 2)
            {
                throw new NotImplementedException();
            }

            byte flags = reader.U8();
            uint size = (uint)((reader.U8() & 0x7F << 21) | (reader.U8() & 0x7F << 14) | (reader.U8() & 0x7F << 7) | (reader.U8() & 0xFF));

            return null;
        }
    }
    
    public class UnsupportedMusicFormat : Exception
    {
        // <3
    }

}
