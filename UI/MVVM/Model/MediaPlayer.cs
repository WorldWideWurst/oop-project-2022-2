using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.MVVM.Model
{
    class MediaPlayer
    {
		private MediaPlayer mediaPlayer = new MediaPlayer();

		public MediaPlayerAudioControlSample()
		{
			InitializeComponent();

			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
				mediaPlayer.Open(new Uri(openFileDialog.FileName));

			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += timer_Tick;
			timer.Start();
		}

		void timer_Tick(object sender, EventArgs e)
		{
			if (mediaPlayer.Source != null)
				lblStatus.Content = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
			else
				lblStatus.Content = "No file selected...";
		}

		private void PlayCheckbox_Checked(object sender, RoutedEventArgs e)
		{
			mediaPlayer.Play();
		}

		private void PlayCheckbox_Unchecked(object sender, RoutedEventArgs e)
		{
			mediaPlayer.Pause();
		}
	}
}
    }
}
