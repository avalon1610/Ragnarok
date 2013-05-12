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
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MyWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            //Loaded += MainWindowLoaded;
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = new MainWindowViewModel();
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

    public class MainWindowViewModel
    {
        readonly PanoramaGroup _albums;
        readonly PanoramaGroup _artists;
        public MainWindowViewModel()
        {
            SampleData.Seed();
            Albums = SampleData.Albums;
            Artists = SampleData.Artists;

            Busy = true;

            _albums = new PanoramaGroup("trending tracks");
            _artists = new PanoramaGroup("trending artists");

            Groups = new ObservableCollection<PanoramaGroup> { _albums, _artists };

            _artists.SetSource(SampleData.Artists.Take(25));
            _albums.SetSource(SampleData.Albums.Take(25));


            Busy = false;
        }

        public ObservableCollection<PanoramaGroup> Groups { get; set; }
        public bool Busy { get; set; }
        public string Title { get; set; }
        public int SelectedIndex { get; set; }
        public List<Album> Albums { get; set; }
        public List<Artist> Artists { get; set; }
    }

    public class Album
    {
        public int AlbumId { get; set; }

        [DisplayName("Genre")]
        public int GenreId { get; set; }

        [DisplayName("Artist")]
        public int ArtistId { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }

        [DisplayName("Album Art URL")]
        public string AlbumArtUrl { get; set; }

        public virtual Genre Genre { get; set; }

        public virtual Artist Artist { get; set; }
    }

    public partial class Genre
    {
        public int GenreId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Album> Albums { get; set; }
    }

    public class Artist
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }
    }

    public static class SampleData
    {
        public static List<Genre> Genres { get; set; }
        public static List<Artist> Artists { get; set; }
        public static List<Album> Albums { get; set; }
        public static void Seed()
        {
            if (Genres != null)
                return;

            Genres = new List<Genre>
            {
                new Genre { Name = "Rock" },
                new Genre { Name = "Jazz" },
                new Genre { Name = "Metal" },
                new Genre { Name = "Alternative" },
                new Genre { Name = "Disco" },
                new Genre { Name = "Blues" },
                new Genre { Name = "Latin" },
                new Genre { Name = "Reggae" },
                new Genre { Name = "Pop" },
                new Genre { Name = "Classical" }
            };

            Artists = new List<Artist>
            {
                new Artist { Name = "Aaron Copland & London Symphony Orchestra" },
                new Artist { Name = "Aaron Goldberg" },
                new Artist { Name = "AC/DC" },
                new Artist { Name = "Accept" },
                new Artist { Name = "Adrian Leaper & Doreen de Feis" },
                new Artist { Name = "Aerosmith" },
                new Artist { Name = "Aisha Duo" },
                new Artist { Name = "Alanis Morissette" },
                new Artist { Name = "Alberto Turco & Nova Schola Gregoriana" },
                new Artist { Name = "Alice In Chains" },
                new Artist { Name = "Amy Winehouse" },
                new Artist { Name = "Anita Ward" }
            };

            Albums = new List<Album>
            {
                new Album {Title = "The Best Of Men At Work", Genre = Genres.Single(g => g.Name == "Rock"), Price = 8.99M, Artist = Artists.Single(a => a.Name == "Accept"), AlbumArtUrl = "/Content/Images/placeholder.gif"},
                new Album {Title = "A Copland Celebration, Vol. I", Genre = Genres.Single(g => g.Name == "Classical"), Price = 8.99M, Artist = Artists.Single(a => a.Name == "Aaron Copland & London Symphony Orchestra"), AlbumArtUrl = "/Content/Images/placeholder.gif"},
                new Album {Title = "Worlds", Genre = Genres.Single(g => g.Name == "Jazz"), Price = 8.99M, Artist = Artists.Single(a => a.Name == "Aaron Goldberg"), AlbumArtUrl = "/Content/Images/placeholder.gif"},
                new Album {Title = "For Those About To Rock We Salute You", Genre = Genres.Single(g => g.Name == "Rock"), Price = 8.99M, Artist = Artists.Single(a => a.Name == "AC/DC"), AlbumArtUrl = "/Content/Images/placeholder.gif"},
                new Album {Title = "Let There Be Rock", Genre = Genres.Single(g => g.Name == "Rock"), Price = 8.99M, Artist = Artists.Single(a => a.Name == "AC/DC"), AlbumArtUrl = "/Content/Images/placeholder.gif"},
                new Album {Title = "Balls to the Wall", Genre = Genres.Single(g => g.Name == "Rock"), Price = 8.99M, Artist = Artists.Single(a => a.Name == "Accept"), AlbumArtUrl = "/Content/Images/placeholder.gif"},
                new Album {Title = "Restless and Wild", Genre = Genres.Single(g => g.Name == "Rock"), Price = 8.99M, Artist = Artists.Single(a => a.Name == "Accept"), AlbumArtUrl = "/Content/Images/placeholder.gif"},
            };
        }

    }
}
