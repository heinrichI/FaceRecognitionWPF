using FaceRecognitionBusinessLogic.DataBase;
using FaceRecognitionDataBase;
using FaceRecognitionWPF.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FaceRecognitionWPF
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //Startup += App_Startup;
        }

        //private void App_Startup(object sender, StartupEventArgs e)
        //{

        //}

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException +=
      new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            //ConfigurationModel confModel = ConfigurationModel.Load();
            //// Register config
            //TinyIoCContainer.Current.Register<IConfigurationModel, ConfigurationModel>(confModel);

            //LanguageService languageService = new LanguageService(confModel);
            //TinyIoCContainer.Current.Register<ILanguageService, LanguageService>(languageService);

            //UndoRedoEngine undoRedoEngine = new UndoRedoEngine();
            //TinyIoCContainer.Current.Register<IUndoRedoEngine, UndoRedoEngine>(undoRedoEngine);

            TinyIoC.TinyIoCContainer.Current.Register<IDataBaseManager, DataBaseManager>().AsMultiInstance();

            var shell = new View.MainWindow();
            WindowService windowService = new WindowService(shell);

            //var model = new MainModel();
            //DllModel model = DllModel.LoadSetting();
            var viewModel = new ViewModel.MainViewModel(windowService);


            shell.DataContext = viewModel;

            shell.Closing += new System.ComponentModel.CancelEventHandler(viewModel.OnClosing);
            shell.Show();

            base.OnStartup(e);
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"{e.Exception.Message} \n {e.Exception.StackTrace} \n {e.Exception?.InnerException?.Message}", "Uncaught Thread Exception",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show($"{ex.Message} \n {ex.StackTrace}", "Uncaught Thread Exception",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
