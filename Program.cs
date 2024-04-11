using System.Diagnostics;
using Ai2_Genetic_Algorithm.Data;

namespace Ai2_Genetic_Algorithm;

class Program
{
    static void Main(string[] args) => new Program().Run();

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
        
        GA();
        

        Console.WriteLine("Program exit.");
    }

    //Schedule, fitness
    private Generic.PriorityQueue<Schedule, double> population;
    private void GA()
    {
        const int popSize = 500;
        
        Random rand = new();
        population = new(popSize);
        double mutationRate = 0.01; // 1%
        
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
                Console.WriteLine($"Completed Generation {fullCounter/popSize:G6} in {stopwatch.Elapsed.TotalMilliseconds:G6} ms");
                Console.WriteLine($"Improvement: {maxFit - lastMaxFit:G6}, {growthPercent:G6}%, Max fit: {maxFit}, Prev max fit: {lastMaxFit:G6}");

                // Reduce mutation rate if improving
                if (growthPercent > 0)
                    mutationRate *= 0.9;
                Console.WriteLine($"Mutation rate: {mutationRate:G6}");

                if (growthPercent < 0.01 && fullCounter/popSize > 100)
                    break;
                
                lastMaxFit = maxFit;
                stopwatch.Restart();
            }
            
        } while (true);

        Console.WriteLine();
        Console.WriteLine("Max fit:");
        _ = GetMostFit(out var schedule);
        // Console.WriteLine(schedule);
        schedule.Compact().ForEach(Console.WriteLine);
        


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