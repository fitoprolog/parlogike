using System.Collections.Generic;
using Generative;
using System;
using System.Text.RegularExpressions;
using System.IO;

namespace Parlogike_
{
   public class Parlogike
  {
    public static SymSpell corrector;

    public static bool init;
    public static Bayes BayesClassifiers;
    public static Dictionary<string, Func<Parlogike, string, List<Variable>, char, bool, Pattern,string,string, Result>> externFunctors;
    public static Dictionary<string, Func<Parlogike, List<Variable>, int, bool>> internFunctors;

    public static Dictionary<string, Markov> MarkovGenerators;

    public Dictionary<string ,Dictionary<string, Variable>> GlobalVariables;
    
    public List<string> LocalStack;
    public List<string> Residues;
    public Dictionary<string, List<string>> Groups;
    List<Pattern> knowledge;
    public int stackCounter = -1;
    public int flags = 0;

    public bool match(string regexp, string input)
    {
      Regex rx = new Regex(regexp, RegexOptions.Compiled | RegexOptions.IgnoreCase);
      return (rx.Matches(input)).Count > 0;
    }

    /*float rand(){
      return distribution(generator);
    }*/
    public static bool IterateDirectory(string dir, Func<string, bool> func)
    {
      string[] files = Directory.GetFiles(dir);
      foreach (var file in files)
      {
        func(file);
      }
      return true;
    }
    public Parlogike()
    {
      corrector = new SymSpell();
      BayesClassifiers = new Bayes(ref corrector);
      externFunctors = new Dictionary<string, Func<Parlogike, string, List<Variable>, char, bool, Pattern,string,string, Result>>();
      internFunctors = new Dictionary<string, Func<Parlogike, List<Variable>, int, bool>>();
      MarkovGenerators = new Dictionary<string, Markov>();
      GlobalVariables = new Dictionary<string,Dictionary<string, Variable>>();
      LocalStack = new List<string>();
      Residues = new List<string>();
      Groups = new Dictionary<string, List<string>>();
      knowledge = new List<Pattern>();
      GlobalVariables[""] = new Dictionary<string, Variable>();

      if (!init)
      {
        Functors.populate();
        init = true;
      }

    }
    public bool parse(string file)
    {
      StreamReader in_ = new StreamReader(file);
      string line;
      int nline = 1;
      bool directiveZone = true;
      bool hasResponse = false;
      bool hasQuestion = false;
      /*auto errfun =[] (int line,string extra){
        cout << "Error at line " << line << ", " << extra << endl; 
      };*/
      Pattern p = new Pattern();
      knowledge.Clear();
      while ((line = in_.ReadLine()) != null)
      {

        if (line.Trim().Length > 0)
        {
          //Console.WriteLine("line:{0}" , line);
          if (line[0] != '#')
          {

            if (line[0] == '.' && !directiveZone)
            {
              errfun(nline, " unexpected directive");
              return false;
            }
            else if (line[0] == '.' && directiveZone)
            {
              p = new Pattern();
              p.isDirective = true;
              //line[0]=' ';
              line = ' ' + line.Substring(1);
              parse(line, true, p, nline);
              knowledge.Add(p);
              p = new Pattern();
            }
            else
            {

              directiveZone = false;

              if (line[0] == '>' && !hasQuestion)
              {
                errfun(nline, "Unexpected response line, there is no question");
                return false;
              }

              if (!hasResponse && hasQuestion && line[0] != '>')
              {
                errfun(nline, "Expected a response");
                return false;
              }

              if (line[0] != '>' && (!hasQuestion || hasResponse))
              {
                hasQuestion = true;
                knowledge.Add(p);
                p = new Pattern();
                parse(line, true, p, nline);
                hasResponse = false;
                continue;
              }
              //line[0] = ' ';
              line = ' ' + line.Substring(1);
              parse(line, false, p, nline);
              hasResponse = true;
            }
          }
        }
        nline++;
      }
      if (hasResponse)
        knowledge.Add(p);
      in_.Close();
      return true;
    }
    public void parse(string line, bool input, Pattern p, int nline)
    {
      char status = 's';
      //int ind =0; 
      string holder = "";
      string _operator = "";
      line += ' ';

      if (!input)
      {
        p.responses.Add(new List<Action>());
      }

      var vTarget = input ? p.input : p.responses[p.responses.Count - 1];

      for (int ind = 0; ind < line.Length; ind++)
      {
        char c = line[ind];

        if (c == ' ' && status == 's') continue;

        if (c == ' ' && (status == 'a' || status == 'o'))
        {
          status = 's';
          if (_operator.Equals(""))
            _operator = " ";

          vTarget.Add(new Action(_operator, holder, nline, ind + 1));
          if (!_operator.Equals("*") && input)
          {
            p.tranferTerms++;
          }
          if (input)
            corrector.CreateDictionaryEntry(holder, 3);
          if (_operator == "->" || _operator == "@")
          {
            p.hasSubject = true;
          }
          holder = "";
          _operator = "";
          continue;
        }

        if (c == ' ' && status == 'w')
        {
          status = 's';

          vTarget.Add(new Action(" ", holder, nline, ind + 1));
          if (!_operator.Equals("*") && input)
          {
            p.tranferTerms++;
          }
          if (input)
            corrector.CreateDictionaryEntry(holder, 3);
          holder = "";
          _operator = "";
          continue;
        }

        if (c == '#') break;

        if ((c >= 65 && c <= 90) || (c >= 97 && c <= 122) ||
            (c >= 48 && c <= 57) ||
            (status == 'n' && c != '_') || status == 'w' || status == 'a')
        {

          if (status == 's')
          {
            status = 'w';
          }

          if (status == 'w')
          {
            holder += c;
            continue;
          }

          if (status == 'o')
          {
            status = 'a';
          }

          if (status == 'a')
          {
            holder += c;
            continue;
          }

          if (status == 'n')
          {
            _operator += c;
          }

        }
        else
        {

          if (c == '_')
          {
            if (status == 's')
            {
              status = 'n';
            }
            else if (status == 'n')
            {
              status = 'a';
            }
            continue;
          }
          else
          {
            if (status == 's')
            {
              status = 'o';
            }
            if (status == 'o')
            {
              _operator += c;
            }
          }

        }
      }
    }
    public void errfun(int line, string extra)
    {
      Console.WriteLine("Error at line{0} , {1}", line, extra);
    }
    public void dumpDebug()
    {
      Console.WriteLine("=========Directives Zone============\n");
      bool directives = true;
      for (int i = 0; i != knowledge.Count; i++)
      {
        Pattern p = knowledge[i];
        if (directives && !p.isDirective)
        {
          Console.WriteLine("=========End OfDirectives Zone============\n");
          directives = false;
        }
        //cout << ""=========End OfDirectives Zone============\n"; 
        var input = p.input;
        var responses = p.responses;
        Console.WriteLine("=========Pattern============\n");
        Console.WriteLine("hasSubject?:{0}", (p.hasSubject ? "YES" : "NO"));
        foreach (var in_ in input)
        {
          if (in_.arguments.Count == 0)
          {
            Console.WriteLine("operator {0} argument:", in_._operator);
          }
          foreach (var arg in in_.arguments)
          {
            Console.WriteLine("operator {0} argument: {1}", in_._operator, arg.strVal);
          }
        }
        if (directives) continue;

        Console.WriteLine("=========Responses============");
        foreach (var response in responses)
        {
          foreach (var out_ in response)
          {
            if (out_.arguments.Count == 0)
            {
              Console.WriteLine("operator {0} argument:", out_._operator);
            }
            foreach (var arg in out_.arguments)
            {
              Console.WriteLine("operator {0} argument: {1} ", out_._operator, arg.strVal);
            }
          }
        }
      }

    }

