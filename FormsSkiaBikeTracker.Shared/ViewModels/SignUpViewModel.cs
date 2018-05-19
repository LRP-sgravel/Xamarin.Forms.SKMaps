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
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Services.Interface;
using FormsSkiaBikeTracker.Services.Validation;
using FormsSkiaBikeTracker.ViewModels;
using LRPFramework.Mvx.ViewModels;
using LRPFramework.Services.Threading;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Plugin.File;
using MvvmCross.Plugin.PictureChooser;
using Realms;
using SimpleCrypto;
using SkiaSharp;
using Xamarin.Forms.Maps.Overlays.Models;

namespace FormsSkiaBikeTracker.ViewModels
{
    public class SignUpViewModel : LRPViewModel<bool>
    {
        private const string PictureSavePath = "AthletePictures";
        private const string TempPicturePath = "Caches";
        private const string TempPictureFileName = TempPicturePath + "/SignupPicture.tmp";

        [MvxInject]
        public ICryptoService Crypto { get; set; }

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
        
        private bool _SignInOnCompletion { get; set; }

        private IMvxCommand _selectAthletePictureCommand;
        public IMvxCommand SelectAthletePictureCommand => _selectAthletePictureCommand ?? (_selectAthletePictureCommand = new MvxAsyncCommand(SelectAthletePicture));

        private IMvxCommand _registerAthleteCommand;
        public IMvxCommand RegisterAthleteCommand => _registerAthleteCommand ?? (_registerAthleteCommand = new MvxCommand<IValidationResult>(RegisterAthlete));

        private string PictureFilePath(string fileName) => FileStore.NativePath($"{DocumentRoot.Path}/{fileName}");

        public override void Start()
        {
            base.Start();

            FileStore.EnsureFolderExists(PictureFilePath(string.Empty));
        }

        public override void Prepare(bool signInOnCompletion)
        {
            base.Prepare(signInOnCompletion);

            _SignInOnCompletion = signInOnCompletion;
        }

        private Task SelectAthletePicture()
        {
            return MainThread.RunAsync(async () =>
                                       {
                                           Stream pictureStream;
                                           IMvxPictureChooserTask pictureChooser = Mvx.Resolve<IMvxPictureChooserTask>();

                                           try
                                           {
                                               pictureStream = await pictureChooser.TakePicture(256, 95);
                                           }
                                           catch (Exception)
                                           {
                                               pictureStream = await pictureChooser.ChoosePictureFromLibrary(256, 95);
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
                Realm realmInstance = Realm.GetInstance(RealmConstants.RealmConfiguration);
                string athleteId = Guid.NewGuid().ToString();
                string salt = Crypto.GenerateSalt();
                string athletePictureRelativePath = SaveUserPicture(athleteId);

                realmInstance.Write(() =>
                {
                    try
                    {
                        Athlete newAthlete;
                        newAthlete = new Athlete
                        {
                            Name = Name,
                            Id = athleteId,
                            PicturePath = athletePictureRelativePath,
                            PasswordSalt = salt,
                            PasswordHash = Crypto.Compute(Password, salt),
                            DistanceUnit = RegionInfo.CurrentRegion.IsMetric ? DistanceUnit.Kilometer : DistanceUnit.Miles
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
                    NavigationService.Navigate<ActivityViewModel, string>(athleteId);
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

