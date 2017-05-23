// **********************************************************************
// 
//   UserLoginControlViewModel.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using FormsSkiaBikeTracker.Models;
using LRPLib.Mvx.ViewModels;
using SkiaSharp;

namespace FormsSkiaBikeTracker.Shared.ViewModels
{
    public class UserLoginControlViewModel : LrpViewModel
    {
        private User _user;
        public User User
        {
            get { return _user; }
            set
            {
                if (User != value)
                {
                    _user = value;
                    RaisePropertyChanged();

                    if (User != null)
                    {
                        UserPicture = SKBitmap.Decode(new SKFileStream(User.PicturePath));
                    }
                    else
                    {
                        UserPicture = null;
                    }
                }
            }
        }

        private SKBitmap _userPicture;
        public SKBitmap UserPicture
        {
            get { return _userPicture; }
            set
            {
                if (UserPicture != value)
                {
                    _userPicture = value;
                    RaisePropertyChanged();
                }
            }
        }

        public UserLoginControlViewModel()
        {
        }

        public override void Start()
        {
            base.Start();
            
        }
    }
}

