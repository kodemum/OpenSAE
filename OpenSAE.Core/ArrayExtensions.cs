﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenSAE.Core
{
    public static class ArrayExtensions
    {
        public static double GetCenterX(this Point[] points)
        {
            return (points.Max(x => x.X) + points.Min(x => x.X)) / 2;
        }

        public static double GetCenterY(this Point[] points)
        {
            return (points.Max(x => x.Y) + points.Min(x => x.Y)) / 2;
        }

        public static Point GetCenter(this Point[] points)
        {
            return new Point(points.GetCenterX(), points.GetCenterY());
        }

        /// <summary>
        /// Gets the next item from the specified index in the array, unless it is the last item
        /// in which case the first item of the array is returned.
        /// </summary>
        /// <exception cref="InvalidOperationException">If array is empty</exception>
        public static T NextOrWrap<T>(this T[] array, int index)
        {
            if (array.Length == 0)
                throw new InvalidOperationException("Array cannot be empty");

            return index < array.Length - 1 ? array[index + 1] : array[0];
        }

        public static int GetMinIndexBy(this Point[] array, bool byX)
        {
            int minIndex = 0;
            double minValue = byX ? array[0].X : array[0].Y;

            for (int i = 1; i < array.Length; i++)
            {
                var target = byX ? array[i].X : array[i].Y;

                if (target < minValue)
                {
                    minIndex = i;
                    minValue = target;
                }
            }

            return minIndex;
        }

        public static int GetMinIndexBy<T, TTarget>(this T[] array, Func<T, TTarget> selector)
            where TTarget : struct, IComparable<TTarget>
        {
            int minIndex = 0;
            TTarget minValue = selector(array[0]);

            for (int i = 1; i < array.Length; i++)
            {
                var target = selector(array[i]);

                if (target.CompareTo(minValue) < 0)
                {
                    minIndex = i;
                    minValue = target;
                }
            }

            return minIndex;
        }

        public static Point GetMinBy(this Point[] array, bool byX)
        {
            return array[array.GetMinIndexBy(byX)];
        }
    }
}
