using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HappyMail.Domain
{
    public class MailMenuItem : INotifyPropertyChanged
    {
        private string _name;
        private object _content;
        private object _iconContent;
        private int? _numberOfEmails;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public object Content
        {
            get { return _content; }
            set
            {
                if (value != this._content)
                {
                    this._content = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public object IconContent
        {
            get { return _iconContent; }
            set
            {
                if (value != this._iconContent)
                {
                    this._iconContent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int? NumberOfEmails
        {
            get { return _numberOfEmails; }
            set
            {
                if (value != this._numberOfEmails)
                {
                    this._numberOfEmails = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
