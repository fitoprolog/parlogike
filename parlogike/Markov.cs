using System.Collections.Generic;
using System.IO;
using System;
using Parlogike_;

namespace Generative
{
    public class Markov
    {
        List<Transition> transitions;

        public int  findPastPresent(string past, string present)
        {
            //Transition ref ret = null ; 

            for (int i = 0; i != transitions.Count; i++)
                if (transitions[i].past.Equals(past)  &&
                    transitions[i].present.Equals(present) )
                {
                    return i; 
                    //ret = transitions[i];
                    //break;
                }
            transitions.Add(new Transition(past, present));
            return transitions.Count - 1;
        }


        public Markov(string corpus)
        {
            transitions = new List<Transition>();
            StreamReader in_ = new StreamReader(corpus);
            string line;

            while ( (line = in_.ReadLine() )!=null)
            {
                string[] tokens = line.Split(' ');
                string past, present, future;
                for (int n = -1; n != tokens.Length; n++)
                {
                    if (n - 1 < 0)
                    {
                        past = "<s>";
                    }
                    else
                    {
                        past = tokens[n - 1];
                    }

                    if (n + 1 >= tokens.Length)
                    {
                        future = "<e>";
                    }
                    else
                    {
                        future = tokens[n + 1];
                    }

                    if (n == -1)
                    {
                        present = "<s>";
                    }
                    else
                    {
                        present = tokens[n];
                    }
                    transitions[findPastPresent(past, present)].addFuture(future);
                    
                }
            }
            in_.Close();
            normalize();
            foreach(var trans in transitions){
                foreach(var fut in trans.future){
                   //Console.Write("{0} {1} ", trans.past,trans.present );
                   //Console.WriteLine(" {0} {1}", fut.Key,fut.Value);
                }
            }
        }

        public string Generate(int limit, Dictionary<string, Variable>  dict, string arg0)
        {

            string past = "<s>", present = "<s>";
            //default_random_engine generator((random_device())());
            //uniform_real_distribution<float> distribution(0,1.0);
            Random random = new Random();

            string ret = "";

            while (limit > 0 && present != "<e>")
            {
                Transition trans = transitions[ findPastPresent(past, present)];
                Dictionary<string, float> futures = trans.future;
                foreach (var future in futures)
                {
                    float next = (float)random.NextDouble();
                    Console.WriteLine("{0} {1}", future.Value,next );
                    if (future.Value >= next)
                    {
                        if (!future.Key.Equals("<e>"))
                        {
                            string var_ = future.Key;
                            Console.WriteLine(var_);
                            if (var_.Length > 2)
                            {
                                if (var_[0] == '<' && var_[var_.Length- 1] == '>')
                                {
                                    var_ = var_.Substring(1, var_.Length - 2);
                                    if (var_.Equals("arg0"))
                                        var_ = arg0;
                                    else
                                    {
                                        if (dict.ContainsKey(var_))
                                            var_ = dict[var_].ToString();
                                    }
                                }
                            }
                            ret += var_ + " ";
                        }
                        past = present;
                        present = future.Key;
                        break;
                    }
                }
                limit--;
            }
            return ret;
        }

        public void normalize()
        {
            int idx;
            for(idx=0; idx != transitions.Count; idx++ ){
                transitions[idx].normalize();
            }
        }

    };
};