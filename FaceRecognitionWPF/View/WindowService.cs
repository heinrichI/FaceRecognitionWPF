using FaceRecognitionWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FaceRecognitionWPF.View
{
    class WindowService 
    {
        private Window _activeWindow;

        public WindowService(Window activeWindow)
        {
            _activeWindow = activeWindow;
        }

        //public Window ActiveWindow
        //{
        //    get
        //    {
        //        return Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
        //    }
        //}

        public bool? ShowDialogWindow<T>(IClosingViewModel dataContext) where T : Window, new()
        {
            bool? result = null;

            Window activeWindow = _activeWindow;
            if (activeWindow == null)
                throw new NullReferenceException("ActiveWindow");

            var view = new T();
            view.Owner = activeWindow;
            view.DataContext = dataContext;
            view.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            view.Closing += new System.ComponentModel.CancelEventHandler(dataContext.OnClosing);
            dataContext.ClosingRequest += (sender, e) =>
            {
                view.DialogResult = e.DialogResult;
                view.Close();
            };
            ApplyEffect(view.Owner);
            result = view.ShowDialog();
            ClearEffect(view.Owner);
            return result;
        }

        /// <summary>
        /// Apply Blur Effect on the window
        /// </summary>
        /// <param name=”win”></param>
        private void ApplyEffect(Window win)
        {
            System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
            objBlur.Radius = 4;
            win.Effect = objBlur;
            //_beforeBackground = win.Background;
            //win.Background = System.Windows.Media.Brushes.Black;
            win.Opacity = 0.7;
        }

        /// <summary>
        /// Remove Blur Effects
        /// </summary>
        /// <param name=”win”></param>
        private void ClearEffect(Window win)
        {
            win.Effect = null;
            //win.Background = _beforeBackground;
            win.Opacity = 1.0;
        }
    }
}