    public bool executeDirectives(bool onlyWarn)
    {
      bool success = true;
      foreach (var pattern in knowledge)
      {
        if (!pattern.isDirective)
        {
          return success;
        }
        foreach (var instruction in pattern.input)
        {
          if (!Parlogike.internFunctors.ContainsKey(instruction._operator))
          {
            Console.WriteLine("Directive execution time error near {0},{1} undefined operator {2}",
                    instruction.line, instruction.column, instruction._operator);
            if (!onlyWarn)
              return false;
            success = false;
          }
          else
          {
            Parlogike.internFunctors[instruction._operator]
              (this, instruction.arguments, instruction.line);
          }
        }
      }
      return true;
    }

    public void clearAttention()
    {
      foreach (var p in knowledge)
      {
        p.attention = 0;
        p.subjectMatched = false;
        foreach (var a in p.input)
          a.matched = false;
        //p.LocalStack.clear();
      }
    }
    public string respond(string input, bool onlyWarn,string session)
    {

      if (!GlobalVariables.ContainsKey(session))
        GlobalVariables[session] = new Dictionary<string, Variable>();

      string ret = "", subret = "";
      clearAttention();
      warmAttention2(input, onlyWarn,session);
      Pattern p;
      float max = 0;
      Random random = new Random();
      foreach (Pattern pm in knowledge)
      {
        if (pm.isDirective) continue;
        if (max < pm.attention)
        {
          max = pm.attention;
        }
      }
      if (max == 0) return ret;

      foreach (Pattern pm in knowledge)
      {
      
        if ((max - pm.attention > 0) || pm.isDirective || pm.responses.Count == 0) continue;
        if ((max - pm.attention == 0) && pm.hasSubject && !pm.subjectMatched) continue;

        p = pm;
        List<Action> responses = new List<Action>();
        //cout << "number of responses" << pm.responses.size() << endl;
        float offset = p.hasSubject ? 1.0f : 0.0f;
        int r = (int)(offset + (float)random.NextDouble() * (float)p.responses.Count);

        if (r >= p.responses.Count)
          r = p.responses.Count - 1;
        responses = p.responses[r];
        //cout << "responding" << r << endl ; 
        foreach (var action in responses)
        {
          if (!Parlogike.externFunctors.ContainsKey(action._operator))
          {
            Console.WriteLine("Output pattern execution time error near {0},{1} undefined operator {2}",
                                action.line, action.column, action._operator);
            if (!onlyWarn)
              return "";
            else
              continue;
          }
          subret = "";
          var res = Parlogike.externFunctors[action._operator]
              (this, "", action.arguments, 'r', true, p,session,"");
          ret += res.s;
        }
        ret += ",";
        p.histogram++;
      }
      /*if (ret.Length > 1)
          ret = ret.Substring(0, ret.Length - 2);*/
      return ret;
    }

