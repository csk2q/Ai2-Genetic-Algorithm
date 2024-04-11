using System.Diagnostics;
using Ai2_Genetic_Algorithm.Data;

namespace Ai2_Genetic_Algorithm;

class Program
{
    static void Main(string[] args) //=> new Program().Run();
    {
        Thread infoThread = new Thread(() => {
            while (true)
            {
                string input = Console.ReadLine() ?? "";
                if(mostSchedule is not null)
                {
                    Console.WriteLine(updateTime);
                    Console.WriteLine($"Max fit: {mostFit}");
                    if (input.Length > 0)
                    {
                        Console.WriteLine(mostSchedule);
                        mostSchedule.Compact().ForEach(Console.WriteLine);
                    }
                }
                else
                {
                    Console.WriteLine("No data yet.");
                }
            }
            
        });
        infoThread.Start();


        for (int i = 0; i < 10; i++)
        {
            new Thread(() =>
            {
                try
                {
                    new Program().Run();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }).Start();
        }
        
        
    }
    
    static double mostFit = Double.MinValue;
    static Schedule? mostSchedule = null!;
    static DateTime updateTime = DateTime.UnixEpoch;

    void Run()
    {
        Console.WriteLine("Program start.");

        /*
        Random rand = new();
        int randCounter = 0;
        Schedule schdule = null;
        
        double storedFitness = double.MinValue;
        uint counter = 0;
        while (counter < 100000)
        {
            var tempSchdule = Schedule.RandomizedSchedule(rand);
            var tempFit = tempSchdule.Fitness();
        
            if (tempFit > storedFitness)
            {
                storedFitness = tempFit;
                schdule = tempSchdule;
                // Console.WriteLine();
                Console.WriteLine(storedFitness);
                counter = 0;
            }
            else
            {
                // Console.Write('*');
                counter++;
            }

            // Reseed random after many uses
            if (++randCounter > 1000)
            {
                randCounter = 0;
                rand = new Random();
            }
        }
        Console.WriteLine();
        Console.WriteLine(schdule);
        Console.WriteLine(schdule!.Fitness());*/

        
        
        while (mostFit < 0)
        {
            GA();
        }


        Console.WriteLine($"Max fit: {mostFit}");
        Console.WriteLine(mostSchedule);
        mostSchedule.Compact().ForEach(Console.WriteLine);

        Console.WriteLine("Program exit.");
        Console.ReadLine();
    }

    //Schedule, fitness
    private Generic.PriorityQueue<Schedule, double> population;
    private void GA()
    {
        const int popSize = 1000;
        
        Random rand = new();
        population = new(popSize);
        double mutationRate = 0.1;
        
        //Create first gen
        for (int i = 0; i < popSize; i++)
        {
            var sch = Schedule.RandomizedSchedule(rand);
            population.Enqueue(sch, sch.Fitness());
        }

        //Evaluation > Reproduction > Crossover > Mutation
        Stopwatch stopwatch = Stopwatch.StartNew();
        ulong fullCounter = 0;
        int counter = 0;
        var growthPercent = double.MinValue;
        double lastMaxFit = GetMostFit(out _);
        do
        {
            
            
            
            //Remove least fit
            population.Dequeue();
            
            // Reproduce / Crossover
            var popCount = population._nodes.Length;
            var father = population._nodes[rand.Next(population._size)].Element;
            var mother = population._nodes[rand.Next(population._size)].Element;
            var child = Schedule.Reproduce(father, mother, rand);
            
            // Mutate
            if (rand.NextDouble() < mutationRate)
            {
                child = Schedule.Mutate(child, rand);
            }
            
            // Add to population
            var childSchedule = Schedule.Expand(child);
            population.Enqueue(childSchedule, childSchedule.Fitness());

            fullCounter++;
            counter++;
            if (counter > 100)
            {
                rand = new Random();
                counter = 0;
            }

            if (fullCounter % popSize == popSize - 1)
            {
                //Evaluate
                var maxFit = GetMostFit(out _);

                if (lastMaxFit == 0)
                {
                    Console.WriteLine("Avoiding divide by zero!");
                    lastMaxFit = -0.1;
                }
                
                growthPercent = (maxFit - lastMaxFit) / Math.Abs(lastMaxFit);

                
                stopwatch.Stop();
                //DEBUG LOG
                // Console.WriteLine($"Completed Generation {fullCounter/popSize} in {stopwatch.Elapsed.TotalMilliseconds} ms");
                // Console.WriteLine($"Improvement: {maxFit - lastMaxFit}, {growthPercent}%, Max fit: {maxFit}, Prev max fit: {lastMaxFit}");
                // Console.WriteLine($"Mutation rate: {mutationRate:G70}");

                if (growthPercent > 0)
                    mutationRate *= 0.9;
                if (growthPercent < 0.01 && fullCounter/popSize > 100)
                    break;
                
                lastMaxFit = maxFit;
                stopwatch.Restart();
            }
            
        } while (true);

        var mostFit = GetMostFit(out var schedule);
        //DEBUG LOG
        // Console.WriteLine();
        // Console.WriteLine($"Max fit: {mostFit}");
        // Console.WriteLine(schedule);
        // schedule.Compact().ForEach(Console.WriteLine);
        
        
        //DEBUG LOGS BELOW
        if (mostFit > Program.mostFit)
        {
            Program.mostFit = mostFit;
            Program.mostSchedule = schedule;
            Program.updateTime = DateTime.Now;
        }
    }

    private double GetMostFit(out Schedule schedule)
    {
        double maxFit = double.MinValue;
        schedule = null!;
        
        foreach (var populationUnorderedItem in population.UnorderedItems)
        {
            if (populationUnorderedItem.Priority > maxFit)
            {
                schedule = populationUnorderedItem.Element;
                maxFit = populationUnorderedItem.Priority;
            }
        }

        return maxFit;
    }
    
}