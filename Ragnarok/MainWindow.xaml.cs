using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Windows.Interop;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Ragnarok
{
    public class MyWindow : MetroWindow
    {
        public MyWindow()
        {
            MouseDoubleClick += (sender, e) =>
            {
                e.Handled = true;
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle Win32 message
            if (msg == WM_NCRBUTTONDOWN || msg == WM_NCRBUTTONUP || msg == WM_NCRBUTTONDBLCLK ||
                msg == WM_RBUTTONDOWN || msg == WM_RBUTTONUP || msg == WM_RBUTTONDBLCLK)
            {
                handled = true;
            }

            return IntPtr.Zero;
        }

        private const int HTCAPTION = 2;
        private const int WM_NCRBUTTONDOWN = 0xA4;
        private const int WM_NCRBUTTONUP = 0xA4;
        private const int WM_NCRBUTTONDBLCLK = 0xA6;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_RBUTTONDBLCLK = 0x206;

        protected BitmapImage LoadImageFromUrl(string url)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = WEBQQ.wc.OpenRead(url);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MyWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindowLoaded;
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = new ViewModel();
        }

        private static bool IsNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            foreach (char c in str)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        private void RestrictNumber(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumber(e.Text))
            {
                e.Handled = true;
            }
            else
                e.Handled = false;
        }

        private void LoginButton(object sender, MouseButtonEventArgs e)
        {
            if (QQ.Text.Length == 0 || Pwd.Password.Length == 0)
                return;
            Busying.IsSelected = true;
            doLogin();
        }

        private void EnterKeyLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Login_Tab.IsSelected == true)
            {
                Busying.IsSelected = true;
                doLogin();
            }
        }

        private void OnMouseUP(object sender, MouseButtonEventArgs e)
        {
            if (ErrorMsg_tab.IsSelected == true)
            {
                GoBackToLogin();
                e.Handled = true;
            }   
        }

        private void GoBackToLogin()
        {
            Login_Tab.IsSelected = true;
        }

        private void OnKeyUP(object sender, KeyEventArgs e)
        {
            if (Login_Tab.IsSelected == true)
                EnterKeyLogin(sender, e);
            else if (ErrorMsg_tab.IsSelected == true)
                GoBackToLogin();
        }

        public void showContact()
        {
            Avatar_Image.Source = LoadImageFromUrl(WEBQQ._face);
            Avatar.Visibility = Visibility.Visible;
            Contact_tab.Visibility = Visibility.Visible;
            Login_Tab.Visibility = Visibility.Hidden;
            Login_Tab.Header = ""; 
            Nick_Text.Text = WEBQQ._info["nick"].ToString();
            Contact_tab.IsSelected = true;        
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        List<PanoramaGroup> plist = new List<PanoramaGroup>();
        public ViewModel()
        { 
            foreach (Category cate in WEBQQ.friendInfo.Categories)
            {
                PanoramaGroup temp = new PanoramaGroup(cate.name);
                temp.SetSource(WEBQQ.friendInfo.Friends.Take(25));
                plist.Add(new PanoramaGroup(cate.name));
            }

            Groups = new ObservableCollection<PanoramaGroup>(plist);
        }

        public ObservableCollection<PanoramaGroup> Groups { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
