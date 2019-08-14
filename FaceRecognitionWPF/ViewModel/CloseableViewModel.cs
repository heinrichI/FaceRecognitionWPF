using FaceRecognitionBusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FaceRecognitionWPF.ViewModel
{
    public abstract class CloseableViewModel : BasePropertyChanged, IClosingViewModel
    {
        public event EventHandler<BoolEventHandler> ClosingRequest;

        public void RaiseClosingRequest(bool dialogResult)
        {
            if (this.ClosingRequest != null)
            {
                this.ClosingRequest(this, new BoolEventHandler(dialogResult));
            }
        }

        ICommand _closeCommand;
        public virtual ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new RelayCommand(arg =>
                {
                    RaiseClosingRequest(false);
                }, arg => true));
            }
        }

        public virtual void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        { }
    }

}
