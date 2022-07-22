using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace OpenSAE.Controls
{
    public class CoordinateCanvas : Canvas
    {
        public static readonly DependencyProperty CoordinateWidthProperty =
            DependencyProperty.Register(
              name: "CoordinateWidth",
              propertyType: typeof(double),
              ownerType: typeof(CoordinateCanvas),
              typeMetadata: new FrameworkPropertyMetadata(defaultValue: 100d, FrameworkPropertyMetadataOptions.AffectsArrange)
        );

        public double CoordinateWidth
        {
            get => (double)GetValue(CoordinateWidthProperty);
            set => SetValue(CoordinateWidthProperty, value);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                double left = GetLeft(child);
                double top = GetTop(child);
                
                child.Arrange(new Rect(ToCanvas(left, top), child.DesiredSize));
            }
            return arrangeSize;
        }

        private double CoordinateScale => ActualWidth / CoordinateWidth;

        private double OffsetX => ActualWidth / 2;

        private double OffsetY => ActualHeight / 2;

        public CoordinateCanvas()
            : base()
        {
            DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement))
                .AddValueChanged(this, (_, __) => InvalidateArrange());

            DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(FrameworkElement))
                .AddValueChanged(this, (_, __) => InvalidateArrange());
        }

        Point ToCanvas(double x, double y)
        {
            return new Point((x * CoordinateScale) + OffsetX, (y * CoordinateScale) + OffsetY);
        }
    }
}
