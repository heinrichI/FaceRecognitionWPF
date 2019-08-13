using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.ObjectModel
{
    public class FaceLocation
    {
        public FaceLocation(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        ////     Gets the y-axis value of the bottom of the rectangle of face.
        public int Bottom { get; }

        ////     Gets the x-axis value of the left side of the rectangle of face.
        public int Left { get; }

        ////     Gets the x-axis value of the right side of the rectangle of face.
        public int Right { get; }

        ////     Gets the y-axis value of the top of the rectangle of face.
        public int Top { get; }

        //public int Width
        //{
        //    get { return Right - Left; }
        //}

        //public int Heigth
        //{
        //    get { return Bottom - Top; }
        //}

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FaceLocation)obj);
        }

        protected bool Equals(FaceLocation other)
        {
            return Bottom == other.Bottom && Left == other.Left && Right == other.Right && Top == other.Top;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Bottom;
                hashCode = (hashCode * 397) ^ Left;
                hashCode = (hashCode * 397) ^ Right;
                hashCode = (hashCode * 397) ^ Top;
                return hashCode;
            }
        }
    }
}
