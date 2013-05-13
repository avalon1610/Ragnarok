using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ragnarok
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : MyWindow
    {
        public PopupWindow()
        {
            InitializeComponent();
            //ic = new ImageContainer();
            //this.DataContext = ic;
            LoadVerifyImage();
            Loaded += (sender, e) =>
            {
                this.Left = this.Owner.Left + this.Owner.Width / 2 - this.Width / 2;
                this.Top = this.Owner.Top + this.Owner.Height / 2 - this.Height / 2;
            };
        }

        //public ImageContainer ic { get; set; }

        private void LoadVerifyImage()
        {
            Random rand = new Random();
            /*BitmapImage image = new BitmapImage();
            image.BeginInit();*/
            string url = "http://captcha.qq.com/getimage?aid=1003903&r=" + rand.NextDouble() + "&uin=" + WEBQQ._qq + "&vc_type=" + WEBQQ._verifyCode;
            /*
            image.StreamSource = WEBQQ.wc.OpenRead(url);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            V_Image.Source = image;*/
            V_Image.Source = LoadImageFromUrl(url);
            //ic.VerifyImage = image;
        }

        private void OnOK(object sender, MouseButtonEventArgs e)
        {
            Submit();
        }

        private void OnOK(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Submit();
        }
    }

    /*
    public class ImageContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ImageSource verify_image;
        public ImageSource VerifyImage
        {
            get { return verify_image; }
            set
            {
                if (verify_image != value)
                {
                    verify_image = value;
                    propchanged("verify_image");
                }
            }
        }

        private void propchanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
     */

}
