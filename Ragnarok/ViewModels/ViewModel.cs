using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Ragnarok
{
    public class PanoramaTileViewModel : INPCBase
    {
        public string Text { get; private set; }
        public BitmapImage ImageUrl { get; private set; }
        public bool IsDoubleWidth { get; private set; }

        public PanoramaTileViewModel(string text, BitmapImage image, bool isDoubleWidth)
        {
            this.Text = text;
            this.ImageUrl = image;
            this.IsDoubleWidth = isDoubleWidth;
        }
    }


    public class ViewModel : INPCBase
    {
        //private List<TileData> ContactData = new List<TileData>();
        public ViewModel()
        {
        }

        public void SetData()
        {
            string url = "";
            long uin;
            List<PanoramaGroup> data = new List<PanoramaGroup>();
            foreach (Category cate in WEBQQ.friendInfo.Categories)
            {
                List<PanoramaTileViewModel> tiles = new List<PanoramaTileViewModel>();
                foreach (Friend friend in cate.Friends)
                {
                    uin = Convert.ToInt64(friend.uin);
                    url = "http://face" + (uin % 10 + 1) + ".qun.qq.com/cgi/svr/face/getface?cache=0&type=1&fid=0&uin=" + uin + "&vfwebqq=" + WEBQQ._vfwebqq;
                    
                    tiles.Add(new PanoramaTileViewModel(friend.nick, MyWindow.LoadImageFromUrl(url), false));
                }

                data.Add(new PanoramaGroup(cate.name, CollectionViewSource.GetDefaultView(tiles)));
            }

            PanoramaItems = new ObservableCollection<PanoramaGroup>(data);
        }

        private ObservableCollection<PanoramaGroup> panoramaItems;

        public ObservableCollection<PanoramaGroup> PanoramaItems
        {
            get { return this.panoramaItems; }
            set 
            {
                if (value != this.panoramaItems)
                {
                    this.panoramaItems = value;
                    NotifyPropertyChanged("PanoramaItems");
                }
            }
        }
        /*
        public ViewModel()
        {

            foreach (Category cate in WEBQQ.friendInfo.Categories)
            {
                PanoramaGroup temp = new PanoramaGroup(cate.name);
                int count = WEBQQ.friendInfo.Friends.Count;
                temp.SetSource(cate.Friends.Take(count));
                plist.Add(temp);
            }

            _groups = new ObservableCollection<PanoramaGroup>(plist);
        }

        private ObservableCollection<PanoramaGroup> _groups;
        public ObservableCollection<PanoramaGroup> Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }
         */
    }
}
