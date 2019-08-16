# FaceRecognitionWPF

A wrapper over FaceRecognitionDotNet to search for known people in directories.
![FaceRecognitionWPF Main window](/img/MainWindow.jpg?raw=true "Main window")

A wrapper over FaceRecognitionDotNet to search for known people in directories.
Uses:
* FaceRecognitionDotNet (Porting face_recognition (by Adam Geitgey) by C #)
* DlibDotNet (.NET wrapper for DLib written in C #)
* LiteDB - A lightweight embedded .NET NoSQL document store in a single datafile

Usage:
1. Create directories with the names of people in the training directory and throw photos with faces in them.
![TrainDirectory.jpg](/img/TrainDirectory.jpg?raw=true)
2. Indicate the folder with the trained model for describing faces. You can download it from https://github.com/ageitgey/face_recognition_models
3. Select a directory to search and distance treshold (for KNN search).
4. The form displays faces with a distance less than a given. You can double-click on this image or copy it to a training folder to improve your search.
You can also use the "Show another class and distance more than" button to show, on the contrary, face images for which the threshold is greater than the threshold for the specified class. In order to find people who are not perceived as belonging to this person.

Оболочка над FaceRecognitionDotNet, для поиска известных людей в директориях.
Использует:
* FaceRecognitionDotNet (Porting face_recognition (by Adam Geitgey) by C#)
* DlibDotNet (.NET wrapper for DLib written in C#)
* LiteDB - A lightweight embedded .NET NoSQL document store in a single datafile

Использование:
1. Создайте в директории тренировки директории с именами людей и кидаете в них фотографии с лицами.
![TrainDirectory.jpg](/img/TrainDirectory.jpg?raw=true)
2. Указываете папку с тренированной моделью описания лиц. Скачать можно с https://github.com/ageitgey/face_recognition_models
3. Выбираете папку для поиска и уровень близости (для KNN поиска).
4. В форме отображаются лица, с уровнем близости меньше заданного. Можно открыть это изображени по двойному клику или скопировать в папку для тренировки для улучшения поиска.
Еще можно по кнопке "Show another class and distance more than" показать наоборот изображения лица для которых порог больше заданного для указанного класса. Для того чтобы найти лица которые не воспринимаеются как принадлежащие этому человеку.
