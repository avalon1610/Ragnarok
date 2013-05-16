using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ragnarok
{
    public class Recent
    {
        public dynamic image { get; set; }
        public string name { get; set; }

        public Recent(dynamic image,string name,long uin)
        {
            this.image = image;
            this.name = name;
        }
    }

    public class RecentCollection : INPCBase
    {
        public RecentCollection() { }
        private List<Recent> items = new List<Recent>();

        public void addRecent(Recent item)
        {
            items.Add(item);
            RecentItems = new ObservableCollection<Recent>(items);
        }
        private ObservableCollection<Recent> recentItems;
        public ObservableCollection<Recent> RecentItems
        {
            get { return this.recentItems; }
            set
            {
                if (value != this.recentItems)
                {
                    this.recentItems = value;
                    NotifyPropertyChanged("RecentItems");
                }
            }
        }

    }
}
