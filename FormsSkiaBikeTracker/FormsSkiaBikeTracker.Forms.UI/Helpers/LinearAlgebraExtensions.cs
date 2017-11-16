// **********************************************************************
// 
//   LinearAlgebraExtensions.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using MathNet.Numerics.LinearAlgebra;
using SkiaSharp;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Helpers
{
    public static class LinearAlgebraExtensions
    {
        public static Vector<double> ToVector(this Point point)
        {
            Vector<double> result = Vector<double>.Build.Dense(3);

            result[0] = point.X;
            result[1] = point.Y;
            result[2] = 1;

            return result;
        }

        public static Matrix<double> ToMatrix(this Rectangle point)
        {
            Matrix<double> result = Matrix<double>.Build.Dense(3, 2);

            result[0, 0] = point.Left;
            result[0, 1] = point.Right;
            result[1, 0] = point.Bottom;
            result[1, 1] = point.Top;
            result[2, 0] = 1;
            result[2, 1] = 1;

            return result;
        }

        public static Point ToPoint(this Vector<double> pointVector)
        {
            return new Point(pointVector[0], pointVector[1]);
        }

        public static SKPoint ToSKPoint(this Vector<double> pointVector)
        {
            return new SKPoint((float)pointVector[0], (float)pointVector[1]);
        }

        public static Rectangle ToRectangle(this Matrix<double> rectMatrix)
        {
            return new Rectangle(rectMatrix[0, 0],
                                 rectMatrix[1, 0],
                                 rectMatrix[0, 1] - rectMatrix[0, 0],
                                 rectMatrix[1, 1] - rectMatrix[1, 0]);
        }

        public static SKRect ToSKRect(this Matrix<double> rectMatrix)
        {
            return new SKRect((float)rectMatrix[0, 0],
                              (float)rectMatrix[1, 1],
                              (float)rectMatrix[0, 1],
                              (float)rectMatrix[1, 0]);
        }
    }
}
