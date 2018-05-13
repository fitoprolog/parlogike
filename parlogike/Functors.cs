using System;
using System.Collections.Generic;
using Generative;

namespace Parlogike_
{
  public class Functors
  {
    public static void populate(/*Parlogike pl*/)
    {
      /*Parlogike::internFunctors["["] = [](<vector>args){

      };*/
      Parlogike.IterateDirectory("./GENERATIVE/BAYES/", (string src) =>
      {
        Parlogike.BayesClassifiers.addClass(/*"./GENERATIVE/BAYES/"+*/ src, src);
        return true;
      });

      Parlogike.IterateDirectory("./GENERATIVE/MARKOV/", (string src) =>
      {
              /*Parlogike.MarkovGenerators.insert(
                pair<string,Markov>(src,
                  MarkovTextGenerator(string("./GENERATIVE/MARKOV/")+src))
              );*/
        Parlogike.MarkovGenerators[src] = new Markov(/*"./GENERATIVE/MARKOV/" + */src);
        return true;
      });

      Parlogike.internFunctors[">>"] = (Parlogike self, List<Variable> args, int line) =>
      {
        foreach (var a in args)
          Console.Write(a.toString());
        Console.WriteLine("\n");
        return true;
      };

      Parlogike.internFunctors[":="] = (Parlogike self, List<Variable> args, int line) =>
      {
        if (args.Count < 2)
        {
          Console.WriteLine("Warning: set operator requires symbol name and value\n");
          return false;
        }
        self.GlobalVariables[""][args[0].toString(true)] = args[1];//.toString(); 
              return true;
      };

      Parlogike.internFunctors["@"] = (Parlogike self, List<Variable> args, int line) =>
      {
        if (args.Count < 2)
        {
          Console.WriteLine("Warning at line {0} trying to create empty group\n", line);
          return false;
        }
        if (!self.Groups.ContainsKey(args[0].toString(true)))
          self.Groups[args[0].toString()] = new List<string>();

        var group = self.Groups[args[0].toString()];
        for (int ind = 1; ind < args.Count; ind++)
        {
          group.Add(args[ind].toString());
        }
        return true;
      };

      //------------------------intern functors -----------------------  
      //self, input , args, n , dir,   and mutate declared in EXTERNFUN 
      // this functor should be remplaced instead of a 1 it should 
      // value less the most commons 
      //Parlogike *self,vector<string>input ,string &out,vector<Variable>args,int n ,char dir,bool mutate, Pattern &pattern 
      //Markov generator name,  Dictionary name, Variable name
      Parlogike.externFunctors["?"] = (Parlogike self, string input,
            List<Variable> args, char dir, bool mutate, 
            Pattern pattern,string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
          if (args.Count == 0)
          {
            Console.WriteLine("Warning: Bayesian operator requieres one class name");
            return ret;
          }
          foreach (var v in args)
          {
            if (Parlogike.BayesClassifiers.classify(input).Equals(v.toString(true)))
            {
              ret.w = 1.0f;
              return ret;
            }
          }
        }
        if (dir == 'r')
        {
          if (args.Count == 0)
          {
            Console.WriteLine("Warning: Markov operator requires at least one class name\n");
            return ret;
          }
          //auto it = Parlogike::MarkovGenerators.find(args[0].toString(true));
          if (!Parlogike.MarkovGenerators.ContainsKey(args[0].toString(true)))
            return ret;

          var generator = Parlogike.MarkovGenerators[args[0].toString(true)];
          string ctx_ = "";
          
          //u means user or local context 
          if (args.Count >=2)
            if (args[1].toString(true).Equals("u"))
              ctx_ = ctx;

          if (args.Count == 3)
          {
            ret.s += generator.Generate(10, self.GlobalVariables[ctx_],args[2].toString(true)) + " ";
          }
          if (args.Count == 2)
          {
            ret.s += generator.Generate(10, self.GlobalVariables[ctx_],"") + " ";
          }
          if (args.Count == 1 ){
            ret.s += generator.Generate(10, self.GlobalVariables[ctx_],"") + " ";
          }
        }
        return ret;
      };
      Parlogike.externFunctors[":="] = (Parlogike self, string input, 
           List<Variable> args, char dir, bool mutate, Pattern pattern, 
           string ctx,string extra) =>
      {
        Result ret = new Result();
        if (dir == 'r')
        {
          if (args.Count < 3)
          {
            Console.WriteLine("Warning: set operator requires  context , symbol name and value");
            return ret;
          }
          string suffix ="";
          if (args.Count >3 ){
            suffix = args[3].toString(true);
          }
          string ctx_ = "";
          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;
          self.GlobalVariables[ctx_][suffix+args[1].toString(true)] = args[2];
        }
        return ret;
      };
      Parlogike.externFunctors["=="] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx,string extra) =>
      {
        Result ret = new Result();
        ret.w = -1.5f;
        if (dir == 'i')
        {
          if (args.Count < 3)
          {
            Console.WriteLine("Warning: gt  operator requires context, symbol name and value");
            return ret;
          }
          string ctx_="";
          
          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;

          if (!self.GlobalVariables[ctx_].ContainsKey(args[1].toString(true)))
            return ret;
          if (self.GlobalVariables[ctx_][args[1].toString()].toString(true).Equals(args[2].toString(true)))
          {
            ret.w = 1.0f;
            return ret;
          }
        }
        return ret;
      };
      Parlogike.externFunctors["=|"] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
          if (args.Count < 2)
          {
            Console.WriteLine("Warning: gt  operator requires context, symbol name and value");
            return ret;
          }
          
