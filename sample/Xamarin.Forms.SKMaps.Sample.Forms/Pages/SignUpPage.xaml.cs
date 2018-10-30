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
using Xamarin.Forms.SKMaps.Sample.Forms.UI.Effects;
using Xamarin.Forms.SKMaps.Sample.Services.Validation;
using MvvmCross.WeakSubscription;

namespace Xamarin.Forms.SKMaps.Sample.Forms.Pages
{
    public partial class SignUpPage
    {
        public IValidationResult CurrentErrors { get; } = new ValidationResult();

        private IDisposable _errorsChangedSubscription;

        public SignUpPage()
        {
            InitializeComponent();

            _errorsChangedSubscription = CurrentErrors.WeakSubscribe(nameof(CurrentErrors.ErrorsChanged),
                                                                     ValidationResultErrorsChanged);
        }

        private void ValidationResultErrorsChanged(object sender, EventArgs e)
        {
            if (CurrentErrors.GetError(nameof(ViewModel.Name)) != null)
            {
                if (NameEntry.Behaviors.Count == 0)
                {
                    NameEntry.Behaviors.Add(new InvalidEntryBehavior());
                }
            }
            else if (NameEntry.Behaviors.Count > 0)
            {
                NameEntry.Behaviors.RemoveAt(0);
            }

            if (CurrentErrors.GetError(nameof(ViewModel.Password)) != null)
            {
                if (PasswordEntry.Behaviors.Count == 0)
                {
                    PasswordEntry.Behaviors.Add(new InvalidEntryBehavior());
                }
            }
            else if (PasswordEntry.Behaviors.Count > 0)
            {
                PasswordEntry.Behaviors.RemoveAt(0);
            }

            if (CurrentErrors.GetError(nameof(ViewModel.PasswordConfirmation)) != null)
            {
                if (ConfirmationEntry.Behaviors.Count == 0)
                {
                    ConfirmationEntry.Behaviors.Add(new InvalidEntryBehavior());
                }
            }
            else if (ConfirmationEntry.Behaviors.Count > 0)
            {
                ConfirmationEntry.Behaviors.RemoveAt(0);
            }
        }
    }
}