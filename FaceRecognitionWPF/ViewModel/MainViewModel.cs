using FaceRecognitionDotNet;
using FaceRecognitionWPF.KNN;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.ViewModel 
{
    class MainViewModel : BasePropertyChanged
    {
        public MainViewModel()
        {
            TrainPath = @"d:\tmp\TrainFace";
            ModelsDirectory = @"d:\face_recognition_models";
        }

        string _trainPath;
        public string TrainPath
        {
            get => this._trainPath;
            set
            {
                this._trainPath = value;
                this.RaisePropertyChangedEvent("TrainPath");
            }
        }

        string _modelsDirectory;
        public string ModelsDirectory
        {
            get => this._modelsDirectory;
            set
            {
                this._modelsDirectory = value;
                this.RaisePropertyChangedEvent("ModelsDirectory");
            }
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {

        }

        private RelayCommand _runCommand;
        public RelayCommand RunCommand
        {
            get
            {
                return this._runCommand ?? (this._runCommand = new RelayCommand(async (arg) =>
                {
                    //var openFileDialog = new OpenFileDialog();
                    //var dialogResult = openFileDialog.ShowDialog();
                    //if (dialogResult != true)
                    //    return;

                    //var path = openFileDialog.FileName;

                    

                    await Task.Run(() =>
                    {
                        using (var faceRecognition = FaceRecognition.Create(ModelsDirectory))
                        {
                            var directories = System.IO.Directory.GetDirectories(TrainPath);

                            foreach (var directory in directories)
                            {
                                ClassInfo ci = new ClassInfo(directory);

                                var files = System.IO.Directory.GetFiles(directory);

                                foreach (var imageFile in files)
                                {
                                    using (var image = FaceRecognition.LoadImageFile(imageFile))
                                    {
                                        //var sw = new Stopwatch();
                                        //sw.Start();

                                        var encodings = faceRecognition.FaceEncodings(image);
                                        if (encodings == null)
                                            continue;

                                        foreach (var encoding in encodings)
                                        {
                                            IFormatterConverter formatterConverter = new FormatterConverter();
                                            var info = new SerializationInfo(typeof(double), formatterConverter);
                                            StreamingContext context = new StreamingContext();
                                            encoding.GetObjectData(info, context);

                                            double[] doubleInfo = (double[])info.GetValue("_Encoding", typeof(double[]));
                                            ci.Data.AddRange(doubleInfo);
                                            //encoding.Dispose();
                                        }

                                        //sw.Stop();

                                        //var total = sw.ElapsedMilliseconds;
                                        //Console.WriteLine($"Total: {total} [ms]");
                                    }

                                    //using (var faceDetector = Dlib.GetFrontalFaceDetector())
                                    //using (var data = new MemoryStream(File.ReadAllBytes(imageFile)))
                                    //{
                                    //    // DlibDotNet can create Array2D from file but this sample demonstrate
                                    //    // converting managed image class to dlib class and vice versa.
                                    //    var bitmap = new WriteableBitmap(BitmapFrame.Create(data));
                                    //    using (var image = bitmap.ToArray2D<RgbPixel>())
                                    //    {
                                    //        var dets = faceDetector.Operator(image);
                                    //        foreach (var r in dets)
                                    //            Dlib.DrawRectangle(image, r, new RgbPixel { Green = 255 });

                                    //        var result = image.ToWriteableBitmap();
                                    //        if (result.CanFreeze)
                                    //            result.Freeze();
                                    //        Application.Current.Dispatcher.Invoke(() =>
                                    //        {
                                    //            this.Image = result;
                                    //        });
                                    //    }
                                    //}
                                }

                                
                            }

                            int predicted = KNN.KNN.Classify(unknown, trainData, numClasses, k);
                            // FaceRecognition.CompareFaces()
                        }
                    });
                }, (arg) => true));
            }
        }
    }
}
