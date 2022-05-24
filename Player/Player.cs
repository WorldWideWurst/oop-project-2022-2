using System;
using System.Windows.Media;
using Project.Data;
using System.Collections.Generic;

namespace Project.Player
{
    public class Player
    {
        public IList<Music>? CurrentList
        {
            get => _musicList;
            set
            {
                if(value == null)
                {
                    // player stoppen
                    _musicList = null;
                    CurrentIndex = null;
                } 
                else
                {   
                    // neue liste wird abgespielt
                    _musicList = value;
                    CurrentIndex = 0;
                }
            }
        }

        private IList<Music> _musicList;

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


        public void PlayNext()
        {
            throw new NotImplementedException();
        }

        public void PlayPrevious()
        {
            throw new NotImplementedException();
        }


    }
}
