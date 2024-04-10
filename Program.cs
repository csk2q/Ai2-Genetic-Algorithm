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

    private void GA()
    {
        Random rand = new();
        
        //Schedule, fitness
        PriorityQueue<Schedule, double> population = new(500);
        for (int i = 0; i < 500; i++)
        {
            var sch = Schedule.RandomizedSchedule(rand);
            population.Enqueue(sch, sch.Fitness());
        }

        var result = Schedule.Reproduce(population.Dequeue(), population.Dequeue(), rand);

    }
    
}