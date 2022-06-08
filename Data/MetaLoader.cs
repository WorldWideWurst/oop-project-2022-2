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

        public long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

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


        public byte[] Bytes(uint n)
        {
            if(Length < n)
            {
                throw new EndOfStreamException();
            }

            byte[] bytes = new byte[n];
            stream.Read(bytes, 0, (int)n);
            return bytes;
        }
        public int Bytes(byte[] buf, int offset, int count) => stream.Read(buf, offset, count);
        
        public void Skip(uint ct)
        {
            stream.Seek(ct, SeekOrigin.Current);
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}

namespace Project.Data
{
    public interface IMetaLoader
    {
        bool SupportsExtension(string extension);

        MusicFileMeta Load(string path);
    }

    public class MusicFileMeta
    {
        public string? File;
        public string? Title;
        public string? Album;
        public string? OriginalAlbum;
        public readonly IList<string> Artists = new List<string>();
        public readonly IList<string> OriginalArtists = new List<string>();

        public MusicFileMeta(string? path)
        {
            File = path;
        }
    }


    /// <summary>
    /// Bekannte Musik-Meta formate:
    /// * ID3v*
    /// * APEtag
    /// * iXML
    /// * MP4 Boxes
    /// * Quicktime Atoms
    /// * OGG
    /// * FLAC
    /// * OPUS
    /// * Speex
    /// * Theora
    /// * VorbisComment
    /// * WMA native metadata
    /// * AIFF/AIFC Metadata
    /// * Matrosca
    /// 
    /// Tag-Conversion-Tabelle: https://wiki.hydrogenaud.io/index.php?title=Tag_Mapping
    /// </summary>
    public class MetaLoader : IMetaLoader
    {

        public static readonly MetaLoader Instance = new MetaLoader();

        static MetaLoader()
        {
            Instance.Loaders.Add(ID3v2MetaLoader.Instance);
            Instance.Loaders.Add(MP3MetaLoader.Instance);
            Instance.Loaders.Add(WAVAccepter.Instance);
        }

        public IList<IMetaLoader> Loaders { get; } = new List<IMetaLoader>();

        public bool SupportsExtension(string extension)
        {
            return Loaders.Any(loader => loader.SupportsExtension(extension));
        }

