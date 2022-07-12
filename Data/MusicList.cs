using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// verfasst von Richard Förster

namespace Project.Data
{
    /// <summary>
    /// eine IMusicList soll eine Liste von Musik abstrahieren,
    /// sodass nicht nur fest eingetragene Listen in der Datenbank
    /// Musiklisten sind, sondern auch beispielsweise Musiklisten wie "alle eingetragenen Lieder"
    /// 
    /// Ist ehrlich geschrieben eher eine ViewModel angelegenheit, aber
    /// nun ist die Datei eben einmal hier.
    /// </summary>
    public interface IMusicList
    {
        string Name { get; }
        string? CoverArtSource { get; }
        string? OwnerName { get; }
        string? Description { get; }
        IEnumerable<Music> Entries { get; }

        int Count => Entries.Count();
    }

    /// <summary>
    /// Musikliste, die alle Lieder ohne Album auflistet.
    /// </summary>
    public class UnregisteredMusicList : IMusicList
    {
        public string Name => "Nicht eingeordnet";
        public string? CoverArtSource => "/UI/Images/heart_hover.png";
        public string? OwnerName => null;
        public string? Description => null;

        public IEnumerable<Music> Entries => Database.Instance.GetMusicWithoutAlbum();
        
    }


    /// <summary>
    /// Musikliste, die jedes Lied auflistet.
    /// </summary>
    public class AllMusicList : IMusicList
    {
        public string Name => "Alle Lieder";
        public string? CoverArtSource => "/UI/Images/heart_hover.png";
        public string? OwnerName => null;
        public string? Description => null;

        public IEnumerable<Music> Entries => Database.Instance.GetMusic();

    }

    /// <summary>
    /// Spezielle Musikliste, die sich auf die Lieblingslieder-Liste konzentriert.
    /// </summary>
    public class FavouritesList : IMusicList
    {
        public string Name => "Lieblingslieder";
        public string? CoverArtSource => "/UI/Images/heart_hover.png";
        public string? OwnerName => null;
        public string? Description => null;

        public IEnumerable<Music> Entries => Database.Instance.GetMusicList(Database.FavouritesPlaylistId).MusicEntries.Target;
    }
}
