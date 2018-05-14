// **********************************************************************
// 
//   MatrixBuilderExtension.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using System;
using MathNet.Numerics.LinearAlgebra;

namespace Xamarin.Forms.Maps.Overlays.Skia
{
    internal static class MatrixBuilderExtension
    {
        public static Matrix<double> RotationDegrees(this MatrixBuilder<double> self, double degrees)
        {
            return self.RotationRadians(degrees.ToRadians());
        }

        public static Matrix<double> RotationDegreesAround(this MatrixBuilder<double> self, double degrees, double x, double y)
        {
            return self.RotationRadiansAround(degrees.ToRadians(), x, y);
        }

        public static Matrix<double> RotationRadians(this MatrixBuilder<double> builder, double radians)
        {
            Matrix<double> result = builder.DenseIdentity(3);
            
            result[0, 0] = Math.Cos(radians);
            result[0, 1] = -Math.Sin(radians);
            result[1, 0] = Math.Sin(radians);
            result[1, 1] = Math.Cos(radians);
            result[2, 2] = 1;

            return result;
        }

        public static Matrix<double> RotationRadiansAround(this MatrixBuilder<double> builder, double radians, double x, double y)
        {
            Matrix<double> result = builder.Translation(-x, -y);

            result *= builder.RotationRadians(radians);
            result *= builder.Translation(x, y);

            return result;
        }

        public static Matrix<double> Translation(this MatrixBuilder<double> builder, double x, double y)
        {
            Matrix<double> result = builder.DenseIdentity(3);

            result[0, 2] = x;
            result[1, 2] = y;

            return result;
        }


        public static Matrix<double> Scale(this MatrixBuilder<double> builder, double scaleX, double scaleY)
        {
            Matrix<double> result = builder.DenseIdentity(3);

            result[0, 0] = scaleX;
            result[1, 1] = scaleY;

            return result;
        }

        public static Matrix<double> Scale(this MatrixBuilder<double> builder, double scale)
        {
            return builder.Scale(scale, scale);
        }

        public static Matrix<double> ScaleAround(this MatrixBuilder<double> builder, double scaleX, double scaleY, double offsetX, double offsetY)
        {
            Matrix<double> result = builder.Translation(-offsetX, -offsetY);

            result *= builder.Scale(scaleX, scaleY);
            result *= builder.Translation(offsetX, offsetY);

            return result;
        }

        public static Matrix<double> Skew (this MatrixBuilder<double> builder, double skewX, double skewY)
        {
            Matrix<double> result = builder.DenseIdentity(3);

            result[0, 1] = skewX;
            result[1, 0] = skewY;

            return result;
        }

        private static double ToRadians(this double degrees)
        {
            const double ConversionFactor = Math.PI / 180.0;

            return degrees * ConversionFactor;
        }
    }
}
