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

using System.Diagnostics;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Shared.Helpers;
using FormsSkiaBikeTracker.Shared.Models.Maps;
using LRPLib.Mvx.ViewModels;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Platform;
using Realms;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

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

        private IMvxCommand _testCoordsCommand;
        public IMvxCommand TestCoordsCommand
        {
            get
            {
                if (_testCoordsCommand == null)
                {
                    _testCoordsCommand = new MvxCommand(TestCoords);
                }

                return _testCoordsCommand;
            }
        }

        private void TestCoords()
        {
            Rectangle largeTile = new Rectangle(0, 0, 256, 256);
            Rectangle smallTile = new Rectangle(0, 0, 128, 128);
            SKMapSpan largeSpan = largeTile.ToGps();
            SKMapSpan smallSpan = smallTile.ToGps();

            Debug.WriteLine($"Large bounds for tile {largeTile}\n" +
                            $"({largeSpan.Center.Longitude + largeSpan.LongitudeDegrees}, {largeSpan.Center.Latitude - largeSpan.LatitudeDegrees}; \n" +
                            $"{largeSpan.Center.Longitude - largeSpan.LongitudeDegrees}, {largeSpan.Center.Latitude + largeSpan.LatitudeDegrees})");
            Debug.WriteLine($"Small bounds for tile {smallTile}\n" +
                            $"({smallSpan.Center.Longitude + smallSpan.LongitudeDegrees}, {smallSpan.Center.Latitude - smallSpan.LatitudeDegrees}; \n" +
                            $"{smallSpan.Center.Longitude - smallSpan.LongitudeDegrees}, {smallSpan.Center.Latitude + smallSpan.LatitudeDegrees})");
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