    public void warmAttention2(string input, bool onlyWarn,string session)
    {
      var tokens_ = input.Split(' ');
      List<string> correcteds = new List<string>();
      List<bool> notPush = new List<bool>();
      for (int t = 0; t != tokens_.Length; t++)
      {
        List<SymSpell.SuggestItem> items = corrector.Lookup(tokens_[t], SymSpell.Verbosity.Closest);
        notPush.Add(false);
        if (items.Count > 0)
        {
          correcteds.Add(items[0].term);
        }
        else
        {
          correcteds.Add("");
        }
      }

      float max = 0;
      for (int kInd = 0; kInd != knowledge.Count; kInd++)
      {
        var sp = knowledge[kInd];
        if (sp.isDirective) continue;
        //Console.WriteLine("at pattern {0}", kInd);
        warmXcorr(sp, tokens_, correcteds.ToArray(), max, (string s, Action a,string extra) =>
        {
                  //(Parlogike self, string input,  List<Variable> args,  char dir, bool mutate, Pattern pattern) 
                  if (!Parlogike.externFunctors.ContainsKey(a._operator))
          {
            Console.WriteLine("Operator {0} doesnt exists at line {1}", a._operator, a.line);
            return 0;
          }
          return (Parlogike.externFunctors
            [a._operator](this, s, a.arguments, 'i', false, sp,session,extra)).w;
        });
      }

    }
    public float warmXcorr(Pattern pattern, string[] input, 
      string[] correcteds, float ret, Func<String, Action,string, float> fun)
    {
      int inputIndex = 0;
      int pIndex, pStart = pattern.input.Count - 1;
      int mode = 0, extra;
      float cw = 0,subcw=0, sub = 0, sub2 = 0,sub3=0,sub4=0;
      //int nWildCards = 0;
      bool notIgnore = false; 

      List<Action> intern = pattern.input;
      List<string> stack = new List<string>();

      string stackTerm = "";

      for (int inputOffset = 0; inputOffset < input.Length; inputOffset++)
      {
        subcw = 0;
        if (mode == 0)
        {
          for (pStart = intern.Count - 1; pStart != -1; pStart--)
          {
            stack = new List<string>();
            inputIndex = 0;

            for (pIndex = pStart; pIndex < intern.Count; pIndex++)
            {
              if ((inputOffset + inputIndex) >= input.Length)
              {
                break;
              }

              if (intern[pIndex]._operator.Equals("*"))
              {
                stackTerm = "";
                extra = 0;
                if (pIndex + 1 < intern.Count){
                  notIgnore = !intern[pIndex+1]._operator.Equals("*!=|")&&
                              !intern[pIndex+1]._operator.Equals("*=|" );
                }
                else
                {
                  notIgnore = false;
                }

                if (pIndex + 1 < intern.Count && !notIgnore)
                {
                  while ((sub3=fun(input[inputIndex + inputOffset + extra], intern[pIndex + 1],stackTerm)) <= 0.01 &&
                        (sub4=fun(correcteds[inputIndex + inputOffset + extra], intern[pIndex + 1],stackTerm)) <= 0.01)
                  {
                    stackTerm += input[inputIndex + inputOffset + extra] + " ";
                    extra++;
                    if ((inputIndex + inputOffset + extra >= input.Length)) break;
                  }
                  subcw+=sub3+sub4;

                }
                else
                {
                  while ((inputIndex + inputOffset + extra) < input.Length)
                  {
                    stackTerm += input[inputIndex + inputOffset + extra] + " ";
                    extra++;
                  }
                }
                stack.Add(stackTerm);
                if (pIndex + 1 < intern.Count){
                  subcw+=fun("", intern[pIndex + 1],"");
                }
                stackTerm = "";
                sub = 0;
                sub2 = 0;
                inputIndex += extra-1;

              }
              else
              {
                if (inputIndex < input.Length)
                {
                  sub = fun(input[inputIndex + inputOffset], intern[pIndex],stackTerm);
                  sub2 = fun(correcteds[inputIndex + inputOffset], intern[pIndex],stackTerm);
                }
              }
              if (sub2 > sub) sub2 = sub;

              subcw += sub;
              inputIndex++;
            }
            if (subcw > cw || (cw == subcw && pattern.LocalStack.Count < stack.Count))
            {
              pattern.LocalStack = stack;
              cw = subcw;
              Console.WriteLine("switching stack Mode=0 pStart={0}", pStart);
            }
          }
          mode = 1;
        }
        else /*(mode == 1)*/
        {
          inputIndex = 0;
          stack = new List<string>();
          for (pIndex = 0; pIndex != intern.Count; pIndex++)
          {
            int ninputOffset = inputIndex + inputOffset;

            if (ninputOffset >= input.Length) break;
            if (intern[pIndex]._operator.Equals("*"))
            {
              stackTerm = "";
              extra = 0;
              if (pIndex + 1 < intern.Count)
              {
                notIgnore = !intern[pIndex + 1]._operator.Equals("*!=|") &&
                            !intern[pIndex + 1]._operator.Equals("*=|" );
              }
              else
              {
                notIgnore = false;
              }
              if (pIndex + 1 < intern.Count && !notIgnore)
              {
                while ((sub3=fun(input[inputIndex + inputOffset + extra], intern[pIndex + 1],stackTerm)) <= 0.01 &&
                      (sub4=fun(correcteds[inputIndex + inputOffset + extra], intern[pIndex + 1],stackTerm)) <= 0.01)
                {
                  stackTerm += input[inputIndex + inputOffset + extra] + " ";
                  extra++;
                  if ((inputIndex + inputOffset + extra >= input.Length)) break;
                }

              }
              else
              {
                while ((inputIndex + inputOffset + extra) < input.Length)
                {
                  stackTerm += input[inputIndex + inputOffset + extra] + " ";
                  extra++;
                }
              }
              inputIndex += extra-1;
              stack.Add(stackTerm);
              //for  local stack depending operators
              if (pIndex + 1 < intern.Count){
                subcw+=fun("", intern[pIndex + 1],"");
              }
              sub = 0;
              sub2 = 0;
            }
            else
            {
              if (inputIndex < input.Length)
              {
                sub = fun(input[inputIndex + inputOffset], intern[pIndex],stackTerm);
                sub2 = fun(correcteds[inputIndex + inputOffset], intern[pIndex],stackTerm);
              }
            }
            if (sub2 > sub) sub2 = sub;
            subcw += sub;
            inputIndex++;
          }

        }
        if (subcw > cw || ( subcw ==cw  && stack.Count > pattern.LocalStack.Count))
        {
          pattern.LocalStack = stack;
          cw = subcw;
          Console.WriteLine("switching stack");
        }

      }
      foreach (var x in pattern.LocalStack)
      {
        Console.WriteLine("A:{0}", x);
      }
      Console.WriteLine("max {0}", cw);
      cw /= pattern.tranferTerms; 
      pattern.attention = cw;
      if (cw > ret )
        ret = cw;  
      return ret;
    }
  }
}