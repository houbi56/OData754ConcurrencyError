using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var c = new HttpClient();

            var url = "http://localhost:5000/odata/ODataEntity?$select=Id,SomeOtherProperty";
            int nrOfTries = 20;

            for(int i = 0; i < nrOfTries; i++)
            {
                var r = await c.GetAsync(url);
                Console.WriteLine($"{i} {r.StatusCode}");
            }

            Parallel.ForEach(Enumerable.Range(1, nrOfTries), async (i) =>
            {
                try
                {
                    var r = await c.GetAsync(url);
                    Console.WriteLine($"{i} {r.StatusCode}");
                }
                catch(Exception e)
                {
                    Console.WriteLine($"{i} {e.Message}");
                }
                
                
            });

            Console.Read();
        }
    }
}
