using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Using Needed Imports, Ignore.
using ReplitForkScraper.Components;

namespace ReplitForkScraper
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string repl_slug = args[0];
            string wt_file = args[1];

            if (wt_file == "yes" || wt_file == "Yes" || wt_file == "y")
            {
                List<String> forks = await Scraper.GrabForks(repl_slug);

                string file_name = args[2];

                using (StreamWriter sr = new StreamWriter(file_name))
                {
                    sr.WriteLine(String.Join("\n", forks));
                }
            }
            else
            {
                List<String> forks = await Scraper.GrabForks(repl_slug);

                forks.ForEach(f =>
                {
                    Console.WriteLine(forks);
                });
            }
        }
    }
}

