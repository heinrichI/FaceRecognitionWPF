using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.DataBase
{
    public class FingerAndLocation
    {
        //     Gets the y-axis value of the bottom of the rectangle of face.
        public int Bottom { get; set; }

        //     Gets the x-axis value of the left side of the rectangle of face.
        public int Left { get; set; }

        //     Gets the x-axis value of the right side of the rectangle of face.
        public int Right { get; set; }

        //     Gets the y-axis value of the top of the rectangle of face.
        public int Top { get; set; }

        public double[] FingerPrint { get; set; }
}
}
