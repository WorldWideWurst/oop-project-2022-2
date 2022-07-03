using Project.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project.UI.MVVM.View
{

	/// <summary>
	/// Interaktionslogik für CurrentListDisplay.xaml
	/// </summary>
	public partial class CurrentListDisplay : UserControl
    {

        public class CurrentListMusicViewModel : INotifyPropertyChanged
        {
            public Music Music;
            public int OwnIndex;

            public event PropertyChangedEventHandler? PropertyChanged;

            public CurrentListMusicViewModel(Music music, int ownIndex)
            {
                Music = music;
                OwnIndex = ownIndex;
                Player.Player.Instance.CurrentMusicChanged += (music, index) =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCurrentlyPlaying)));
                };
            }

            public string Description
            {
                get
                {
                    var sb = new StringBuilder();
                    sb.Append(Music.Title != null ? Music.Title : (Music.Sources.Target.FirstOrDefault().Address ?? "??"));
                    if(Music.Artists.Target.Any())
                    {
                        sb.Append(" - by ");
                        sb.AppendJoin("/", Music.Artists.Target.Select(a => a.ArtistId));
                    }
                    return sb.ToString();
                }
            }

            public bool IsCurrentlyPlaying
            {
                get
                {
                    return OwnIndex == Player.Player.Instance.CurrentIndex;
                }
                set
                {
                    if(value)
                    {
                        Player.Player.Instance.CurrentIndex = OwnIndex;
                    }
                }
            }
        }

        public ObservableCollection<CurrentListMusicViewModel> CurrentListMirror = new();

        public CurrentListDisplay()
        {
            InitializeComponent();
            DataContext = this;

            Player.Player.Instance.CurrentList.CollectionChanged += (list, args) =>
            {
                CurrentListMirror.Clear();
                var l = (IList<Music>)list;
                for(int i = 0; i < l.Count; i++)
                {
                    var music = l[i];
                    var entry = new CurrentListMusicViewModel(music, i);
                    CurrentListMirror.Add(entry);
                }
            };

        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var vm = (CurrentListMusicViewModel)((Button)sender).DataContext;
            Player.Player.Instance.RemoveMusic(CurrentListMirror.IndexOf(vm));
        }
    }
}
