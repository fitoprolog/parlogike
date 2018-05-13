using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Parlogike_;
using webapi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace webapi.Controllers
{
  [Route("api/[controller]")]
  public class ChatController : Controller
  {

    public readonly TestDataContext _context;  
    public static int visits = 0;
    public static volatile Parlogike bot;

    public ChatController(TestDataContext context)
    {
      _context = context;
    }
    public static bool botstarted = false;
    // GET api/values
    [HttpGet]
    public string Get(){

       string ret=":D\n"; 
       //var dic = bot.GlobalVariables;
       foreach(var k in bot.GlobalVariables["testuid"]){
         ret += k.Key + "=>" + k.Value +"\n" ;
       }
       /*int count = HttpContext.Session.GetInt32("counter") ?? 0;
       count++;
       HttpContext.Session.SetInt32("counter",count);
       return ""+count ;*/
       return ret;
    }

    /**
     this defines only the route but doesn't bind against any variable 
     */
    [Route("create/")]
    /*
       Here the FromQuery attribute implies that the value members 
       gonna be read like this example:
       host/api/[controller]/?member0=value0&member1=value1...&
     */
    public IActionResult Get( [FromQuery] TestData item){
           
      /*_context.Add(item);
      _context.SaveChanges();*/
      if (!ModelState.IsValid){
          return BadRequest(ModelState);
      }
      Console.WriteLine("numeric : {0}, nomimal:{1}",item.numeric,item.nominal);
      return Ok("works");
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
       TestData item  = _context.testElements.Find(id);
       if (item == null)
         return NotFound();
       return Ok(item.nominal);
    }

    // GET api/values/5
    [Route("chat/{message}")]
    public string Get(string message)
    {
      if (!botstarted)
      {
        Console.WriteLine("parsing");
        bot = new Parlogike();
        bot.parse("bots/ivanka.plk");
        bot.executeDirectives(true);
        //bot.respond("warmup",true,"testuid");
        botstarted = true;
      }
      return bot.respond(message, true,"testuid");
    }

    // POST api/values
    [HttpPost]
    public void Post([FromBody]string value)
    {
    }

    // PUT api/values/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/values/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
    
    [HttpPost("UploadFiles")]
    public async Task<IActionResult> Post()
    {
        long size_=0;
        var files = HttpContext.Request.Form.Files;
        Console.Write("nfiles :{0}",files.Count);
        for(int f=0; files.Count != f; f++){
            Console.WriteLine(size_+=files[f].Length);
        }

        return Ok(new {size=size_});
    }
  }
}
