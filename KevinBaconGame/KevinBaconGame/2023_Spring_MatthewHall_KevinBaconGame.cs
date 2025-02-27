using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace kb;

class Program
{
    public static void Main(string[] args)
    {
        //makes sure command line has the text file to read from
        if (args.Length == 0)
        {
            Console.WriteLine("Program requires at least 1 parameter");
            return;
        }

        //I decided to do the stopwatch to see how long it takes my program to read and enter all the names into the database
        Stopwatch readTimer = Stopwatch.StartNew();

        readTimer.Start();
        string fileName = args[0];

        //initialize my array of lines and my temporary list for names
        string[] lines = File.ReadAllLines(fileName);
        List<string> temp = new List<string>();

        //loop to add names to temp
        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split('|'); //split each line into 2 names: one actor and one movie

            if (parts[0].Contains('('))
            {
                temp.Add(parts[0].Remove(parts[0].IndexOf('(') - 1)); //if the name of the actor or movie has any () in it, they will be snipped
            }
            else
            {
                temp.Add(parts[0]);
            }

            if (parts[1].Contains('('))
            {
                temp.Add(parts[1].Remove(parts[1].IndexOf('(') - 1)); //if the name of the actor or movie has any () in it, they will be snipped
            }
            else
            {
                temp.Add(parts[1]);
            }
        }

        //turn the list into an array so I can easily add all the vertexes into my graph
        string[] vertex = temp.ToArray();

        MathGraph<string> graph = new MathGraph<string>();

        //add each vertex as long as the graph does not already contain the vertex
        foreach (var item in vertex)
        {
            if (!graph.ContainsVertex(item))
            {
                graph.AddVertex(item);
            }
        }

        //uses the vertex array to add edges between each pair of actor and movie
        for (int i = 0; i < vertex.Length - 2; i += 2)
        {
            graph.AddEdge(vertex[i], vertex[i + 1]);
        }

        //after the timer stops, check to see if the user input the full path, and if they did return the name of only the txt file after the last set of \\
        readTimer.Stop();
        if (fileName.Contains("\\"))
        {
            string fileShortened = fileName.Remove(0, fileName.LastIndexOf('\\') + 1);
            Console.WriteLine($"Read '{fileShortened}' in {readTimer.ElapsedMilliseconds}ms");
        }
        else
        {
            Console.WriteLine($"Read '{fileName}' in {readTimer.ElapsedMilliseconds}ms");
        }

        //game loop bool
        bool game = true;

        //check if there is only the txt argument, and if there is, do a manual entry, but if not, continue program with all 3 args
        if (args.Length == 1)
        {
            Console.WriteLine("Please seperate names with '&'\nExample: Harrison Ford & Bobbert"); //tells user what the seperating character is
            while (game)
            {
                Console.Write("> "); //line prompt
                string entry = Console.ReadLine()!;

                //exit function
                if (entry.ToUpper() == "QUIT" || entry.ToUpper() == "EXIT")
                {
                    return;
                }
                else
                {
                    //checks to make sure user input correct seperator character
                    if (!entry.Contains('&'))
                    {
                        Console.WriteLine("Please use '&' to seperate celebrities");
                        return;
                    }

                    //seperates the ReadLine string into two strings, one for each actor
                    string[] entries = Regex.Split(entry, @" & ");
                    //found Regex.Split from stackoverflow because I am lazy so I wanted to be able to seperate the strings using the & and spaces

                    //checking to make sure there are a correct amount of entries
                    if (entries.Length > 2)
                    {
                        Console.WriteLine("Please only input 2 celebrities");
                        return;
                    }
                    else if (entries.Length == 1)
                    {
                        Console.WriteLine("Please enter 2 celebrity names");
                        return;
                    }
                    //if the number of entries are correct, move on
                    else
                    {
                        //first checks to see that both names are in the database
                        if (!graph.ContainsVertex(entries[0]) || !graph.ContainsVertex(entries[1]))
                        {
                            if (!graph.ContainsVertex(entries[0]))
                            {
                                Console.WriteLine($"{entries[0]} is not on the list");
                                return;
                            }
                            else if (!graph.ContainsVertex(entries[1]))
                            {
                                Console.WriteLine($"{entries[1]} is not on the list");
                                return;
                            }
                        }
                        //then checks to see if there is a connection between both entries
                        else if (!graph.TestConnectedTo(entries[0], entries[1]))
                        {
                            Console.WriteLine($"{entries[0]} is not connected to {entries[1]}");
                            return;
                        }
                        //if both parameters are met, find shortest path
                        else
                        {
                            List<string> edges = graph.FindShortestPath(entries[0], entries[1]);

                            //add an interesting fact about both
                            Console.WriteLine($"Interesting fact: {entries[0]} is in {graph.CountAdjacent(entries[0])} movie(s) in this database");
                            Console.WriteLine($"Interesting fact: {entries[1]} is in {graph.CountAdjacent(entries[1])} movie(s) in this database");

                            //show how many steps it took to get the path between the two actors using the formula : actor movie actor
                            Console.WriteLine($"{edges.Count / 2} degrees of seperation between '{entries[0]}' and '{entries[1]}':");
                            for (int i = 1; i < edges.Count - 1; i += 2)
                            {
                                Console.WriteLine($"{edges[i - 1]} was in {edges[i]} with {edges[i + 1]}");
                            }
                        }
                    }
                }
            }
        }
        //if there are more than the txt parameter then use the automatic path maker, same program when using entries[], just now with args[]
        else
        {
            if (args.Length > 3)
            {
                Console.WriteLine("Please only input 2 celebrities");
                return;
            }
            else if (args.Length == 2)
            {
                Console.WriteLine("Please enter 2 celebrity names");
                return;
            }
            else
            {
                if (!graph.ContainsVertex(args[1]) || !graph.ContainsVertex(args[2]))
                {
                    Console.WriteLine($"{args[1]} is not connected to {args[2]}");
                    if (!graph.ContainsVertex(args[1]))
                    {
                        Console.WriteLine($"{args[1]} is not on the list");
                        return;
                    }
                    if (!graph.ContainsVertex(args[2]))
                    {
                        Console.WriteLine($"{args[2]} is not on the list");
                        return;
                    }
                }
                else if (!graph.TestConnectedTo(args[1], args[2]))
                {
                    Console.WriteLine($"{args[1]} is not connected to {args[2]}");
                    return;
                }
                else
                {
                    List<string> edges = graph.FindShortestPath(args[1], args[2]);

                    Console.WriteLine($"Interesting fact: {args[1]} is in {graph.CountAdjacent(args[1])} movie(s) in this database");
                    Console.WriteLine($"Interesting fact: {args[2]} is in {graph.CountAdjacent(args[2])} movie(s) in this database");
                    Console.WriteLine($"{edges.Count/2} degrees of seperation between '{args[1]}' and '{args[2]}':");
                    for (int i = 1; i < edges.Count - 1; i+=2)
                    {
                        Console.WriteLine($"{edges[i-1]} was in {edges[i]} with {edges[i+1]}");
                    }
                }
            }
        }
        
    }
}