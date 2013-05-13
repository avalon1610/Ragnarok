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
using System.Runtime.CompilerServices;
using System.Windows.Threading;

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

     
    }

    public class ViewModel : INotifyPropertyChanged
    {
        List<PanoramaGroup> plist = new List<PanoramaGroup>();
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
    }

    public class myPanorama : Panorama
    {
        public static readonly DependencyProperty MouseScrollEnabledProperty = DependencyProperty.Register("MouseScrollEnabled", typeof(bool), typeof(myPanorama), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty HorizontalScrollBarEnabledProperty = DependencyProperty.Register("HorizontalScrollBarEnabled", typeof(bool), typeof(myPanorama), new FrameworkPropertyMetadata(true));

        public bool MouseScrollEnabled
        {
            get { return (bool)GetValue(MouseScrollEnabledProperty); }
            set { SetValue(MouseScrollEnabledProperty, value); }
        }

        public bool HorizontalScrollBarEnabled
        {
            get { return (bool)GetValue(HorizontalScrollBarEnabledProperty); }
            set { SetValue(HorizontalScrollBarEnabledProperty, value); }
        }

        private DispatcherTimer scrollBarTimer = new DispatcherTimer(DispatcherPriority.DataBind);
        private ScrollViewer sv;
        private Point scrollTarget;
        private Point scrollStartPoint;
        private Point scrollStartOffset;
        private static int PixelsToMoveToBeConsideredScroll = 5;
        //private static int PixelsToMoveToBeConsideredClick = 2;

        public myPanorama()
        {
            scrollBarTimer.Interval = TimeSpan.FromSeconds(1);
            scrollBarTimer.Tick += (s, e) => HideHorizontalScrollBar();
        }

        private void HideHorizontalScrollBar()
        {
            // Ignore when scroll happen with mouse drag or not to be viewed
            if (!HorizontalScrollBarEnabled || Mouse.LeftButton == MouseButtonState.Pressed) return;

            // Hide the scrollbar
            scrollBarTimer.Stop();
            if (sv.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible)
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void ShowHorizontalScrollBar()
        {
            // Ignore if scrollbar is visible yet or not to be viewed
            if (!HorizontalScrollBarEnabled || sv.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible) return;

            // Restart the timer and show the scrollbar
            scrollBarTimer.Stop();
            if (sv.HorizontalScrollBarVisibility == ScrollBarVisibility.Hidden)
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            scrollBarTimer.Start();
        }

        public override void OnApplyTemplate()
        {
            sv = (ScrollViewer)Template.FindName("PART_ScrollViewer", this);

            // Apply the handler for scrollbar visibility
            sv.ScrollChanged += (s, e) =>
            {
                if (HorizontalScrollBarEnabled && Math.Abs(e.HorizontalChange) > PixelsToMoveToBeConsideredScroll)
                    ShowHorizontalScrollBar();
            };

            base.OnApplyTemplate();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (!MouseScrollEnabled) return;

            // Pause the scrollbar timer
            if (scrollBarTimer.IsEnabled)
                scrollBarTimer.Stop();

            // Determine the new amount to scroll.
            scrollTarget.X = sv.HorizontalOffset + ((e.Delta * -1) / 3);

            // Scroll to the new position.
            sv.ScrollToHorizontalOffset(scrollTarget.X);
            CaptureMouse();

            // Save starting point, used later when determining how much to scroll.
            scrollStartPoint = e.GetPosition(this);
            scrollStartOffset.X = sv.HorizontalOffset;

            // Restart the scrollbar timer
            if (HorizontalScrollBarEnabled)
                scrollBarTimer.Start();

            base.OnPreviewMouseWheel(e);
        }
    }
}
