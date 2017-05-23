﻿using LRPLib.Views.XForms;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Controls
{
    public partial class DrawnBackgroundControlView
    {
        public static readonly BindableProperty BackgroundProperty = BindableProperty.Create(nameof(Background),
                                                                                             typeof(DrawableView),
                                                                                             typeof(DrawnBackgroundControlView),
                                                                                             null,
                                                                                             BindingMode.OneWay,
                                                                                             null,
                                                                                             BackgroundPropertyChanged);

        public static readonly BindableProperty ForegroundProperty = BindableProperty.Create(nameof(Foreground),
                                                                                             typeof(View),
                                                                                             typeof(DrawnBackgroundControlView),
                                                                                             null,
                                                                                             BindingMode.OneWay,
                                                                                             null,
                                                                                             ForegroundPropertyChanged);

        public View Foreground
        {
            get { return (View)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }


        public DrawableView Background
        {
            get { return (DrawableView)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public DrawnBackgroundControlView()
        {
            InitializeComponent();

            RefreshLayout();
        }

        private static void BackgroundPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            DrawnBackgroundControlView view = bindable as DrawnBackgroundControlView;

            view.RefreshLayout();
        }

        private static void ForegroundPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            DrawnBackgroundControlView view = bindable as DrawnBackgroundControlView;

            view.RefreshLayout();
        }

        private void RefreshLayout()
        {
            SetFullSize(Background);
            SetFullSize(Foreground);
            
            Children.Clear();

            if (Background != null)
            {
                Children.Add(Background);
            }

            if (Foreground != null)
            {
                Foreground.BackgroundColor = Color.Transparent;
                Children.Add(Foreground);
            }
        }

        private void SetFullSize(Element uiElement)
        {
            if (uiElement != null)
            {
                uiElement.SetValue(LayoutBoundsProperty, new Rectangle(0, 0, 1, 1));
                uiElement.SetValue(LayoutFlagsProperty, AbsoluteLayoutFlags.All);
            }
        }
    }
}
