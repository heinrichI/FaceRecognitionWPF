using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.ViewModel
{
    public interface IClosingViewModel
    {
        void OnClosing(object sender, System.ComponentModel.CancelEventArgs e);
    }
}
