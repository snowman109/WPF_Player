using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace WPF_Player
{
   public class PlayList
    {
       
    }
    public class Song
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value;  }
        }
        private string _location;

        public string Location
        {
            get { return _location; }
            set { _location = value; }
        }
    }
}
