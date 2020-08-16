using GrimBuildingCodecs;
using LiteDB;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
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

namespace GrimBuilding.Controls
{
    /// <summary>
    /// Interaction logic for DatabaseImageControl.xaml
    /// </summary>
    public partial class DatabaseImageControl : UserControl
    {
        public LiteDatabase MainDatabase
        {
            get { return (LiteDatabase)GetValue(MainDatabaseProperty); }
            set { SetValue(MainDatabaseProperty, value); }
        }

        public static readonly DependencyProperty MainDatabaseProperty =
            DependencyProperty.Register(nameof(MainDatabase), typeof(LiteDatabase), typeof(DatabaseImageControl), new PropertyMetadata((d, e) => ((DatabaseImageControl)d).UpdateCurrentImageModel()));

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(string), typeof(DatabaseImageControl), new PropertyMetadata((d, e) => ((DatabaseImageControl)d).UpdateCurrentImageModel()));

        readonly static Dictionary<string, ImageModel> cache = new Dictionary<string, ImageModel>();

        public class ImageModel : ReactiveObject
        {
            BitmapSource bitmap;
            public BitmapSource Bitmap { get => bitmap; set => this.RaiseAndSetIfChanged(ref bitmap, value); }
        }

        public ImageModel CurrentImageModel
        {
            get { return (ImageModel)GetValue(CurrentImageModelProperty); }
            private set { SetValue(CurrentImageModelProperty, value); }
        }

        public static readonly DependencyProperty CurrentImageModelProperty =
            DependencyProperty.Register("CurrentImageModel", typeof(ImageModel), typeof(DatabaseImageControl));

        private void UpdateCurrentImageModel()
        {
            if (MainDatabase is null || Path is null)
                CurrentImageModel = null;
            else
            {
                if (!cache.TryGetValue(Path, out var img))
                {
                    cache[Path] = img = new ImageModel();
                    var db = MainDatabase;
                    var path = Path;

                    Task.Run(() =>
                    {
                        using var stream = db.FileStorage.OpenRead(path);
                        var bytes = new byte[stream.Length];
                        stream.Read(bytes);

                        if (!WebP.Decode(bytes, out var hasAlpha, out var width, out var height, out var stride, out var outputBytes))
                            throw new InvalidOperationException();

                        var bmp = BitmapSource.Create(width, height, 0, 0, hasAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr24, null, outputBytes, stride);
                        bmp.Freeze();

                        Dispatcher.BeginInvoke(new Action(() => img.Bitmap = bmp));
                    });
                }
                CurrentImageModel = img;
            }
        }

        public DatabaseImageControl()
        {
            InitializeComponent();
        }
    }
}
