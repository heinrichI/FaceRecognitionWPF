using FaceRecognitionBusinessLogic.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace FaceRecognitionWPF.Menu
{
    public class ImageContextMenuItemSourceBindingOnOpenBehavior : Behavior<System.Windows.Controls.Image>
    {
        // Using a DependencyProperty as the backing store for theFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MenuGeneratorProperty =
            DependencyProperty.Register("MenuGenerator",
            typeof(Func<FaceInfo, BindableMenuItem[]>),
            typeof(ImageContextMenuItemSourceBindingOnOpenBehavior),
            new UIPropertyMetadata(null));

        public Func<FaceInfo, BindableMenuItem[]> MenuGenerator
        {
            get { return (Func<FaceInfo, BindableMenuItem[]>)GetValue(MenuGeneratorProperty); }
            set { SetValue(MenuGeneratorProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.ContextMenuOpening += AssociatedObject_ContextMenuOpening;
        }

        void AssociatedObject_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var image = sender as System.Windows.Controls.Image;
            if (image != null)
            {
                if (image.DataContext is FaceInfo)
                {
                    if (MenuGenerator != null)
                        image.ContextMenu.ItemsSource = MenuGenerator(image.DataContext as FaceInfo);
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.ContextMenuOpening -= AssociatedObject_ContextMenuOpening;
        }
    }
}
