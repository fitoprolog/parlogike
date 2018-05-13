using System.Collections.Generic;
namespace Generative
{
    public class Transition
    {
        public string past, present;
        public Dictionary<string, float> future;
        public float counter = 1;

        public void addFuture(string next)
        {
            if (!future.ContainsKey(next))
                future[next] = 1;
            else
                future[next]++;
            counter++;
        }

        public Transition(string _past, string _present) { 
            future = new Dictionary<string, float>();
            past = _past; 
            present  = _present; 
        }

        public void normalize()
        {
            List<string> keys = new List<string>(future.Keys); 
            for(int idx=0; idx != keys.Count;idx++){
              future[keys[idx]]  /= counter;
            }
        }

    };

};