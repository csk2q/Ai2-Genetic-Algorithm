using System.Collections.Immutable;

using static Ai2_Genetic_Algorithm.Data.Facilitators;

namespace Ai2_Genetic_Algorithm.Data;

public static class TestData
{
    //Make enum? name = capacity?
    public static readonly ImmutableList<Room> Rooms = [
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

    public static readonly ImmutableList<Activity> Activities = [
        new Activity("SLA100A", 50, [Glen, Lock, Banks, Zeldin], [Numen, Richards]),
        new Activity("SLA100B", 50, [Glen, Lock, Banks, Zeldin], [Numen, Richards]),
        new Activity("SLA191A", 50, [Glen, Lock, Banks, Zeldin], [Numen, Richards]),
        new Activity("SLA191B", 50, [Glen, Lock, Banks, Zeldin], [Numen, Richards]),
        new Activity("SLA201", 50, [Glen, Banks, Zeldin, Shaw], [Numen, Richards, Singer]),
        new Activity("SLA291", 50, [Lock, Banks, Zeldin, Singer], [Numen, Richards, Shaw, Tyler]),
        new Activity("SLA303", 60, [Glen, Zeldin, Banks], [Numen, Singer, Shaw]),
        new Activity("SLA304", 25, [Glen, Banks, Tyler], [Numen, Singer, Shaw, Richards, Uther, Zeldin]),
        new Activity("SLA394", 20, [Tyler, Singer], [Richards, Zeldin]),
        new Activity("SLA449", 60, [Tyler, Singer, Shaw], [Zeldin, Uther]),
        new Activity("SLA451", 100, [Tyler, Singer, Shaw], [Zeldin, Uther, Richards, Banks])
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


    
    
