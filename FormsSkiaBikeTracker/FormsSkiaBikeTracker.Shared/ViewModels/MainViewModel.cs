// **********************************************************************
// 
//   MainViewModel.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using FormsSkiaBikeTracker.Models;
using LRPLib.Mvx.ViewModels;
using Realms;

namespace FormsSkiaBikeTracker.ViewModels
{
    public class MainViewModel : LrpViewModel
    {
        private Athlete _athlete;
        public Athlete Athlete
        {
            get { return _athlete; }
            set
            {
                if (Athlete != value)
                {
                    _athlete = value;
                    RaisePropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
        }

        public void Init(string athleteId)
        {
            Athlete = Realm.GetInstance()
                           .Find<Athlete>(athleteId);
        }

        public override void Start()
        {
            base.Start();
        }
    }
}

