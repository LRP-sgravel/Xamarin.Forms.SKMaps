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
using Xamarin.Forms.SKMaps.Sample.Models;
using Xamarin.Forms.SKMaps.Sample.Services;
using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using Xamarin.Forms.SKMaps.Sample.Services.Validation;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.File;
using MvvmCross.Plugin.PictureChooser;
using MvvmCross.ViewModels;
using Realms;
using SimpleCrypto;
using SkiaSharp;
using Xamarin.Forms.SKMaps.Models;

namespace Xamarin.Forms.SKMaps.Sample.ViewModels
{
    public class SignUpViewModel : MvxViewModel
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

        [MvxInject]
        public IMvxNavigationService NavigationService { get; set; }

        [MvxInject]
        public IMvxMainThreadAsyncDispatcher MainThread { get; set; }

        [MvxInject]
        public IResourceLocator ResourceLocator { get; set; }

        public LanguageBinder LanguageBinder { get; private set; }

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
        public IMvxCommand SelectAthletePictureCommand => _selectAthletePictureCommand ?? (_selectAthletePictureCommand = new MvxAsyncCommand(SelectAthletePicture));

        private IMvxCommand _registerAthleteCommand;
        public IMvxCommand RegisterAthleteCommand => _registerAthleteCommand ?? (_registerAthleteCommand = new MvxCommand<IValidationResult>(RegisterAthlete));

        private string PictureFilePath(string fileName) => FileStore.NativePath($"{DocumentRoot.Path}/{fileName}");

        public override void Start()
        {
            base.Start();

            LanguageBinder = new LanguageBinder(ResourceLocator.ResourcesNamespace,
                                                GetType().FullName.Replace(ResourceLocator.ResourcesNamespace + ".", string.Empty),
                                                false);

            FileStore.EnsureFolderExists(PictureFilePath(string.Empty));
        }
        
        private Task SelectAthletePicture()
        {
            return MainThread.ExecuteOnMainThreadAsync(async () =>
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

                NavigationService.Navigate<ActivityViewModel, string>(athleteId);
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

