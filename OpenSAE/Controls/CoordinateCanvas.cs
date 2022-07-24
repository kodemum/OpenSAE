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

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.RegisterAttached(
                "Center",
                typeof(bool),
                typeof(CoordinateCanvas),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(
                "Offset",
                typeof(Point),
                typeof(CoordinateCanvas),
                new FrameworkPropertyMetadata(new Point(0,0), FrameworkPropertyMetadataOptions.AffectsArrange));

        public double CoordinateWidth
        {
            get => (double)GetValue(CoordinateWidthProperty);
            set => SetValue(CoordinateWidthProperty, value);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var offset = Offset;
            offset.X *= CoordinateScale;
            offset.Y *= CoordinateScale;

            foreach (UIElement child in InternalChildren)
            {
                double left = GetLeft(child);
                double top = GetTop(child);

                if (double.IsNaN(left))
                    left = 0;

                if (double.IsNaN(top))
                    top = 0;

                var childPoint = ToCanvas(left, top);

                childPoint.X += offset.X;
                childPoint.Y += offset.Y;

                // center the child relative to the canvas
                if (GetCenter(child))
                {
                    childPoint.X -= (child.DesiredSize.Width - Width) / 2;
                    childPoint.Y -= (child.DesiredSize.Height - Height) / 2;
                }

                child.Arrange(new Rect(childPoint, child.DesiredSize));
            }
            return arrangeSize;
        }

        private double CoordinateScale => ActualWidth / CoordinateWidth;

        private double OffsetX => ActualWidth / 2;

        private double OffsetY => ActualHeight / 2;

        /// <summary>
        /// Sets the origin of the canvas to the center rather than the default upper left.
        /// </summary>
        public bool CenterOrigin { get; set; }

        /// <summary>
        /// Value to use as an offset for all positions in the canvas. Specified in coordinate units.
        /// </summary>
        public Point Offset
        {
            get => (Point)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        public static bool GetCenter(UIElement element)
        {
            return ((bool?)element.GetValue(CenterProperty)) ?? false;
        }

        public static void SetCenter(UIElement element, bool value)
        {
            element.SetValue(CenterProperty, value);
        }

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
            if (CenterOrigin)
                return new Point((x * CoordinateScale) + OffsetX, (y * CoordinateScale) + OffsetY);
            else
                return new Point(x * CoordinateScale, y * CoordinateScale);
        }
    }
}
