using System.Collections.Generic;
namespace Parlogike_
{
     public class Pattern
    {

        public bool isDirective = false;
        public float tranferTerms = 0.0f;
        /***
         * Attention tell us how much the pattern matches the input
         * -1 means ignore or inhibit 
         * */
        public float attention = 0.0f;
        /**
         * each time the pattern matches it increases
         * */
        public float histogram = 0.0f;
        public bool hasSubject = false;
        public bool subjectMatched = false;
        public List<Action> input;
        public List<List<Action>> responses;
        public List<string> LocalStack;
        public string context ="";
        public string contextRouter = ""; 
        public Pattern()
        {
            input = new List<Action>();
            responses = new List<List<Action>>();
            LocalStack = new List<string>();
        }
    };
}