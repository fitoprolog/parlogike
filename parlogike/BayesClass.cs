using System.Collections.Generic;
using System.IO;
using System;

namespace Generative
{
    public class BayesClass
    {
        public string name;
        public float probability = 0; //class probability 
        public float tokencounter = 0;
        Dictionary<string, float> pToken; //token probability
        public void normalize()
        {
            /*for (map<string,float>::iterator it=pToken.begin(); it!=pToken.end(); it++){
              it->second/=tokencounter; 
            }*/
            foreach (var ptoken in pToken)
                pToken[ptoken.Key] /= tokencounter;
        }
        public BayesClass(string _name)
        {
            name = _name;
            pToken = new Dictionary<string, float>();
        }
        public float classify(string[] tokens)
        {
            float ret = 1.0f;
            for (int ind = 0; ind != tokens.Length; ind++)
            {
                if (!pToken.ContainsKey(tokens[ind]))
                {
                    ret *= 0.001f;
                }
                else
                {
                    ret *= pToken[tokens[ind]];
                }
            }
            return ret;
        }
        public int parseFile(ref SymSpell corrector, string filename)
        {
            //fstream in(filename);
            StreamReader in_ = new StreamReader(filename);
            string line;
            int ret = 0;
            while ((line = in_.ReadLine()) != null)
            {
                string[] tokens = line.Split(' ');
                tokencounter += tokens.Length;
                for (int ind = 0; ind != tokens.LongLength; ind++)
                {
                    //corrector->CreateDictionaryEntry(tokens[ind]);
                    if (!pToken.ContainsKey(tokens[ind]))
                    {
                        pToken[tokens[ind]] = 1;
                    }
                    else
                    {
                        pToken[tokens[ind]]++;
                    }
                }
                ret++;
            }
            probability += ret;
            return ret;
        }

    };
};