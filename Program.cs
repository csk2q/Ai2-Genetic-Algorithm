using System.Diagnostics;
using Ai2_Genetic_Algorithm.Data;

namespace Ai2_Genetic_Algorithm;

class Program
{
    static void Main(string[] args) => new Program().Run();

    void Run()
    {
        Console.WriteLine("Program start.");

        // Run genetic algorithm
        var bestResult = GA();

        //Bulk run GA
        /*for (int i = 0; i < 100; i++)
        {
            var tempResult = GA();
            if (tempResult.fitness > bestResult.fitness)
                bestResult = tempResult;
        }
        Console.WriteLine();
        Console.WriteLine($"Max fit: {bestResult.fitness:G5}");
        Console.WriteLine(bestResult.schedule);
        bestResult.schedule.Compact().ForEach(Console.WriteLine);*/

        Console.WriteLine("Program exit.");
    }

    //Schedule, fitness
    private Generic.PriorityQueue<Schedule, double> population;

    private (double fitness, Schedule schedule) GA()
    {
        const int popSize = 500;
        const double highMutationRate = 0.01;
        const double lowMutationRate = 0.001;
        const bool reportPerGeneration = true;
        const bool reportAtEnd = true;

        Random rand = new();
        population = new(popSize);
        double mutationRate = 0.01;

        //Create first gen
        for (int i = 0; i < popSize; i++)
        {
            var sch = Schedule.RandomizedSchedule(rand);
            population.Enqueue(sch, sch.Fitness());
        }

        
        // Start timer
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        //Set up values
        int counter = 0;
        var growthPercent = double.MinValue;
        double lastMaxFit = GetMostFit(out _);
        HashSet<Schedule> children = new HashSet<Schedule>(popSize / 2);
        
        // Evaluation > Reproduction > Crossover > Mutation
        do
        {
            //Remove least fit half of population
            for (int i = 0; i < popSize / 2; i++)
                population.Dequeue();

            // Remove old data
            children.Clear();

            // Reproduce / Crossover
            while (children.Count < popSize / 2)
            {
                var popCount = population._nodes.Length;
                var father = population._nodes[rand.Next(population._size)].Element;
                var mother = population._nodes[rand.Next(population._size)].Element;
                var newChildren = Schedule.Reproduce(father, mother, rand);

                // Mutate
                for (int i = 0; i < newChildren.Count; i++)
                {
                    if (rand.NextDouble() < mutationRate)
                        newChildren[i] = Schedule.Mutate(newChildren[i], rand);
                }

                // Store new children
                foreach (var newChild in newChildren)
                    children.Add(Schedule.Expand(newChild));
            }

            // Add new children to the population
            foreach (var child in children)
            {
                population.Enqueue(child, child.Fitness());
            }

            //Counter work
            counter++;

            // Reseed random occasionally 
            if (counter % 10 == 0)
                rand = new Random();

            // Evaluate generation
            var maxFit = GetMostFit(out _);
            growthPercent = (maxFit - lastMaxFit) / lastMaxFit;
            
            if (reportPerGeneration)
            {
                // Stop timer to enable reading
                stopwatch.Stop();
                
                //Report generation improvement
                Console.WriteLine($"Completed Generation {counter} in {stopwatch.Elapsed.TotalMilliseconds:G6} ms");
                Console.WriteLine(
                    $"Improvement: {maxFit - lastMaxFit:G6}, {growthPercent:G3}%, Max fit: {maxFit:G7}, Prev max fit: {lastMaxFit:G7}, Mutation rate: {mutationRate:G6}");
            }
            
            // Clean up steps

            // TODO change to short bursts of mutation followed by long periods of low mutation
            // Reduce mutation rate if improving
            // if (growthPercent > 0)
            //     mutationRate *= 0.9;

            // Short bursts of higher mutation
            // Values are arbitrary
            if (counter % 20 < 4)
                mutationRate = highMutationRate;
            else
                mutationRate = lowMutationRate;            

            //Update last fit
            lastMaxFit = maxFit;
            
            if(reportPerGeneration)
                //Restart timer
                stopwatch.Restart();
            
        } while (counter < 100 || growthPercent > 0.01);

        // Final results
        var endMaxFit = GetMostFit(out var schedule);
        if (reportAtEnd)
        {
            Console.WriteLine();
            Console.Write($"Max fit: {endMaxFit:G5}");
            if (!reportPerGeneration)
                Console.Write($" in {stopwatch.Elapsed.TotalMilliseconds:G6} ms");
            Console.WriteLine();
            // Console.WriteLine(schedule);
            schedule.Compact().ForEach(Console.WriteLine);
        }

        return (endMaxFit, schedule);
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

    // Pure random fishing for schedules
    private void RandomFishing()
    {
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
       Console.WriteLine(schdule!.Fitness());
    }
}