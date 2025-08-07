using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ATEDNIULI_NET8.ViewModels;

namespace ATEDNIULI_NET8.Services
{
    public class TagGeometryConverterService : IValueConverter
    {
        private const double Width = 40;
        private const double Height = 30;
        private const double PointerSize = 10;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PointerDirection direction))
                return Geometry.Empty;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                switch (direction)
                {
                    case PointerDirection.Left:
                        ctx.BeginFigure(new Point(0, Height / 2), true, true);
                        ctx.LineTo(new Point(PointerSize, 0), true, false);
                        ctx.LineTo(new Point(Width, 0), true, false);
                        ctx.LineTo(new Point(Width, Height), true, false);
                        ctx.LineTo(new Point(PointerSize, Height), true, false);
                        break;
                    case PointerDirection.Right:
                        ctx.BeginFigure(new Point(Width, Height / 2), true, true);
                        ctx.LineTo(new Point(Width - PointerSize, 0), true, false);
                        ctx.LineTo(new Point(0, 0), true, false);
                        ctx.LineTo(new Point(0, Height), true, false);
                        ctx.LineTo(new Point(Width - PointerSize, Height), true, false);
                        break;
                        // Add cases for Up/Down similarly
                }
                ctx.Close();
            }
            geometry.Freeze();
            return geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

// TODO: implement percetage of overlap with other tags to determine the direction the lesser the overlap that will be the returned direction
// like it will take into account the fixed size of the tags and calculate there, if that is possible which i am sure it is, 100%
// i think it will need to complete list of clickaitem coords so that it can calculate the before and after positions
// because the current implementation currently put tags "live" meaning once the point is calculated/get it immediately draws the tags
// this is implemented in the show items command class with the for loop implementation
// Or just another logic to solve this overlap problem
// like if tags overlap the tags will be different color from each other but i am sure it will implement the algorithm above first
