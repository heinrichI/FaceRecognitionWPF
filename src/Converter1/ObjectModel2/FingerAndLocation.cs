using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter1.ObjectModel2
{
    public class FingerAndLocation 
    {
        public int Id { get; set; }

        //     Gets the y-axis value of the bottom of the rectangle of face.
        public int Bottom { get; set; }

        //     Gets the x-axis value of the left side of the rectangle of face.
        public int Left { get; set; }

        //     Gets the x-axis value of the right side of the rectangle of face.
        public int Right { get; set; }

        //     Gets the y-axis value of the top of the rectangle of face.
        public int Top { get; set; }

        public double[] FingerPrint { get; set; }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FingerAndLocation)obj);
        }

        protected bool Equals(FingerAndLocation other)
        {
            return Bottom == other.Bottom
                && Left == other.Left 
                && Right == other.Right 
                && Top == other.Top 
                && FingerPrint.SequenceEqual(other.FingerPrint);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Bottom;
                hashCode = (hashCode * 397) ^ Left;
                hashCode = (hashCode * 397) ^ Right;
                hashCode = (hashCode * 397) ^ Top;
                hashCode = (hashCode * 397) ^ (FingerPrint != null ? FingerPrint.GetHashCode() : 0);
                return hashCode;
            }
        }

    }
}