          string ctx_="";
          string postfix = "";
          
          if (args.Count > 2 ){
            if (args[2].toNumber(true) < pattern.LocalStack.Count){
               postfix = pattern.LocalStack[(int)args[2].toNumber()];
            }
          }

          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;

          if (self.GlobalVariables[ctx_].ContainsKey(args[1].toString(true)+postfix)){
            ret.w=1.0f;
            return ret;
          }
        }
        return ret;
      };
      Parlogike.externFunctors["!=|"] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
          if (args.Count < 2)
          {
            Console.WriteLine("Warning: gt  operator requires context, symbol name and value");
            return ret;
          }

          string ctx_ = "";
          string postfix= "";
          
          if (args.Count > 2)
          {
            if (args[2].toNumber(true) < pattern.LocalStack.Count)
            {
              postfix = pattern.LocalStack[(int)args[2].toNumber()];
            }
          }

          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;

          if (!self.GlobalVariables[ctx_].ContainsKey(args[1].toString(true)+postfix)){
            ret.w=1.0f;
            return ret;
          }
        }
        return ret;
      };
      //scope, prefix, wildcard position 
      Parlogike.externFunctors["*!=|"] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
          if (args.Count < 3)
          {
            Console.WriteLine("Warning: exists *  operator requires context, symbol name and value");
            return ret;
          }
          if (pattern.LocalStack.Count <= args[2].toNumber(true)  ){
            return ret;
          }
          string ctx_ = "";
          string wildcard= pattern.LocalStack[(int)args[2].toNumber(true)];

          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;
          
          //Console.WriteLine("NE [{0}][{1}]",ctx_,(args[1].toString(true)+"-"+wildcard),1);

          if (!self.GlobalVariables[ctx_].
               ContainsKey((args[1].toString(true)+"-"+wildcard).Trim())){
            ret.w=0.1f;
            return ret;
          }
        }
        return ret;
      };
      //forget operator scope, prefix, wildcard
      Parlogike.externFunctors["*--"] = (Parlogike self, string input,
            List<Variable> args, char dir, bool mutate,
            Pattern pattern, string ctx, string extra) =>
          {
            Result ret = new Result();
            if (dir == 'r')
            {
              if (args.Count < 3)
              {
                Console.WriteLine("Warning: exists *  operator requires context, symbol name and value");
                return ret;
              }
              if (pattern.LocalStack.Count <= args[2].toNumber(true))
              {
                return ret;
              }
              string ctx_ = "";
              string wildcard = pattern.LocalStack[(int)args[2].toNumber(true)];

              if (args[0].toString(true).Equals("u"))
                ctx_ = ctx;
              string key_ =  (args[1].toString(true) + "-" + wildcard).Trim();

              if (self.GlobalVariables[ctx_].ContainsKey(key_))
              {
                self.GlobalVariables[ctx_].Remove(key_);
              }
            }
            return ret;
          };
      //scope, varname
      Parlogike.externFunctors["--"] = (Parlogike self, string input,
       List<Variable> args, char dir, bool mutate,
       Pattern pattern, string ctx, string extra) =>
     {
       Result ret = new Result();
       if (dir == 'r')
       {
         if (args.Count < 2)
         {
           Console.WriteLine("Warning: exists *  operator requires context, symbol name and value");
           return ret;
         }
         if (pattern.LocalStack.Count <= args[2].toNumber(true))
         {
           return ret;
         }

         string ctx_= "";         
         
         if (args[0].toString(true).Equals("u"))
           ctx_ = ctx;

         if (self.GlobalVariables[ctx_].ContainsKey(args[1].toString(true)))
         {
           self.GlobalVariables[ctx_].Remove(args[1].toString());
         }
       }
       return ret;
     };
      Parlogike.externFunctors["*=|"] = (Parlogike self, string input,
        List<Variable> args, char dir, bool mutate,
        Pattern pattern, string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
          if (args.Count < 3)
          {
            Console.WriteLine("Warning: exists *  operator requires context, symbol name and value");
            return ret;
          }
          if (pattern.LocalStack.Count <= args[2].toNumber(true)  ){
            return ret;
          }
          string ctx_ = "";
          string wildcard= pattern.LocalStack[(int)args[2].toNumber(true)];

          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;
          //Console.WriteLine("E [{0}][{1}]",ctx_,(args[1].toString(true)+"-"+wildcard));
          if (self.GlobalVariables[ctx_].
              ContainsKey((args[1].toString(true)+"-"+wildcard).Trim())){
            //Console.WriteLine("yep it contains " + (args[1].toString(true)+"-"+wildcard));
            ret.w=1.0f;
            return ret;
          }
        }
        return ret;
      };
 
      Parlogike.externFunctors["!="] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
          if (args.Count < 3)
          {
            Console.WriteLine("Warning: gt  operator requires context, symbol name and value");
            return ret;
          }
          string ctx_="";
          
          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;

          if (!self.GlobalVariables[ctx_].ContainsKey(args[1].toString(true))){
            ret.w=0.0f;
            return ret;
          }
          //Variable var = self.GlobalVariables[ctx_][args[1].toString()];
          if (!self.GlobalVariables[ctx_][args[1].toString()].toString(true).Equals( args[2].toString(true)))
          {
            //Console.WriteLine("passed");
            ret.w = 1.0f;
            return ret;
          }else{
              ret.w  = -1.0f;
              return ret;
          }
        }
        return ret;
      };
      Parlogike.externFunctors["&"] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern,string ctx,string extra) =>
      {
        Result ret = new Result();
        if (dir == 'r')
        {
          if (args.Count < 2)
          {
            Console.WriteLine("Warning: reference  operator requires context and symbol name");
            return ret;
          }
          string ctx_="";
          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;
          if (!self.GlobalVariables[ctx_].ContainsKey(args[1].toString(true))){
            return ret;
          }
          ret.s += "," + self.GlobalVariables[ctx_][args[1].toString()].toString() + " ";
        }
        return ret;
      };
      Parlogike.externFunctors["&+"] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx,string extra) =>
      {
        Result ret = new Result();
        if (dir == 'r')
        {
          if (args.Count < 3)
          {
            Console.WriteLine("Warning: reference sum requires context target, and source symbols name");
            return ret;
          }
          /*if (self->GlobalVariables.find(args[0].toString(true)) == self->GlobalVariables.end()||
              self->GlobalVariables.find(args[1].toString(true)) == self->GlobalVariables.end()
          )*/
          string ctx_ = "";
          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;
          
          if (self.GlobalVariables[ctx_].ContainsKey(args[1].toString(true)) ||
                    self.GlobalVariables[ctx_].ContainsKey(args[2].toString(true)))
            return ret;
          Variable v = self.GlobalVariables[ctx_][args[1].toString()];
          v.toNumber(true);
          v.fVal += self.GlobalVariables[ctx_][args[2].toString()].toNumber(true);
                //out+= self->GlobalVariables[args[0].toString()].toString()+" ";
                /*if ( self->GlobalVariables[args[0].toString()].toNumber(0) > args[1].toNumber(true))
                  return 1.0;*/
        }
        return ret;
     };
     Parlogike.externFunctors["@"] = (Parlogike self, string input, 
       List<Variable> args, char dir, bool mutate, 
       Pattern pattern, string ctx_, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
          foreach (var v in args)
          {
            if (!self.Groups.ContainsKey(v.toString(true)))
            {
                    //Console.WriteLine("Warning group {0} does not exist", v.toString());
                    continue;
            }
                  //cout << "testing" << input[n]<<" inside group" << v.toString() <<endl;
                  var group = self.Groups[v.toString()];
            for (int ind = 0; ind != group.Count; ind++)
            {
                    //cout << input[n] << " Vs "<< group[ind]  << endl;
                    if (input.Equals(group[ind]))
              {
                      //cout << "matched group ";
                      pattern.subjectMatched = true;
                ret.w = 1.0f;
                return ret;
              }
            }
          }
        }
        return ret;
      };
      Parlogike.externFunctors["->"] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern,string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
          foreach (var v in args)
          {
            if (v.toString(true).Equals(input))
            {
              pattern.subjectMatched = true;
              ret.w += 1.5f;
            }
          }
        }
        return ret;
      };
      Parlogike.externFunctors[" "] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
          foreach (var v in args)
          {
            if (v.toString(true).Equals(input))
            {
              ret.w = 1.0f;
              return ret;
            }
          }
        }
        if (dir == 'r')
        {

          foreach (var v in args)
          {
            ret.s += v.toString(true) + " ";
          }
        }
        return ret;
      };
      Parlogike.externFunctors["url"] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'i')
        {
                //cout << "testing" << input[n] <<endl;
                if (self.match("https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,4}\\b([-a-zA-Z0-9@:%_\\+.~#?&//=]*)", input))
          {
            ret.w = 1.0f;
            return ret;
          }
        }
        return ret;
      };
      Parlogike.externFunctors["*"] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern,string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'r')
        {
          if (pattern.LocalStack.Count == 0)
            return ret;
          if (args.Count > 0)
          {
            foreach (var v in args)
            {
              if ((int)v.toNumber(true) > (pattern.LocalStack.Count - 1))
                continue;
              ret.s += pattern.LocalStack[(int)v.toNumber()] + " ";
            }
          }
        }
        return ret;
      };
      /*scope,prefix,captured index  */
      Parlogike.externFunctors["*:="] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx,string extra) =>
      {
        Result ret = new Result();
        if (dir == 'r')
        {
          
          if (pattern.LocalStack.Count == 0)
            return ret;
          if (args.Count < 3)
          {
            Console.WriteLine("Warning at line {0}: *:= operator needs 3 arguments"
                             , pattern.input[0].line);
            return ret;
          }
          string ctx_="";
          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;
          if (args[2].toNumber(true) >= pattern.LocalStack.Count )
            return ret;
          string wildcard = pattern.LocalStack[(int)args[2].toNumber(true)];
          //Console.WriteLine("A [{0}][{1}]={2}",ctx_,(args[1].toString(true)+wildcard),1);
          if (args.Count == 3  ){
            self.GlobalVariables[ctx_][(args[1].toString(true)+"-"+wildcard).Trim()] = new Variable(1.0f) ;
          }
          else if (args.Count > 3)
          {
            if (args[3].toNumber(true) >= pattern.LocalStack.Count )
              return ret;
            self.GlobalVariables[ctx_]
              [(args[1].toString(true)+"-"+wildcard).Trim()] = 
                 new Variable(pattern.LocalStack[(int)args[3].toNumber()]) ;
           
          } 
        }
        return ret;
      };
      //scope, var name, index 
      Parlogike.externFunctors[":=*"] = (Parlogike self, string input, 
        List<Variable> args, char dir, bool mutate, 
        Pattern pattern, string ctx, string extra) =>
      {
        Result ret = new Result();
        if (dir == 'r')
        {
          
          if (pattern.LocalStack.Count == 0)
            return ret;
          if (args.Count < 3)
          {
            Console.WriteLine("Warning at line {0}: :=* operator needs 3 arguments"
                             , pattern.input[0].line);
            return ret;
          }
          string ctx_="";
          if (args[0].toString(true).Equals("u"))
            ctx_ = ctx;
          if (args[2].toNumber(true) >= pattern.LocalStack.Count )
            return ret;

          self.GlobalVariables[ctx_][args[1].toString(true)] =  new Variable(pattern.LocalStack[(int)args[2].toNumber()]);
        }
        return ret;
      };
    }
  };
};