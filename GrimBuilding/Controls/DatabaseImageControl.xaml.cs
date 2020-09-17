using GrimBuilding.Common;
using GrimBuilding.Codecs;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;

namespace GrimBuilding.Controls
{
    /// <summary>
    /// Interaction logic for DatabaseImageControl.xaml
    /// </summary>
    public partial class DatabaseImageControl : UserControl
    {
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(string), typeof(DatabaseImageControl), new((d, e) => ((DatabaseImageControl)d).UpdateCurrentImageModel()));

        readonly static Dictionary<string, ImageModel> cache = new();

        internal class ImageModel : ReactiveObject
        {
            BitmapSource bitmap;
            public BitmapSource Bitmap { get => bitmap; set => this.RaiseAndSetIfChanged(ref bitmap, value); }
        }

        internal ImageModel CurrentImageModel
        {
            get { return (ImageModel)GetValue(CurrentImageModelProperty); }
            private set { SetValue(CurrentImageModelProperty, value); }
        }

        public static readonly DependencyProperty CurrentImageModelProperty =
            DependencyProperty.Register("CurrentImageModel", typeof(ImageModel), typeof(DatabaseImageControl));
        public Popup Popup
        {
            get { return (Popup)GetValue(PopupProperty); }
            set { SetValue(PopupProperty, value); }
        }

        public static readonly DependencyProperty PopupProperty =
            DependencyProperty.Register("Popup", typeof(Popup), typeof(DatabaseImageControl));

        private void UpdateCurrentImageModel()
        {
            if (Path is null)
                CurrentImageModel = null;
            else
            {
                if (!cache.TryGetValue(Path, out var img))
                {
                    cache[Path] = img = new();
                    var path = Path;

                    Task.Run(async () =>
                    {
                        // load the image on the thread pool thread and then freeze it
                        using var db = new GdDbContext("data.db");
                        var bytes = (await db.Files.FirstAsync(w => w.Path == path).ConfigureAwait(false)).Data;

                        if (!WebP.Decode(bytes, out var hasAlpha, out var width, out var height, out var stride, out var outputBytes))
                            throw new InvalidOperationException();

                        var bmp = BitmapSource.Create(width, height, 0, 0, hasAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr24, null, outputBytes, stride);
                        bmp.Freeze();

                        // dispatch it to the binding on the main thread
                        _ = Dispatcher.BeginInvoke(new Action(() => img.Bitmap = bmp));
                    });
                }

                CurrentImageModel = img;
            }
        }

        public ICommand LeftCommand
        {
            get { return (ICommand)GetValue(LeftCommandProperty); }
            set { SetValue(LeftCommandProperty, value); }
        }

        public static readonly DependencyProperty LeftCommandProperty =
            DependencyProperty.Register("LeftCommand", typeof(ICommand), typeof(DatabaseImageControl));

        public ICommand RightCommand
        {
            get { return (ICommand)GetValue(RightCommandProperty); }
            set { SetValue(RightCommandProperty, value); }
        }

        public static readonly DependencyProperty RightCommandProperty =
            DependencyProperty.Register("RightCommand", typeof(ICommand), typeof(DatabaseImageControl));

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && LeftCommand?.CanExecute(null) == true)
                LeftCommand?.Execute(null);
            else if (e.RightButton == MouseButtonState.Pressed && RightCommand?.CanExecute(null) == true)
                RightCommand?.Execute(null);

            base.OnPreviewMouseDown(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (Popup is not null)
            {
                Popup.DataContext = DataContext;
                Popup.PlacementTarget = this;
                Popup.IsOpen = true;
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (Popup is not null)
                Popup.IsOpen = false;

            base.OnMouseLeave(e);
        }

        public DatabaseImageControl()
        {
            InitializeComponent();
        }
    }
}
