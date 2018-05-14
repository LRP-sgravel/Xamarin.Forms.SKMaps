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
using FormsSkiaBikeTracker.ViewModels;
using LRPFramework.Services;
using LRPFramework.Mvx.ViewModels;
using MvvmCross.Commands;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using Realms;
using SimpleCrypto;

namespace FormsSkiaBikeTracker.Shared.ViewModels
{
    public class LoginViewModel : LRPViewModel
    {
        [MvxInject]
        public ICryptoService Crypto { get; set; }

        [MvxInject]
        public IUserDialogs AlertInteraction { get; set; }

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
        public IMvxCommand LoginAthleteCommand
        {
            get
            {
                if (_loginAthleteCommand == null)
                {
                    _loginAthleteCommand = new MvxCommand<string>(LoginAthlete);
                }

                return _loginAthleteCommand;
            }
        }

        private IMvxCommand _goToSignupCommand;
        public IMvxCommand GoToSignupCommand
        {
            get
            {
                if (_goToSignupCommand == null)
                {
                    _goToSignupCommand = new MvxCommand(GoToSignup);
                }

                return _goToSignupCommand;
            }
        }

        private IMvxCommand _selectAthleteCommand;
        public IMvxCommand SelectAthleteCommand
        {
            get
            {
                if (_selectAthleteCommand == null)
                {
                    _selectAthleteCommand = new MvxCommand<Athlete>(SelectAthlete);
                }

                return _selectAthleteCommand;
            }
        }

        private void SelectAthlete(Athlete pickedAthlete)
        {
            SelectedAthlete = pickedAthlete;
        }

        public override Task Initialize()
        {
            MainThread.Run(() =>
            {
                Athletes = Realm.GetInstance()
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
                NavigationService.Navigate<MainViewModel, string>(SelectedAthlete.Id);
            }
            else
            {
                AlertInteraction.AlertAsync(LanguageBinder.GetText("InvalidPassword"));
            }
        }

        private void GoToSignup()
        {
            NavigationService.Navigate<SignUpViewModel, bool>(true);
        }
    }
}

