using Ai2_Genetic_Algorithm.Data;

namespace Ai2_Genetic_Algorithm;

class Program
{
    static void Main(string[] args) => new Program().Run();

    void Run()
    {
        Console.WriteLine("Program start.");

        Random rand = new();
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
                Console.WriteLine();
                Console.WriteLine(storedFitness);
                counter = 0;
            }
            else
            {
                Console.Write('*');
                counter++;
            }
        }
        Console.WriteLine();
        Console.WriteLine(schdule);
        Console.WriteLine(schdule!.Fitness());
        

        Console.WriteLine("Program exit.");
    }
    
}