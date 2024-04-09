using System.Collections.Immutable;

using static Ai2_Genetic_Algorithm.Data.Facilitator;

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
        //NOTE: SLA100A/B is renamed to SLA101A/B since it referenced as SLA 101 A/B in the fitness function description.
        new Activity("SLA101A", 50, [Glen, Lock, Banks, Zeldin], [Numen, Richards]),
        new Activity("SLA101B", 50, [Glen, Lock, Banks, Zeldin], [Numen, Richards]),
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
    
    public static readonly Activity SLA101A = Activities[0];
    public static readonly Activity SLA101B = Activities[1];
    public static readonly Activity SLA191A = Activities[2];
    public static readonly Activity SLA191B = Activities[3];
    public static readonly Activity SLA201 = Activities[4];
    public static readonly Activity SLA291 = Activities[5];
    public static readonly Activity SLA303 = Activities[6];
    public static readonly Activity SLA304 = Activities[7];
    public static readonly Activity SLA394 = Activities[8];
    public static readonly Activity SLA449 = Activities[9];
    public static readonly Activity SLA451 = Activities[10];
    
    public const int TimeSlotCount = 6; //Enum.GetNames(typeof(TimeSlot)).Length;
    public const int FacilitatorCount = 10; //Enum.GetNames(typeof(Facilitator)).Length;
}

public enum Facilitator
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

public class Activity(string name, int expEnroll, Facilitator[] preferred, Facilitator[] other)
{
    public readonly string name = name;
    public readonly int expectedEnrollment = expEnroll;
    public readonly Facilitator[] preferredFacilitators = preferred;
    public readonly Facilitator[] otherFacilitators = other;
}


    
    
