using System;
using Project.Music;

namespace Project.Player
{
    public class Player
    {
        public MusicList? CurrentList
        {
            get => _musicList;
            set
            {
                if(value == null)
                {
                    _musicList = null;
                    CurrentIndex = null;
                } 
                else
                {
                    _musicList = value;
                    CurrentIndex = 0;
                }
            }
        }

        private MusicList? _musicList;

        public int? CurrentIndex { get; set; }

        public bool Play
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool Shuffle
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }


        public void NextEntry()
        {
            throw new NotImplementedException();
        }

        public void PreviousEntry()
        {
            throw new NotImplementedException();
        }


    }
}
