﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.KNN
{
    public class VoteAndDistance
    {
        public VoteAndDistance(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public int Vote { get; internal set; }

        public double Distance { get; internal set; }

        public ClassInfo ClassInfo { get; internal set; }
    }
}
