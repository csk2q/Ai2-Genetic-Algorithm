namespace Ai2_Genetic_Algorithm.Data;

public static class TestData
{
    //Make enum? name = capacity?
    public static readonly List<Room> Rooms = [
        new Room("Slater 003", 45),
        new Room("Roman 216", 30),
        new Room("Loft 206", 75),
        new Room("Roman 201", 50),
        new Room("Loft 310", 108),
        new Room("Beach 201", 60),
        new Room("Beach 301", 75),
        new Room("Logos 325", 450),
        new Room("Frank 119", 60)
    ];

}

public enum Facilitators
    {
        Lock, Glen, Banks, Richards, Shaw, Singer, Uther, Tyler, Numen, Zeldin
    }

public enum TimeSlot
{
    _10AM,
    _11AM,
    _12PM,
    _1PM,
    _2PM,
    _3PM
}

public struct Room(string name, int capacity)
{
    public readonly string name = name;
    public readonly int capacity = capacity;
}

public class Activity(string name, int expEnroll, Facilitators[] preferred, Facilitators[] other)
{
    public readonly string name = name;
    public readonly int expectedEnrollment = expEnroll;
    public readonly Facilitators[] preferredFacilitators = preferred;
    public readonly Facilitators[] otherFacilitators = other;
}


    
    
