namespace Parlogike_
{
    public class BotIface
    {
       private Parlogike parlogike;
       public BotIface (string corpus){
           parlogike = new Parlogike();
           parlogike.parse(corpus);
           parlogike.executeDirectives(true);
       }
       public string respond (string input){
          return parlogike.respond(input, true,"test");
       } 
      
    };
}