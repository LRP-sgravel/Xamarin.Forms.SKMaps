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

using System.Collections.Generic;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.ViewModels;
using LRPLib.Mvx.ViewModels;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.IoC;
using SimpleCrypto;

namespace FormsSkiaBikeTracker.Shared.ViewModels
{
    public class LoginViewModel : LrpViewModel
    {
        [MvxInject]
        public ICryptoService Crypto { get; set; }

        private IEnumerable<User> _users;
        public IEnumerable<User> Users
        {
            get { return _users; }
            set
            {
                if (Users != value)
                {
                    _users = value;
                    RaisePropertyChanged();
                }
            }
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (SelectedUser != value)
                {
                    _selectedUser = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMvxCommand _loginUserCommand;
        public IMvxCommand LoginUserCommand
        {
            get
            {
                if (_loginUserCommand == null)
                {
                    _loginUserCommand = new MvxCommand<string>(LoginUser);
                }

                return _loginUserCommand;
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

        public LoginViewModel()
        {
        }

        public override void Start()
        {
            base.Start();

            Users = new List<User>
                    {
                        new User { Name = "Sylvain Gravel" },
                        new User { Name = "Marco Vega" },
                        new User { Name = "Ivan Čuljak" },
                    };
        }
        
        private void LoginUser(string password)
        {
            string hashedPassword = Crypto.Compute(password, SelectedUser.PasswordSalt);
            bool isPasswordValid = Crypto.Compare(SelectedUser.PasswordHash, hashedPassword);

            if (isPasswordValid)
            {
                ShowViewModel<MainViewModel>();
            }
            else
            {
//                UserDialogs
            }
        }

        private void GoToSignup()
        {
            ShowViewModel<SignUpViewModel>();
        }
    }
}

