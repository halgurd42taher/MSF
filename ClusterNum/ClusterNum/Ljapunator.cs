﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Math;
using Accord.Math.Decompositions;
using System.Windows.Forms;

namespace ClusterNum
{
    //Ljapunator Klasse integriert eta über t
    public class Ljapunator
    {


        [ThreadStatic]
        static Random rand;


        double beta, sigma, delta;

        public List<double[]> etat = new List<double[]>();
        public double pertubation = 0.00;

        double[,] BMat;
        double[][,] JMats;
        List<double[]> smts;
        int clusterCount;

        int nodeCount;

        public double ljapunowSum;


        //Konstruktor , berechnung braucht J_m's , B und die Cluster zum summieren
        public Ljapunator(double[][,] JMats, double[,] BMat, int clustercount, List<double[]> smts, double beta, double sigma, double delta)
        {
            rand = new Random();
            this.beta = beta;
            this.sigma = sigma;
            this.delta = delta;
            this.nodeCount = BMat.GetLength(0);
            etat.Add(new double[nodeCount]);

            ljapunowSum = 0;

            this.smts = smts;
            this.JMats = JMats;
            this.BMat = BMat;

            // this.cluster = cluster;
            this.clusterCount = clustercount;
        }


        public void iterate()
        {

            int tIndex = etat.Count - 1;

            double[] oldetai = etat[etat.Count - 1];
            double[] newetai = new double[nodeCount];

            double[,] sumJDF = new double[nodeCount, nodeCount];
            double[,] sumJDH = new double[nodeCount, nodeCount];

            for (int clusternum = 0; clusternum < clusterCount; clusternum++)
            {

                double DFsmt = beta * dintensity(smts[tIndex][clusternum]);
                double DHsmt = sigma * dintensity(smts[tIndex][clusternum]);

                sumJDF = sumJDF.Add(DFsmt.Multiply(JMats[clusternum]));
                sumJDH = sumJDH.Add(DHsmt.Multiply(JMats[clusternum]));

            }

            newetai = sumJDF.Multiply(oldetai);
            newetai = newetai.Add(BMat.Multiply(sumJDH).Multiply(oldetai));

            //orthogonalisieren - nur instabile richtungen beibehalten
            for (int icluster = 0; icluster < clusterCount; icluster++)
            {
                newetai[icluster] = 0;
            }


            double oldetalength = oldetai.Euclidean();
            double newetalength = newetai.Euclidean();

            double scale = oldetalength / newetalength;
            double tmp = Math.Log(Math.Abs(1 / scale));
            //double[] tmp = newetai.ElementwiseDivide(oldetai).Abs().Log();

            ljapunowSum += tmp;
            //normalisieren
            newetai = scale.Multiply(newetai);

            etat.Add(newetai);
        }
        public void iterate(int numIteration)
        {
            for (int i = 0; i < numIteration; i++)
            {
                iterate();
            }
        }




        private double intensity(double x)
        {
            double ret = (1 - Math.Cos(x)) / 2.0;
            return ret;
        }
        public double dintensity(double x)
        {
            double ret = (Math.Sin(x)) / 2.0;
            return ret;
        }

        private double[] intensities(double[] xi)
        {
            double[] pertIntensity = new double[nodeCount];
            for (int i = 0; i < pertIntensity.Length; i++)
            {

                //double err = (rand.NextDouble() - 0.5) * 2.0 * pertubation;
                double u1 = rand.NextDouble();
                double u2 = rand.NextDouble();
                double randnormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double err = pertubation * randnormal;

                pertIntensity[i] = intensity(xi[i]);
                pertIntensity[i] += err;
            }
            return pertIntensity;

        }

    }

}
