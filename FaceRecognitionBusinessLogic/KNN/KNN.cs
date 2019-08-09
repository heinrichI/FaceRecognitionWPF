using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.KNN
{
    class KNN
    {
        public static int Classify(double[] unknown, double[][] trainData, int numClasses, int k)
        {
            // compute and store distances from unknown to all train data 
            int n = trainData.Length;  // number data items
            IndexAndDistance[] info = new IndexAndDistance[n];
            for (int i = 0; i < n; ++i)
            {
                IndexAndDistance curr = new IndexAndDistance();
                double dist = Distance(unknown, trainData[i]);
                curr.Index = i;
                curr.Distance = dist;
                info[i] = curr;
            }

            Array.Sort(info);  // sort by distance
            Console.WriteLine("\nNearest  /  Distance  / Class");
            Console.WriteLine("==============================");
            for (int i = 0; i < k; ++i)
            {
                int classIndex = (int)trainData[info[i].Index][2];
                string dist = info[i].Distance.ToString("F3");
                Console.WriteLine("( " + trainData[info[i].Index][0] + "," + trainData[info[i].Index][1] + " )  :  " 
                    + dist + "        " + classIndex);
            }

            int result = Vote(info, trainData, numClasses, k);  // k nearest classes
            return result;

        } // Classify

        static double Distance(double[] unknown, double[] data)
        {
            double sum = 0.0;
            for (int i = 0; i < unknown.Length; ++i)
                sum += (unknown[i] - data[i]) * (unknown[i] - data[i]);
            return Math.Sqrt(sum);
        }

        static int Vote(IndexAndDistance[] info, double[][] trainData, int numClasses, int k)
        {
            int[] votes = new int[numClasses];  // one cell per class
            for (int i = 0; i < k; ++i)  // just first k nearest
            {
                int idx = info[i].Index;  // which item
                int c = (int)trainData[idx][2];  // class in last cell
                ++votes[c];
            }

            //for (int p = 0; p < votes.Length; ++p)
            //{
            //  Console.Write(votes[p] + " ");
            //}
            //Console.WriteLine("");

            int mostVotes = 0;
            int classWithMostVotes = 0;
            for (int j = 0; j < numClasses; ++j)
            {
                if (votes[j] > mostVotes)
                {
                    mostVotes = votes[j];
                    classWithMostVotes = j;
                }
            }

            return classWithMostVotes;
        }

    }
}