        public MusicFileMeta Load(string path)
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
                    catch(Exception ex) when (ex is UnknownMusicFileFormat || ex is NotImplementedException)
                    {
                        Console.WriteLine("Format " + extension + " wird (teilweise?) nicht unterstützt.");
                        continue;
                    }
                }
            }

            throw new UnknownMusicFileFormat(extension);
        }
    }

    public class MP3MetaLoader : IMetaLoader
    {

        public static readonly MP3MetaLoader Instance = new MP3MetaLoader();

        private MP3MetaLoader() { }

        public bool SupportsExtension(string extension)
        {
            return "mp3" == extension;
        }
        public MusicFileMeta Load(string path)
        {
            var reader = new StreamDecoder(new FileStream(path, FileMode.Open, FileAccess.Read));
            if ((reader.U32BE() & 0b11111111_11110000_00000001_00000000) == 0b11111111_11110000_00000000_00000000)
            {
                // sieht nach mp3-header aus. Keine Meta vorhanden
                return new MusicFileMeta(path);
            }
            else
            {
                throw new UnknownMusicFileFormat();
            }
        }

    }

    /// <summary>
    /// Zur referenz: das muss geparst werden https://id3.org/id3v2.4.0-structure
    /// </summary>
    public class ID3v2MetaLoader : IMetaLoader
    {


        public static readonly ID3v2MetaLoader Instance = new ID3v2MetaLoader();

        private ID3v2MetaLoader() { }

        public bool SupportsExtension(string extension)
        {
            return extension == "mp3"; // TODO: gibt es ID3v2 header auch wo anders?
        }

        public MusicFileMeta Load(string path)
        {
            var reader = new StreamDecoder(new FileStream(path, FileMode.Open, FileAccess.Read));
            reader.Endian = StreamDecoder.Endianness.Big;

            // Magic lesen: "ID3"
            var magic = Encoding.ASCII.GetString(reader.Bytes(3));
            if (magic != "ID3") throw new UnknownMusicFileFormat();

            // 2 byte version
            var version = (hi: reader.U8(), lo: reader.U8());

            // 1 byte flag
            byte flags = reader.U8();
            // 4 byte size
            uint size = (uint)(((reader.U8() & 0x7F) << 21) | ((reader.U8() & 0x7F) << 14) | ((reader.U8() & 0x7F) << 7) | (reader.U8() & 0xFF));
            
            var meta = new MusicFileMeta(path);

            if(version.hi == 4)
            {
                // check flags
                if((flags & 0b00001111) != 0)
                {
                    throw new InvalidMusicFileFormat();
                }

                if((flags & 0b00100000) > 0)
                {
                    // extended header stuff, skip it
                    uint extHeaderSize = (uint)(((reader.U8() & 0x7F) << 21) | ((reader.U8() & 0x7F) << 14) | ((reader.U8() & 0x7F) << 7) | (reader.U8() & 0xFF));
                    reader.Skip(2 + extHeaderSize);
                }

                var remainingSize = size;
                while(remainingSize > 0)
                {
                    var frameName = Encoding.ASCII.GetString(reader.Bytes(4));
                    uint frameSize = (uint)(((reader.U8() & 0x7F) << 21) | ((reader.U8() & 0x7F) << 14) | ((reader.U8() & 0x7F) << 7) | (reader.U8() & 0xFF));
                    if (frameSize <= 0)
                    {
                        reader.Skip(remainingSize);
                        break;
                    }
                    ushort frameFlags = reader.U16();

                    byte[] data = reader.Bytes(frameSize);
                    remainingSize -= 4 + 4 + 2 + frameSize;

                    switch(frameName)
                    {
                        case "TIT2":
                            // title
                            meta.Title = DecodeTextContent(data);
                            break;
                        case "TALB":
                            // album
                            meta.Album = DecodeTextContent(data);
                            break;
                        case "TOAL":
                            // original album
                            if(meta.Album == null) meta.Album = DecodeTextContent(data);
                            break;
                        case "TPE1":
                            // artists
                        case "TOPE":
                            // original artists
                            foreach (var artistRaw in DecodeTextContent(data).Split("/"))
                            {
                                meta.Artists.Add(artistRaw.Trim());
                            }
                            break;
                    }
                }

            }
            else if (version.hi == 3)
            {
                bool unsyncFlag = (flags & 0b10000000) > 0;
                bool extHeaderFlag = (flags & 0b01000000) > 0;
                bool expFlag = (flags & 0b00100000) > 0;

                if(extHeaderFlag)
                {
                    uint headerSize = reader.U32();
                    reader.Skip(headerSize + 2 + 4);
                }

                var remainingSize = size;
                while (remainingSize > 0)
                {
                    var frameName = Encoding.ASCII.GetString(reader.Bytes(4));
                    uint frameSize = reader.U32();
                    if (frameSize == 0)
                    {
                        reader.Skip(remainingSize);
                        break;
                    }
                    ushort frameFlags = reader.U16();

                    byte[] data = reader.Bytes(frameSize);
                    remainingSize -= 4 + 4 + 2 + frameSize;

                    switch (frameName)
                    {
                        case "TIT2":
                            // title
                            meta.Title = DecodeTextContent(data);
                            break;
                        case "TALB":
                            // album
                            meta.Album = DecodeTextContent(data);
                            break;
                        case "TOAL":
                            // original album
                            if (meta.Album == null) meta.Album = DecodeTextContent(data);
                            break;
                        case "TPE1":
                        // artists
                        case "TOPE":
                            // original artists
                            foreach (var artistRaw in DecodeTextContent(data).Split("/"))
                            {
                                meta.Artists.Add(artistRaw.Trim());
                            }
                            break;
                    }
                }


            }
            else if(version.hi == 2)
            {
                var remainingSize = size;
                while (remainingSize > 0)
                {

                    var frameName = Encoding.ASCII.GetString(reader.Bytes(3));
                    var frameSize = (uint)((reader.U8() << 16) | (reader.U8() << 8) | (reader.U8() & 0xFF));
                    if(frameSize <= 0)
                    {
                        reader.Skip(remainingSize);
                        break;
                    }
                    var data = reader.Bytes(frameSize);
                    remainingSize -= frameSize + 3 + 3;
                    
                    switch(frameName)
                    {
                        case "TAL":
                            // album
                            meta.Album = DecodeTextContent(data);
                            break;
                        case "TOT":
                            // original album
                            if (meta.Album == null)
                                meta.Album = DecodeTextContent(data);
                            break;
                        case "TT2":
                            // Title
                            meta.Title = DecodeTextContent(data);
                            break;
                        case "TP1":
                            // artists
                        case "TOA":
                            // original artists
                            foreach (var artistRaw in DecodeTextContent(data).Split("/"))
                            {
                                meta.Artists.Add(artistRaw.Trim());
                            }
                            break;
                    }
                }

            }

            return meta;
        }

        private static Encoding byteToEncoding(byte b)
        {
            return b switch
            {
                0 => Encoding.Latin1,
                1 => Encoding.Unicode,
                2 => Encoding.BigEndianUnicode,
                3 => Encoding.UTF8,
                _ => throw new InvalidMusicFileFormat()
            };
        }

        /// <summary>
        /// Scheiß Micorosft hat mir ganze 3 Stunden Lebenszeit geraubt FÜR DIESEN SCHEIß
        /// </summary>
        /// <param name="str">DEINE MUTTER</param>
        /// <returns>AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA</returns>
        private static string stripBOM(string str)
        {
            return str[0] == '\uFEFF' ? str[1..] : str;
        }

        private static string DecodeTextContent(byte[] bytes)
        {
            var enc = byteToEncoding(bytes[0]);
            var str = enc.GetString(bytes, 1, bytes.Length - 1).TrimEnd('\0');
            str = stripBOM(str);
            return str;
        }
    }

    public class WAVAccepter : IMetaLoader
    {

        public static readonly WAVAccepter Instance = new WAVAccepter();

        public MusicFileMeta Load(string path)
        {
            return new MusicFileMeta(path);
        }

        public bool SupportsExtension(string extension)
        {
            return extension == "wav";
        }
    }

    public class UnknownMusicFileFormat : Exception
    {
        // <3
        public UnknownMusicFileFormat() { }

        public UnknownMusicFileFormat(string extension)
            : base($"Unbekanntes Musikformat .{extension}") { }
    }

    public class InvalidMusicFileFormat : Exception
    {

    }

}
