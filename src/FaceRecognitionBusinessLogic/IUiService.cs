using System.Collections.Generic;
using FaceRecognitionBusinessLogic.ObjectModel;

namespace FaceRecognitionBusinessLogic
{
    /// <summary>
    /// Output Port: уведомления для пользователя из worker-нитей use-cases.
    /// Реализуется UI-адаптером (WPF); реализация сама отвечает за
    /// marshaling вызовов в UI-поток.
    /// </summary>
    public interface IUiService
    {
        void ShowMessage(string message, string caption);

        /// <summary>
        /// Показать окно с изображением и рамками лиц
        /// (пустой список — «лицо не найдено»).
        /// </summary>
        bool? ShowFaceWindow(string imagePath, IEnumerable<FaceLocation> faceLocations);
    }
}