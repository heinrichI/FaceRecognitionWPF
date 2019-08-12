using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaceRecognitionBusinessLogic.ObjectModel;

namespace FaceRecognitionBusinessLogic.DataBase
{
    public class FingerAndLocation 
    {
        public int Id { get; set; }

        ////     Gets the y-axis value of the bottom of the rectangle of face.
        //public int Bottom { get; set; }

        ////     Gets the x-axis value of the left side of the rectangle of face.
        //public int Left { get; set; }

        ////     Gets the x-axis value of the right side of the rectangle of face.
        //public int Right { get; set; }

        ////     Gets the y-axis value of the top of the rectangle of face.
        //public int Top { get; set; }

        public double[] FingerPrint { get; set; }

        public FaceLocation Location { get; set; }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FingerAndLocation)obj);
        }

        protected bool Equals(FingerAndLocation other)
        {
            return Equals(Location, other.Location) && Equals(FingerPrint, other.FingerPrint);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Location != null ? Location.GetHashCode() : 0) * 397) ^ (FingerPrint != null ? FingerPrint.GetHashCode() : 0);
            }
        }

    }
}
