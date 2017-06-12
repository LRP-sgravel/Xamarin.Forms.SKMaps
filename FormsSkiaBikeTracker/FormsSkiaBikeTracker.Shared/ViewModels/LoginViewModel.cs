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
using Bulboss.MvvmCross.Plugins.UserInteraction;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.ViewModels;
using LRPLib.Mvx.ViewModels;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.IoC;
using MvvmCross.Platform.Platform;
using Realms;
using SimpleCrypto;

namespace FormsSkiaBikeTracker.Shared.ViewModels
{
    public class LoginViewModel : LrpViewModel
    {
        [MvxInject]
        public ICryptoService Crypto { get; set; }

        [MvxInject]
        public IAlertUserInteraction AlertInteraction { get; set; }

        private IEnumerable<Athlete> _athletes;
        public IEnumerable<Athlete> Athletes
        {
            get { return _athletes; }
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
            get { return _selectedAthlete; }
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

        public LoginViewModel()
        {
        }

        public override void Start()
        {
            base.Start();

            Athletes = Realm.GetInstance().All<Athlete>();
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
                MvxTrace.Trace(MvxTraceLevel.Diagnostic, $"Error logging in user {e.Message}");
                isPasswordValid = false;
            }

            if (isPasswordValid)
            {
                MvxBundle presentationBundle = new MvxBundle(new Dictionary<string, string>
                                                             {
                                                                 [PresenterConstants.SetAsNavigationRoot] =
                                                                 true.ToString(),
                                                             });

                ShowViewModel<MainViewModel>(new { athleteId = SelectedAthlete.Id }, presentationBundle);
            }
            else
            {
                AlertInteraction.AlertAsync(LanguageBinder.GetText("InvalidPassword"));
            }
        }

        private void GoToSignup()
        {
            ShowViewModel<SignUpViewModel>(new { signInOnCompletion = true });
        }
    }
}

