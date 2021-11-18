using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    public class Contact : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string ip { get; set; }
        public string port { get; set; }

        private bool _IsSelected = false;
        public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; OnChanged("IsSelected"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
