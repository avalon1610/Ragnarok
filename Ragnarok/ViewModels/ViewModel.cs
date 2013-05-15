using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Ragnarok
{
    public class PanoramaTileViewModel : INPCBase
    {
        public string Text { get; private set; }
        public dynamic ImageUrl { get; set; }
        public bool IsDoubleWidth { get; private set; }
        public ICommand TileClickedCommand { get; private set; }
        public int Counter { get; set; }
        public long uin { get; set; }

        public PanoramaTileViewModel(string text, BitmapImage image, long uin, bool isDoubleWidth)
        {
            this.Text = text;
            if (image == null)
                this.ImageUrl = "images/placeholder_person.gif";
            else
                this.ImageUrl = image;
            this.IsDoubleWidth = isDoubleWidth;
            this.uin = uin;
            this.TileClickedCommand = new SimpleCommand<object, object>(ExecuteTileClickedCommand);
        }

        public void ExecuteTileClickedCommand(object parameter)
        {
            MessageBox.Show(string.Format("you clicked {0}", this.Text));
        }
    }


    public class ViewModel : INPCBase
    {
        public ViewModel()
        {
        }

        public void BindingToUI()
        {
            Console.WriteLine("BindingToUI thread:{0}", Thread.CurrentThread.ManagedThreadId);
            PanoramaItems = new ObservableCollection<PanoramaGroup>(data);
        }

        List<PanoramaGroup> data = new List<PanoramaGroup>();

        public void SetData()
        {

            foreach (Category cate in WEBQQ.friendInfo.Categories)
            {
                string url = "";
                long uin = 0;
                List<PanoramaTileViewModel> tiles = new List<PanoramaTileViewModel>();
                foreach (Friend friend in cate.Friends)
                {
                    uin = Convert.ToInt64(friend.uin);
                    url = "http://face" + (uin % 10 + 1) + ".qun.qq.com/cgi/svr/face/getface?cache=0&type=1&fid=0&uin=" + uin + "&vfwebqq=" + WEBQQ._vfwebqq;
                    tiles.Add(new PanoramaTileViewModel(friend.nick, MyWindow.LoadImageFromUrl(url), uin, false));      
                }

                data.Add(new PanoramaGroup(cate.name, CollectionViewSource.GetDefaultView(tiles)));
            }
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
