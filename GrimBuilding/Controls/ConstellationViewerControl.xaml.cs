using LiteDB;
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

namespace GrimBuilding.Controls
{
    public partial class ConstellationViewerControl : UserControl
    {
        public LiteDatabase MainDatabase
        {
            get { return (LiteDatabase)GetValue(MainDatabaseProperty); }
            set { SetValue(MainDatabaseProperty, value); }
        }

        public static readonly DependencyProperty MainDatabaseProperty =
            DependencyProperty.Register(nameof(MainDatabase), typeof(LiteDatabase), typeof(ConstellationViewerControl));

        public ConstellationViewerControl()
        {
            InitializeComponent();
        }

        Point lastDrag;
        bool dragging;
        private void RootView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                (dragging, lastDrag) = (true, e.GetPosition(this));
                CaptureMouse();
            }
        }

        private void RootView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentDrag = e.GetPosition(this);
                var delta = currentDrag - lastDrag;

                var matrix = matrixTransform.Matrix;
                matrix.Translate(delta.X, delta.Y);
                matrixTransform.Matrix = matrix;

                lastDrag = currentDrag;
            }
        }

        private void RootView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
            ReleaseMouseCapture();
        }

        private void RootView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var matrix = matrixTransform.Matrix;
            var scaleFactor = e.Delta < 0 ? 0.9 : 1.1;
            matrix.Scale(scaleFactor, scaleFactor);
            matrixTransform.Matrix = matrix;
        }
    }
}
