using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.KNN
{
    class MyKnn
    {
        public static VoteAndDistance Classify(double[] unknown, List<ClassInfo> trainData, string[] classes, int k)
        {
            if (!classes.Contains(trainData.First().Name))
                throw new ArgumentException($"В классах нет класа {trainData.First().Name}");

            // compute and store distances from unknown to all train data 
            int totalDataItems = trainData.Count;  // number data items
            IndexAndDistance[] info = new IndexAndDistance[totalDataItems];
            for (int i = 0; i < totalDataItems; ++i)
            {
                IndexAndDistance curr = new IndexAndDistance();
                double dist = Distance(unknown, trainData[i].Data);
                curr.Index = i;
                curr.Distance = dist;
                info[i] = curr;
            }

            Array.Sort(info);  // sort by distance
            //Debug.WriteLine("\nDistance  / Class");
            //for (int i = 0; i < k; ++i)
            //{
            //    string className = trainData[info[i].idx].Name;
            //    string dist = info[i].dist.ToString("F3");
            //    Debug.WriteLine(dist + " " + className);
            //}

            VoteAndDistance result = Vote(info, trainData, classes, k);  // k nearest classes
            return result;
        }

        static double Distance(double[] unknown, double[] data)
        {
            double sum = 0.0;
            for (int i = 0; i < unknown.Length; ++i)
                sum += (unknown[i] - data[i]) * (unknown[i] - data[i]);
            //return Math.Sqrt(sum);
            return sum;
        }

        static VoteAndDistance Vote(IndexAndDistance[] info, List<ClassInfo> trainData, string[] classes, int k)
        {
            Dictionary<string, VoteAndDistance> votes = new Dictionary<string, VoteAndDistance>(classes.Length);
            foreach (var item in classes)
            {
                votes.Add(item, new VoteAndDistance(item));
            }

            for (int i = 0; i < k; ++i)  // just first k nearest
            {
                int idx = info[i].Index;  // which item
                string className = trainData[idx].Name; 
                votes[className].Vote++;
                votes[className].Distance =+ info[i].Distance;
            }

            //for (int p = 0; p < votes.Length; ++p)
            //{
            //  Console.Write(votes[p] + " ");
            //}
            //Console.WriteLine("");

            int mostVotes = 0;
            VoteAndDistance classWithMostVotes = null;
            foreach (var className in classes)
            {
                if (votes[className].Vote > mostVotes)
                {
                    mostVotes = votes[className].Vote;
                    classWithMostVotes = votes[className];
                }
            }

            return classWithMostVotes;
        }
    }
}
