using System.Collections.Generic;

namespace Parlogike_
{
    public class Action
    {
        public bool isAFilter = false;
        public bool matched = false;
        public int error = 0;
        public string _operator = "";
        public int line, column;  //for future debug 
        public List<Variable> arguments;
        public Action(string op, string arg, int _line, int _column)
        {
            arguments = new List<Variable>();
            line = _line;
            column = _column;
            _operator = op;

            string[] _split = arg.Split(',');
            foreach (string narg in _split)
            {
                arguments.Add(new Variable(narg));
            }
        }
    };
}