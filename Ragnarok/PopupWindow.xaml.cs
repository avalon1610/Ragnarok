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
            LoadVerifyImage();
            Loaded += (sender, e) =>
            {
                this.Left = this.Owner.Left + this.Owner.Width / 2 - this.Width / 2;
                this.Top = this.Owner.Top + this.Owner.Height / 2 - this.Height / 2;
            };
        }

        private void LoadVerifyImage()
        {
            Random rand = new Random();
            string url = "http://captcha.qq.com/getimage?aid=1003903&r=" + rand.NextDouble() + "&uin=" + WEBQQ._qq + "&vc_type=" + WEBQQ._verifyCode;
            V_Image.Source = LoadImageFromUrl(url);

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
}
