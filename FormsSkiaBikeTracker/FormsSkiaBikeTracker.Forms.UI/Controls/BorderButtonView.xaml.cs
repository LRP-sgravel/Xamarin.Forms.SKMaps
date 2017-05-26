// **********************************************************************
// 
//   BorderButtonView.xaml.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.Windows.Input;
using LRPLib.Views.XForms;
using MvvmCross.Platform.WeakSubscription;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Controls
{
    public partial class BorderButtonView
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(BorderButtonView), string.Empty, BindingMode.OneWay, null, TextPropertyChanged);
        public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(int), typeof(BorderButtonView), 2, BindingMode.OneWay, null, BorderWidthPropertyChanged);
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(BorderButtonView), null, BindingMode.OneWay, null, CommandPropertyChanged);
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(BorderButtonView), null, BindingMode.OneWay, null, CommandParameterPropertyChanged);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public int BorderWidth
        {
            get { return (int)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        private MvxWeakEventSubscription<LinearGradientBoxView> _backgroundPropertyChangedSubscription;

        public BorderButtonView()
        {
            InitializeComponent();

            _backgroundPropertyChangedSubscription = Background.WeakSubscribe(nameof(Background.SizeChanged), SignUpBackgroundSizeChanged);
        }
        
        private static void TextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            BorderButtonView view = bindable as BorderButtonView;

            view.Button.Text = view.Text;
        }

        private static void BorderWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            BorderButtonView view = bindable as BorderButtonView;

            view.UpdateBorderMask();
        }

        private static void CommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            BorderButtonView view = bindable as BorderButtonView;

            view.Button.Command = view.Command;
        }
        
        private static void CommandParameterPropertyChanged(BindableObject bindable, object oldvalue, object newValue)
        {
            BorderButtonView view = bindable as BorderButtonView;

            view.Button.CommandParameter = newValue;
        }
        
        private void SignUpBackgroundSizeChanged(object sender, EventArgs args)
        {
            UpdateBorderMask();
        }

        private void UpdateBorderMask()
        {
            SKRect buttonRect = Background.Bounds.ToSKRect();
            SKPath clipPath = new SKPath();
            float cornerSize = buttonRect.Height * 0.5f;

            clipPath.AddRoundedRect(buttonRect, cornerSize, cornerSize);
            buttonRect.Inflate(new SKSize(-BorderWidth, -BorderWidth));
            cornerSize = buttonRect.Height * 0.5f;
            clipPath.AddRoundedRect(buttonRect, cornerSize, cornerSize, SKPathDirection.CounterClockwise);

            Background.ClippingPath = clipPath;
        }
    }
}
