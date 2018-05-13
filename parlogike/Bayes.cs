using System.Collections.Generic;
using System.IO;
using System;

namespace Generative
{
    public  class Bayes
    {

        public SymSpell corrector;

        public Bayes(ref SymSpell corrector_)
        {
            corrector = corrector_;
            classes = new List<BayesClass>();
        }

        public float samplesCount = 0;

        List<BayesClass> classes;

        public void normalize()
        {

            for (int ind = 0; ind != classes.Count; ind++)
            {
                classes[ind].normalize();
                classes[ind].probability /= samplesCount;
            }
        }

        public void addClass(string filename, string classname)
        {
            BayesClass cl = new BayesClass(classname);
            samplesCount += cl.parseFile(ref corrector, filename);
            classes.Add(cl);
        }

        public string classify(string input)
        {
            return classify(input.Split(' '));
        }

        string classify(string[] tokens)
        {
            float max = 0, p = 1;
            BayesClass cl = new BayesClass("");

            for (int ind = 0; ind != classes.Count; ind++)
            {
                p = classes[ind].classify(tokens) * classes[ind].probability;
                if (p > max)
                {
                    max = p;
                    cl = classes[ind];
                }
            }
            return cl.name;
        }
        public static void Test(){
            SymSpell corrector = new SymSpell();
            Bayes cl = new Bayes(ref corrector);
            cl.addClass("./GENERATIVE/BAYES/grasa","grasa");
            cl.addClass("./GENERATIVE/BAYES/good","good");
            while(true){
               Console.Write("Input=");
               string line = Console.ReadLine();
               Console.WriteLine(cl.classify(line));   
            }
        }
    };
};