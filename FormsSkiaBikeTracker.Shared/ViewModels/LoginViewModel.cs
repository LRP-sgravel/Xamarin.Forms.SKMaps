// **********************************************************************
// 
//   LoginViewModel.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Services;
using FormsSkiaBikeTracker.Services.Interface;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Realms;
using SimpleCrypto;

namespace FormsSkiaBikeTracker.ViewModels
{
    public class LoginViewModel : MvxViewModel
    {
        [MvxInject]
        public ICryptoService Crypto { get; set; }

        [MvxInject]
        public IUserDialogs AlertInteraction { get; set; }

        [MvxInject]
        public IMvxNavigationService NavigationService { get; set; }

        [MvxInject]
        public IMvxMainThreadAsyncDispatcher MainThread { get; set; }

        [MvxInject]
        public IResourceLocator ResourceLocator { get; set; }

        public LanguageBinder LanguageBinder { get; private set; }

        private IEnumerable<Athlete> _athletes;
        public IEnumerable<Athlete> Athletes
        {
            get => _athletes;
            set
            {
                if (Athletes != value)
                {
                    _athletes = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Athlete _selectedAthlete;
        public Athlete SelectedAthlete
        {
            get => _selectedAthlete;
            set
            {
                if (SelectedAthlete != value)
                {
                    _selectedAthlete = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMvxCommand _loginAthleteCommand;
        public IMvxCommand LoginAthleteCommand => _loginAthleteCommand ?? (_loginAthleteCommand = new MvxCommand<string>(LoginAthlete));

        private IMvxCommand _goToSignupCommand;
        public IMvxCommand GoToSignupCommand => _goToSignupCommand ?? (_goToSignupCommand = new MvxCommand(GoToSignup));

        private IMvxCommand _selectAthleteCommand;
        public IMvxCommand SelectAthleteCommand => _selectAthleteCommand ?? (_selectAthleteCommand = new MvxCommand<Athlete>(SelectAthlete));

        private void SelectAthlete(Athlete pickedAthlete)
        {
            SelectedAthlete = pickedAthlete;
        }

        public override void Start()
        {
            base.Start();

            LanguageBinder  = new LanguageBinder(ResourceLocator.ResourcesNamespace,
                                                 GetType().FullName.Replace(ResourceLocator.ResourcesNamespace + ".", string.Empty),
                                                 false);
        }

        public override Task Initialize()
        {
            MainThread.ExecuteOnMainThreadAsync(() =>
                {
                    Athletes = Realm.GetInstance(RealmConstants.RealmConfiguration)
                                    .All<Athlete>();
                });

            return base.Initialize();
        }

        private void LoginAthlete(string password)
        {
            bool isPasswordValid;

            try
            {
                string hashedPassword = Crypto.Compute(password, SelectedAthlete.PasswordSalt);

                isPasswordValid = Crypto.Compare(SelectedAthlete.PasswordHash, hashedPassword);
            }
            catch (Exception e)
            {
                MvxLog.Instance.Log(MvxLogLevel.Info, () => $"Error logging in user {e.Message}");
                isPasswordValid = false;
            }

            if (isPasswordValid)
            {
                NavigationService.Navigate<ActivityViewModel, string>(SelectedAthlete.Id);
            }
            else
            {
                AlertInteraction.AlertAsync(LanguageBinder.GetText("InvalidPassword"));
            }
        }

        private void GoToSignup()
        {
            NavigationService.Navigate<SignUpViewModel>();
        }
    }
}

