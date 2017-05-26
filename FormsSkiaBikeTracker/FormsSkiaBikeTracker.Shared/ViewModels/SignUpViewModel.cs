// **********************************************************************
// 
//   SignUpPage.xaml.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.IO;
using FormsSkiaBikeTracker.Services.Validation;
using LRPLib.Mvx.ViewModels;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.IoC;
using MvvmCross.Plugins.File;
using MvvmCross.Plugins.PictureChooser;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace FormsSkiaBikeTracker.Shared.ViewModels
{
    public class SignUpViewModel : LrpViewModel
    {
        [MvxInject]
        public IMvxPictureChooserTask PictureChooser { get; set; }

        [MvxInject]
        public IMvxFileStore FileStore { get; set; }

        private SKBitmapImageSource _pictureBitmap;
        public SKBitmapImageSource PictureBitmap
        {
            get { return _pictureBitmap; }
            set
            {
                if (PictureBitmap != value)
                {
                    _pictureBitmap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (Name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (Password != value)
                {
                    _password = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _passwordConfirmation;
        public string PasswordConfirmation
        {
            get { return _passwordConfirmation; }
            set
            {
                if (PasswordConfirmation != value)
                {
                    _passwordConfirmation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMvxCommand _selectUserPictureCommand;
        public IMvxCommand SelectUserPictureCommand
        {
            get
            {
                if (_selectUserPictureCommand == null)
                {
                    _selectUserPictureCommand = new MvxCommand(SelectUserPicture);
                }

                return _selectUserPictureCommand;
            }
        }

        private IMvxCommand _registerUserCommand;
        public IMvxCommand RegisterUserCommand
        {
            get
            {
                if (_registerUserCommand == null)
                {
                    _registerUserCommand = new MvxCommand<IValidationResult>(RegisterUser);
                }

                return _registerUserCommand;
            }
        }

        public SignUpViewModel()
        {
        }

        public override void Start()
        {
            base.Start();
        }

        private void SelectUserPicture()
        {
            try
            {
                PictureChooser.TakePicture(256, 95, PicturePicked, PicturePickCancelled);
            }
            catch (Exception e)
            {
                PictureChooser.ChoosePictureFromLibrary(256, 95, PicturePicked, PicturePickCancelled);
            }
        }

        private void RegisterUser(IValidationResult validationResult)
        {
            if (string.IsNullOrEmpty(Name))
            {
                validationResult.AddError(nameof(Name), LanguageBinder.GetText("FieldRequired"));
            }
            else
            {
                validationResult.ClearError(nameof(Name));
            }

            if (string.IsNullOrEmpty(Password))
            {
                validationResult.AddError(nameof(Password), LanguageBinder.GetText("FieldRequired"));
            }
            else
            {
                validationResult.ClearError(nameof(Password));
            }

            if (Password != PasswordConfirmation)
            {
                validationResult.AddError(nameof(PasswordConfirmation), LanguageBinder.GetText("FieldsDoNotMatch"));
            }
            else
            {
                validationResult.ClearError(nameof(PasswordConfirmation));
            }

            if (!validationResult.HasErrors)
            {
                int i = 0;
            }
        }

        private void PicturePicked(Stream pictureStream)
        {
            if (pictureStream != null)
            {
                string userPictureSavePath = FileStore.NativePath("../Library/Caches/UserPictures");
                string userPictureFilePath = $"{userPictureSavePath}/SignupPicture.tmp";

                FileStore.EnsureFolderExists(userPictureSavePath);

                FileStore.WriteFile(userPictureFilePath,
                                    fileStream =>
                                    {
                                        const int BufferSize = 10240;
                                        byte[] buffer = new byte[BufferSize];
                                        int readCount = 0;

                                        do
                                        {
                                            readCount = pictureStream.Read(buffer, 0, BufferSize);

                                            if (readCount > 0)
                                            {
                                                fileStream.Write(buffer, 0, readCount);
                                            }
                                        }
                                        while (readCount > 0);
                                    });
                PictureBitmap = new SKBitmapImageSource();
                PictureBitmap.Bitmap = SKBitmap.Decode(new SKFileStream(userPictureFilePath));
            }
        }

        private void PicturePickCancelled()
        {
        }
    }
}

