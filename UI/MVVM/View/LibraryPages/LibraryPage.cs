using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.MVVM.View.LibraryPages
{

    public enum LibraryPageState
    {
        Overview,
        Search,
        PreSearch = Search,
    }
    public interface ILibraryPage
    {
        void Overview();
        void Search(string queryString);
    }
}
