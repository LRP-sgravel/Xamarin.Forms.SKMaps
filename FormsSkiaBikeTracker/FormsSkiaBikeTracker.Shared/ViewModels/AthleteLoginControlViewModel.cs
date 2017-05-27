// **********************************************************************
// 
//   AthleteLoginControlViewModel.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.Reflection;
using FormsSkiaBikeTracker.Models;
using LRPLib.Mvx.ViewModels;
using MvvmCross.Core.Platform;
using MvvmCross.Platform;
using SkiaSharp;

namespace FormsSkiaBikeTracker.Shared.ViewModels
{
    public class AthleteLoginControlViewModel : LrpViewModel
    {
        private Athlete _athlete;
        public Athlete Athlete
        {
            get { return _athlete; }
            set
            {
                if (Athlete != value)
                {
                    _athlete = value;
                    RaisePropertyChanged();

                    if (Athlete != null)
                    {
                        try
                        {
                            PropertyInfo pi = typeof(Athlete).GetProperty(nameof(Athlete.PicturePath));
                            string propPath = Athlete.PicturePath;
                            string propReflectPath = pi.GetValue(Athlete) as string;

                            AthletePicture = SKBitmap.Decode(new SKFileStream(Athlete.PicturePath));
                        }
                        catch (Exception)
                        {
                            AthletePicture = null;
                        }
                    }
                    else
                    {
                        AthletePicture = null;
                    }
                }
            }
        }

        private SKBitmap _athletePicture;
        public SKBitmap AthletePicture
        {
            get { return _athletePicture; }
            set
            {
                if (AthletePicture != value)
                {
                    _athletePicture = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _enteredPassword;
        public string EnteredPassword
        {
            get { return _enteredPassword; }
            set
            {
                if (EnteredPassword != value)
                {
                    _enteredPassword = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AthleteLoginControlViewModel() : base("ViewModels." + nameof(LoginViewModel))
        {
        }

        public override void Start()
        {
            base.Start();
        }
    }
}

