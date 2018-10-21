// **********************************************************************
// 
//   AthleteLoginControlViewModel.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System;
using System.IO;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Services.Interface;
using MvvmCross.IoC;
using MvvmCross.Plugin.File;
using MvvmCross.ViewModels;
using SkiaSharp;

namespace FormsSkiaBikeTracker.ViewModels
{
    public class AthleteLoginControlViewModel : MvxViewModel
    {
        public Athlete Athlete
        {
            get => _athlete;
            set
            {
                if (Athlete != value)
                {
                    _athlete = value;
                    RaisePropertyChanged();

                    if (Athlete != null)
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
                    else
                        AthletePicture = null;
                }
            }
        }

        public SKBitmap AthletePicture
        {
            get => _athletePicture;
            set
            {
                if (AthletePicture != value)
                {
                    _athletePicture = value;
                    RaisePropertyChanged();
                }
            }
        }

        [MvxInject]
        public IDocumentRoot DocumentRoot { get; set; }

        public string EnteredPassword
        {
            get => _enteredPassword;
            set
            {
                if (EnteredPassword != value)
                {
                    _enteredPassword = value;
                    RaisePropertyChanged();
                }
            }
        }

        [MvxInject]
        public IMvxFileStore FileStore { get; set; }

        private Athlete _athlete;

        private SKBitmap _athletePicture;

        private string _enteredPassword;

        public override void Start()
        {
            base.Start();
        }
    }
}
