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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Services.Interface;
using FormsSkiaBikeTracker.Services.Validation;
using FormsSkiaBikeTracker.ViewModels;
using LRPFramework.Mvx.ViewModels;
using LRPFramework.Services;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Plugin.File;
using MvvmCross.Plugin.PictureChooser;
using Realms;
using SimpleCrypto;
using SkiaSharp;

namespace FormsSkiaBikeTracker.Shared.ViewModels
{
    public class SignUpViewModel : LRPViewModel<bool>
    {
        private const string PictureSavePath = "AthletePictures";
        private const string TempPicturePath = "Caches";
        private const string TempPictureFileName = TempPicturePath + "/SignupPicture.tmp";
        private string PictureFilePath(string fileName) => FileStore.NativePath($"{DocumentRoot.Path}/{fileName}");

        [MvxInject]
        public ICryptoService Crypto { get; set; }

        [MvxInject]
        public IMvxPictureChooserTask PictureChooser { get; set; }

        [MvxInject]
        public IMvxFileStore FileStore { get; set; }

        [MvxInject]
        public IDocumentRoot DocumentRoot { get; set; }

        private SKBitmap _pictureBitmap;
        public SKBitmap PictureBitmap
        {
            get => _pictureBitmap;
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
            get => _name;
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
            get => _password;
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
            get => _passwordConfirmation;
            set
            {
                if (PasswordConfirmation != value)
                {
                    _passwordConfirmation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMvxCommand _selectAthletePictureCommand;
        public IMvxCommand SelectAthletePictureCommand
        {
            get
            {
                if (_selectAthletePictureCommand == null)
                {
                    _selectAthletePictureCommand = new MvxAsyncCommand(SelectAthletePicture);
                }

                return _selectAthletePictureCommand;
            }
        }

        private IMvxCommand _registerAthleteCommand;
        public IMvxCommand RegisterAthleteCommand
        {
            get
            {
                if (_registerAthleteCommand == null)
                {
                    _registerAthleteCommand = new MvxCommand<IValidationResult>(RegisterAthlete);
                }

                return _registerAthleteCommand;
            }
        }
        
        private bool _SignInOnCompletion { get; set; }

        public override void Prepare(bool signInOnCompletion)
        {
            base.Prepare(signInOnCompletion);

            _SignInOnCompletion = signInOnCompletion;
        }

        public override Task Initialize()
        {
            FileStore.EnsureFolderExists(PictureFilePath(string.Empty));

            return base.Initialize();
        }

        private Task SelectAthletePicture()
        {
            return MainThread.RunAsync(async () =>
                                       {
                                           Stream pictureStream;

                                           try
                                           {
                                               pictureStream = await PictureChooser.TakePicture(256, 95);
                                           }
                                           catch (Exception)
                                           {
                                               pictureStream = await PictureChooser.ChoosePictureFromLibrary(256, 95);
                                           }

                                           if (pictureStream != null)
                                           {
                                               PicturePicked(pictureStream);
                                           }
                                           else
                                           {
                                               PicturePickCancelled();
                                           }
                                       });
        }

        private void RegisterAthlete(IValidationResult validationResult)
        {
            ValidateForm(validationResult);

            if (!validationResult.HasErrors)
            {
                Realm realmInstance = Realm.GetInstance();
                string athleteId = Guid.NewGuid()
                                       .ToString();
                string salt = Crypto.GenerateSalt();
                string athletePictureRelativePath = SaveUserPicture(athleteId);

                realmInstance.Write(() =>
                {
                    try
                    {
                        Athlete newAthlete;
                        newAthlete = new Athlete
                        {
                            Id = athleteId,
                            Name = Name,
                            PicturePath = athletePictureRelativePath,
                            PasswordSalt = salt,
                            PasswordHash = Crypto.Compute(Password, salt)
                        };

                        realmInstance.Add(newAthlete);
                    }
                    catch (Exception e)
                    {
                        MvxLog.Instance.Log(MvxLogLevel.Warn, () => e.Message);
                    }
                });

                if (_SignInOnCompletion)
                {
                    NavigationService.Navigate<MainViewModel, string>(athleteId);
                }
                else
                {
                    NavigationService.Close(this);
                }
            }
        }

        private string SaveUserPicture(string athleteId)
        {
            string sourcePath = PictureFilePath(TempPictureFileName);
            string athletePictureRelativePath = $"{PictureSavePath}/{athleteId}";
            string picturePath = PictureFilePath(athletePictureRelativePath);

            FileStore.EnsureFolderExists(PictureFilePath(PictureSavePath));

            if (!FileStore.TryMove(sourcePath, picturePath, true))
            {
                picturePath = null;
            }

            return athletePictureRelativePath;
        }

        private void ValidateForm(IValidationResult validationResult)
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
        }

        private void PicturePicked(Stream pictureStream)
        {
            if (pictureStream != null)
            {
                string destinationFile = PictureFilePath(TempPictureFileName);
               
                FileStore.EnsureFolderExists(PictureFilePath(TempPicturePath));

                FileStore.WriteFile(destinationFile,
                                    fileStream =>
                                    {
                                        const int BufferSize = 1024 * 10;
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
                PictureBitmap = SKBitmap.Decode(new SKFileStream(destinationFile));
            }
        }

        private void PicturePickCancelled()
        {
        }
    }
}

