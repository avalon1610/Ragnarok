using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ragnarok
{
    /// <summary>
    /// Represents a grouping of tiles
    /// </summary>
    public class PanoramaGroup
    {
        public PanoramaGroup(string header, ICollectionView tiles)
        {
            this.Header = header;
            this.Tiles = tiles;
        }
        public string Header { get; private set; }
        public ICollectionView Tiles { get; private set; }
    }

    /// <summary>
    /// Interaction logic for myPanorama.xaml
    /// </summary> 
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
    public partial class myPanorama : ItemsControl
    {
        #region Data
        private ScrollViewer sv = new ScrollViewer();
        private Point scrollTarget;
        private Point scrollStartPoint;
        private Point scrollStartOffset;
        private Point previousPoint;
        private Vector velocity;
        private double friction;
        private DispatcherTimer animationTimer = new DispatcherTimer(DispatcherPriority.DataBind);
        private static int PixelsToMoveToBeConsiderScroll = 5;
        //private static int PixelsToMoveToBeConsiderClick = 2;
        private Random rand = new Random(DateTime.Now.Millisecond);
        private bool _mouseDownFlag;
        private Cursor _savedCursor;
        #endregion

        #region Ctor
        public myPanorama()
        {
            friction = 0.85;

            animationTimer.Interval = new System.TimeSpan(0, 0, 0, 0, 20);
            animationTimer.Tick += new EventHandler(HandleWorldTimerTick);
            animationTimer.Start();

            TileColors = new Brush[] {
                new SolidColorBrush(Color.FromRgb((byte)111,(byte)189,(byte)69)),
                new SolidColorBrush(Color.FromRgb((byte)75,(byte)179,(byte)221)),
                new SolidColorBrush(Color.FromRgb((byte)65,(byte)100,(byte)165)),
                new SolidColorBrush(Color.FromRgb((byte)225,(byte)32,(byte)38)),
                new SolidColorBrush(Color.FromRgb((byte)128,(byte)0,(byte)128)),
                new SolidColorBrush(Color.FromRgb((byte)0,(byte)128,(byte)64)),
                new SolidColorBrush(Color.FromRgb((byte)0,(byte)148,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)0,(byte)199)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)135,(byte)15)),
                new SolidColorBrush(Color.FromRgb((byte)45,(byte)255,(byte)87)),
                new SolidColorBrush(Color.FromRgb((byte)127,(byte)0,(byte)55))
    
            };

            ComplimentaryTileColors = new Brush[] {
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255)),
                new SolidColorBrush(Color.FromRgb((byte)255,(byte)255,(byte)255))
            };
        }

        static myPanorama()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(myPanorama), new FrameworkPropertyMetadata(typeof(myPanorama)));
        }
        #endregion

        #region Properties
        public double Friction
        {
            get { return 1.0 - friction; }
            set { friction = Math.Min(Math.Max(1.0 - value, 0), 1.0); }
        }

        public List<Brush> TileColorPair
        {
            get
            {
                int idx = rand.Next(TileColors.Length);
                return new List<Brush>() { TileColors[idx], ComplimentaryTileColors[idx] };
            }
        }
        #region DPs
        #region ItemBox

        public static readonly DependencyProperty ItemBoxProperty =
            DependencyProperty.Register("ItemHeight", typeof(double), typeof(myPanorama), new FrameworkPropertyMetadata((double)120.0));

        public double ItemBox
        {
            get { return (double)GetValue(ItemBoxProperty); }
            set { SetValue(ItemBoxProperty, value); }
        }
        #endregion

        #region GroupHeight
        public static readonly DependencyProperty GroupHeightProperty =
            DependencyProperty.Register("GroupHeight", typeof(double), typeof(myPanorama), new FrameworkPropertyMetadata((double)640.0));

        public double GroupHeight
        {
            get { return (double)GetValue(GroupHeightProperty); }
            set { SetValue(GroupHeightProperty, value); }
        }
        #endregion

        #region HeaderFontSize
        public static readonly DependencyProperty HeaderFontSizeProperty =
            DependencyProperty.Register("HeaderFontSize", typeof(double), typeof(myPanorama), new FrameworkPropertyMetadata((double)30.0));

        public double HeaderFontSize
        {
            get { return (double)GetValue(HeaderFontSizeProperty); }
            set { SetValue(HeaderFontSizeProperty, value); }
        }
        #endregion

        #region HeaderFontColor
        public static readonly DependencyProperty HeaderFontColorProperty =
            DependencyProperty.Register("HeaderFontColor", typeof(Brush), typeof(myPanorama), new FrameworkPropertyMetadata((Brush)Brushes.White));

        public Brush HeaderFontColor
        {
            get { return (Brush)GetValue(HeaderFontColorProperty); }
            set { SetValue(HeaderFontColorProperty, value); }
        }
        #endregion

        #region HeaderFontFamily
        public static readonly DependencyProperty HeaderFontFamilyProperty =
            DependencyProperty.Register("HeaderFontFamily", typeof(FontFamily), typeof(myPanorama), new FrameworkPropertyMetadata((FontFamily)new FontFamily("Segoe UI")));

        public FontFamily HeaderFontFamily
        {
            get { return (FontFamily)GetValue(HeaderFontColorProperty); }
            set { SetValue(HeaderFontFamilyProperty, value); }
        }
        #endregion

        #region TileColors
        public static readonly DependencyProperty TileColorsProperty =
            DependencyProperty.Register("TileColors", typeof(Brush[]), typeof(myPanorama), new FrameworkPropertyMetadata((Brush[])null));

        public Brush[] TileColors
        {
            get { return (Brush[])GetValue(TileColorsProperty); }
            set { SetValue(TileColorsProperty, value); }
        }
        #endregion

        #region ComplimentaryTileColors
        public static readonly DependencyProperty ComplimentaryTileColorsProperty =
            DependencyProperty.Register("ComplimentaryTileColors", typeof(Brush[]), typeof(myPanorama), new FrameworkPropertyMetadata((Brush[])null));

        public Brush[] ComplimentaryTileColors
        {
            get { return (Brush[])GetValue(ComplimentaryTileColorsProperty); }
            set { SetValue(ComplimentaryTileColorsProperty, value); }
        }
        #endregion

        #region UseSnapBackScrolling
        public static readonly DependencyProperty UseSnapBackScrollingProperty =
            DependencyProperty.Register("UseSnapBackScrolling", typeof(bool), typeof(myPanorama), new FrameworkPropertyMetadata((bool)true));

        public bool UseSnapBackScrolling
        {
            get { return (bool)GetValue(UseSnapBackScrollingProperty); }
            set { SetValue(UseSnapBackScrollingProperty, value); }
        }
        #endregion

        #endregion

        #endregion

        #region Private Methods

        private void DoStandardScrolling()
        {
            sv.ScrollToHorizontalOffset(scrollTarget.X);
            sv.ScrollToVerticalOffset(scrollTarget.Y);
            scrollTarget.X += velocity.X;
            scrollTarget.Y += velocity.Y;
            velocity *= friction;
        }

        private void HandleWorldTimerTick(object sender, EventArgs e)
        {
            if (IsLoaded == false)
                return;
            var prop = DesignerProperties.IsInDesignModeProperty;
            bool isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;

            if (isInDesignMode)
                return;

            if (IsMouseCaptured)
            {
                Point currentPoint = Mouse.GetPosition(this);
                velocity = previousPoint - currentPoint;
                previousPoint = currentPoint;
            }
            else
            {
                if (velocity.Length > 1)
                    DoStandardScrolling();
                else
                {
                    if (UseSnapBackScrolling)
                    {
                        int mx = (int)sv.HorizontalOffset % (int)ActualWidth;
                        if (mx == 0)
                            return;
                        int ix = (int)sv.HorizontalOffset / (int)ActualWidth;
                        double snapBackX = mx > ActualWidth / 2 ? (ix + 1) * ActualWidth : ix * ActualWidth;

                        sv.ScrollToHorizontalOffset(sv.HorizontalOffset + (snapBackX - sv.HorizontalOffset) / 4.0);
                    }
                    else
                    {
                        DoStandardScrolling();
                    }
                }
            }
        }

        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            sv = (ScrollViewer)Template.FindName("PART_ScrollViewer", this);
            base.OnApplyTemplate();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (sv.IsMouseOver)
            {
                _mouseDownFlag = true;

                // Save starting point, used later whern determining how mush to scroll
                scrollStartPoint = e.GetPosition(this);
                scrollStartOffset.X = sv.HorizontalOffset;
                scrollStartOffset.Y = sv.VerticalOffset;
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (_mouseDownFlag)
            {
                Point currentPoint = e.GetPosition(this);

                // Determine the new amount to scroll
                Point delta = new Point(scrollStartPoint.X - currentPoint.X, scrollStartPoint.Y - currentPoint.Y);
                if (Math.Abs(delta.X) > PixelsToMoveToBeConsiderScroll ||
                    Math.Abs(delta.Y) > PixelsToMoveToBeConsiderScroll)
                {
                    scrollTarget.X = scrollStartOffset.X + delta.X;
                    scrollTarget.Y = scrollStartOffset.Y + delta.Y;

                    // scroll to the new position
                    sv.ScrollToHorizontalOffset(scrollTarget.X);
                    sv.ScrollToVerticalOffset(scrollTarget.Y);

                    if (!this.IsMouseCaptured)
                    {
                        if ((sv.ExtentWidth > sv.ViewportHeight) ||
                            (sv.ExtentHeight > sv.ViewportHeight))
                        {
                            _savedCursor = this.Cursor;
                            this.Cursor = Cursors.ScrollWE;
                        }

                        this.CaptureMouse();
                    }
                }
            }
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            bool mouseDownFlag = _mouseDownFlag;
            // mouse move events may trigger while inside this handler
            _mouseDownFlag = false;

            if (this.IsMouseCaptured)
            {
                // scroll action stopped
                this.Cursor = _savedCursor;
                this.ReleaseMouseCapture();
            }
            else if (mouseDownFlag)
            {
                // click action stopped
            }

            _savedCursor = null;
            base.OnPreviewMouseLeftButtonUp(e);
        }
        #endregion
    }

    public class PanoramaGroupWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double itemBox = double.Parse(values[0].ToString());
            double groupHeight = double.Parse(values[1].ToString());

            double ratio = groupHeight / itemBox;
            ListBox list = (ListBox)values[2];


            double width = Math.Ceiling(list.Items.Count / ratio);
            width *= itemBox;
            if (width < itemBox)
                return itemBox;
            else
                return width;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Simple delegating command, based largely on DelegateCommand from PRISM/CAL
    /// </summary>
    /// <typeparam name="T">The type for the </typeparam>
    public class SimpleCommand<T1, T2> : ICommand
    {
        private Func<T1, bool> canExecuteMethod;
        private Action<T2> executeMethod;

        public SimpleCommand(Func<T1, bool> canExecuteMethod, Action<T2> executeMethod)
        {
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public SimpleCommand(Action<T2> executeMethod)
        {
            this.executeMethod = executeMethod;
            this.canExecuteMethod = (x) => { return true; };
        }


        public bool CanExecute(T1 parameter)
        {
            if (canExecuteMethod == null) return true;
            return canExecuteMethod(parameter);
        }

        public void Execute(T2 parameter)
        {
            if (executeMethod != null)
            {
                executeMethod(parameter);
            }
        }

        public bool CanExecute(object parameter)
        {
            return CanExecute((T1)parameter);
        }

        public void Execute(object parameter)
        {
            Execute((T2)parameter);
        }

#if SILVERLIGHT
        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
#else
        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (canExecuteMethod != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (canExecuteMethod != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }
#endif



        /// <summary>
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "The this keyword is used in the Silverlight version")]
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
            Justification = "This cannot be an event")]
        public void RaiseCanExecuteChanged()
        {
#if SILVERLIGHT
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#else
            CommandManager.InvalidateRequerySuggested();
#endif
        }
    }
}
