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
        public long uin { get; set; }

        public Recent(dynamic image,string name,long uin)
        {
            this.image = image;
            this.name = name;
            this.uin = uin;
        }
    }

    public class RecentCollection : INPCBase
    {
        public RecentCollection() { }
        private List<Recent> items = new List<Recent>();
        public long now_uin = 0;

        public void addRecent(Recent i)
        {
            now_uin = i.uin;
            foreach (Recent item in items)
            {
                if (item.uin == i.uin)
                    return;
            }
            items.Add(i);         
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
