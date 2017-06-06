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
using System.IO;
using System.Reflection;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Services.Interface;
using LRPLib.Mvx.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;
using MvvmCross.Plugins.File;
using SkiaSharp;

namespace FormsSkiaBikeTracker.Shared.ViewModels
{
    public class AthleteLoginControlViewModel : LrpViewModel
    {
        [MvxInject]
        public IMvxFileStore FileStore { get; set; }
        
        [MvxInject]
        public IDocumentRoot DocumentRoot { get; set; }

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
                            string picPath = FileStore.NativePath($"{DocumentRoot.Path}/{Athlete.PicturePath}");
                            Stream fileStream = FileStore.OpenRead(picPath);

                            using (fileStream)
                            {
                                AthletePicture = SKBitmap.Decode(fileStream);
                            }
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

