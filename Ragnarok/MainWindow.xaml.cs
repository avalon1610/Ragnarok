﻿using MahApps.Metro.Controls;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public static BitmapImage LoadImageFromUrl(string url)
        {
            if (url.Length == 0)
                return null;
            MyWebClient wc = WEBQQ.wc_primary.IsBusy ? new MyWebClient(WEBQQ.wc_primary.Cookies) : WEBQQ.wc_primary;
            BitmapImage image = new BitmapImage();
            byte[] imageBytes = wc.DownloadData(url);
            if (imageBytes == null)
                return null;
            MemoryStream imageStream = new MemoryStream(imageBytes);

            image.BeginInit();
            image.StreamSource = imageStream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            imageStream.Close();
            return image;
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            // Search immediate children
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childofChild = FindVisualChild<childItem>(child);
                    if (childofChild != null)
                        return childofChild;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MyWindow
    {
        public static ViewModel viewmodel = new ViewModel();
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (sender, e) => { DataContext = viewmodel; };
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

        private void OnLogoutButton(object sender, RoutedEventArgs e)
        {
            Login_Tab.Header = "Login";
            Pwd.Password = "";
            Recent_tab.Visibility = Visibility.Hidden;
            Contact_tab.Visibility = Visibility.Hidden;
            Group_tab.Visibility = Visibility.Hidden;
            Login_Tab.Visibility = Visibility.Visible;
            Avatar.Visibility = Visibility.Hidden;
            Login_Tab.IsSelected = true;
        }

        public static RecentCollection rc = new RecentCollection();

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            grid.DataContext = rc;
        }

        private void Contact_click(object sender, RoutedEventArgs e)
        {
            Recent_tab.IsSelected = true;

            //for (int i = 0; i < RecentListBox.Items.Count; i++)
            //{
            //    DependencyObject obj = RecentListBox.ItemContainerGenerator.ContainerFromIndex(i);
            //    TextBlock block = FindVisualChild<TextBlock>(obj);
            //    if (block != null && block.Uid == Convert.ToString(rc.now_uin))
            //    {
            //        RecentListBox.SelectedItem = obj as ListBoxItem;
            //        break;
            //    }
            //}
        }
    }

}
