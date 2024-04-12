using System.Diagnostics;

namespace Ai2_Genetic_Algorithm.Data;

using Data;
using static Data.Facilitator;
using static Data.TestData;

public class Slot(Activity activity, Facilitator facilitator)
{
    public Activity activity = activity;
    public Facilitator facilitator = facilitator;

    public override string ToString()
    {
        return activity.name + "/" + facilitator;
    }
}

public class Schedule
{
    //Add caching of the fitness value.
    //* Without a reliable way invalidate on change fitness cannot be cached. *//
    // private int fitness = 0;

    //3D array of activities
    //Room index, TimeSlot index, List of activities
    private List<Slot>[][] allSlots;


    public Schedule()
    {
        allSlots = new List<Slot>[Rooms.Count][];
        for (int i = 0; i < allSlots.Length; i++)
        {
            allSlots[i] = new List<Slot>[TimeSlotCount];
            for (int j = 0; j < TimeSlotCount; j++)
                allSlots[i][j] = new List<Slot>();
        }
    }

    public static Schedule RandomizedSchedule(in Random rand)
    {
        Schedule sch = new();

        foreach (var activity in TestData.Activities)
        {
            int room = rand.Next(0, sch.allSlots.Length);
            int timeSlot = rand.Next(0, TimeSlotCount);
            Facilitator facilitator = (Facilitator)rand.Next(0, TestData.FacilitatorCount);
            sch.allSlots[room][timeSlot].Add(new Slot(activity, facilitator));
        }

        return sch;
    }

    public static List<List<ActivityData>> Reproduce(in Schedule father, in Schedule mother, in Random random)
    {
        // Set up
        // The -1 is to ensure we do not just duplicate the father
        int splitIndex = random.Next(0, Activities.Count - 1);
        var fatherSimple = father.Compact();
        var motherSimple = mother.Compact();

        Dictionary<string, ActivityData> sonDictionary = [];
        Dictionary<string, ActivityData> daughterDictionary = [];
        
        // Add data from father
        for (int i = 0; i < fatherSimple.Count; i++)
        {
            if (i <= splitIndex)
                sonDictionary.Add(fatherSimple[i].act.name, fatherSimple[i]);
            else
                daughterDictionary.Add(fatherSimple[i].act.name, fatherSimple[i]);
        }

        // Add rest of data from mother
        foreach (var motherData in motherSimple)
        {
            _ = sonDictionary.TryAdd(motherData.act.name, motherData);
            _ = daughterDictionary.TryAdd(motherData.act.name, motherData);
        }

        var sonSimple = sonDictionary.Values.ToList();
        var daughterSimple = daughterDictionary.Values.ToList();

        // Activity count should be unchanged.
        Debug.Assert(sonSimple.Count == Activities.Count,
            $"Activity count for son is incorrect! Expected {Activities.Count} but got {sonSimple.Count}");
        Debug.Assert(daughterSimple.Count == Activities.Count,
            $"Activity count for son is incorrect! Expected {Activities.Count} but got {daughterSimple.Count}");

        return [sonSimple, daughterSimple];
    }

    public static List<ActivityData> Mutate(List<ActivityData> activityData, in Random random)
    {
        int removeIndex = random.Next(0, Activities.Count);

        switch (random.Next(3))
        {
            case 0: // facilitator
                activityData[removeIndex] = activityData[removeIndex] with
                {
                    facilitator = (Facilitator)random.Next(0, FacilitatorCount)
                };
                break;

            case 1: // timeslotId
                activityData[removeIndex] =
                    activityData[removeIndex] with { timeslotId = random.Next(0, TimeSlotCount) };
                break;

            case 2: // roomId
                activityData[removeIndex] = activityData[removeIndex] with { roomId = random.Next(0, Rooms.Count) };
                break;
        }

        return activityData;
    }

    public record ActivityData(Activity act, Facilitator facilitator, int timeslotId, int roomId);

    public List<ActivityData> Compact() => Compact(this);

    public static List<ActivityData> Compact(in Schedule schedule)
    {
        List<ActivityData> actData = new();

        //Room
        for (int roomId = 0; roomId < schedule.allSlots.Length; roomId++)
        {
            //Timeslot
            for (int timeslotId = 0; timeslotId < schedule.allSlots[roomId].Length; timeslotId++)
            {
                foreach (Slot slot in schedule.allSlots[roomId][timeslotId])
                {
                    actData.Add(new(slot.activity, slot.facilitator, timeslotId, roomId));
                }
            }
        }

        return actData;
    }

    public static Schedule Expand(in List<ActivityData> activityData)
    {
        Schedule schedule = new();

        foreach (var actData in activityData)
        {
            schedule.allSlots[actData.roomId][actData.timeslotId].Add(new Slot(actData.act, actData.facilitator));
        }

        return schedule;
    }

    public double Fitness()
    {
        // Note: Implementation focuses on aligning closely the requirement's wording over performance.


        // For each activity, fitness starts at 0.
        double fitness = 0;

        //Facilitator load data
        Dictionary<Facilitator, int> facilitatorLoad = [];
        foreach (var faclitator in Enum.GetValues<Facilitator>())
            facilitatorLoad[faclitator] = 0;

        //Mapping data
        Dictionary<Activity, TimeSlot> activityToTimeSlot = [];
        Dictionary<Activity, Room> activityToRoom = [];


        // For every room
        for (int roomIndex = 0; roomIndex < allSlots.Length; roomIndex++)
        {
            var roomCapacity = Rooms[roomIndex].capacity;

            // For every timeslot for a room
            foreach (var slots in allSlots[roomIndex])
            {
                // Activity is scheduled at the same time in the same room as another of the activities: -0.5
                // Eg: 2 activities in the same room is -1.0 to fitness
                if (slots.Count > 0)
                    fitness -= (slots.Count - 1) * 0.5;

                //For each slot in the timeslot
                foreach (var slot in slots)
                {
                    //Room size:
                    // ◦ Activities is in a room too small for its expected enrollment: -0.5
                    // ◦ Activities is in a room with capacity > 3 times expected enrollment: -0.2
                    // ◦ Activities is in a room with capacity > 6 times expected enrollment: -0.4
                    // ◦ Otherwise + 0.3
                    if (roomCapacity < slot.activity.expectedEnrollment)
                        fitness -= 0.5;
                    else if (roomCapacity > slot.activity.expectedEnrollment * 3)
                        fitness -= 0.2;
                    else if (roomCapacity > slot.activity.expectedEnrollment * 6)
                        fitness -= 0.4;
                    else
                        fitness += 0.3;

                    // • Activities is overseen by a preferred facilitator: + 0.5
                    // • Activities is overseen by another facilitator listed for that activity: +0.2
                    // • Activities is overseen by some other facilitator: -0.1
                    if (slot.activity.preferredFacilitators.Contains(slot.facilitator))
                        fitness += 0.5;
                    else if (slot.activity.otherFacilitators.Contains(slot.facilitator))
                        fitness += 0.2;
                    else
                        fitness -= 0.1;

                    //Track total facilitator load
                    facilitatorLoad[slot.facilitator] += 1;
                }
            }
        }

        //For every timeslot
        for (int timeslotId = 0; timeslotId < TimeSlotCount; timeslotId++)
        {
            // Track facilitator load per timeslot
            Dictionary<Facilitator, int> facilitatorLoadTimeslot = [];
            foreach (var facilitator in Enum.GetValues<Facilitator>())
                facilitatorLoadTimeslot[facilitator] = 0;

            // For every room
            for (int roomIndex = 0; roomIndex < allSlots.Length; roomIndex++)
            {
                //For each slot in the timeslot
                foreach (var slot in allSlots[roomIndex][timeslotId])
                {
                    // Increment facilitator load
                    facilitatorLoadTimeslot[slot.facilitator] += 1;

                    // Keep track of mappings
                    activityToTimeSlot.Add(slot.activity, (TimeSlot)timeslotId);
                    activityToRoom.Add(slot.activity, Rooms[roomIndex]);
                }
            }

            // Facilitator load per-timeslot:
            // ◦ Activity facilitator is scheduled for only 1 activity in this time slot: + 0.2
            // ◦ Activity facilitator is scheduled for more than one activity at the same time: - 0.2
            foreach (var activityCount in facilitatorLoadTimeslot.Values)
            {
                if (activityCount == 1)
                    fitness += 0.2;
                else if (activityCount > 1)
                    fitness -= 0.2;
            }
        }


        // Total Facilitator load:
        // ◦ Facilitator is scheduled to oversee more than 4 activities total: -0.5
        // ◦ Facilitator is scheduled to oversee 1 or 2 activities*: -0.4
        //      ▪ Exception: Dr. Tyler is committee chair and has other demands on his time.
        //          *No penalty if he’s only required to oversee < 2 activities.
        
        // TODO Ask if facilitator checking is correct?
        // Optimal is a facilitator teaching 3 classes and Tyler teaching 1 or 0?

        // Get list of facilitator minus Dr. Tyler
        var normalFacilitators = Enum.GetValues<Facilitator>().ToList();
        normalFacilitators.Remove(Facilitator.Tyler);
        
        foreach (var facilitator in normalFacilitators)
        {
            if (facilitatorLoad[facilitator] > 4)
                fitness -= 0.5;
            else if (facilitatorLoad[facilitator] == 1 || facilitatorLoad[facilitator] == 2)
                fitness -= 0.4;
        }

        // Penalty if Dr. Tyler is teaching more than one class.
        if (facilitatorLoad[Tyler] > 4)
            fitness -= 0.4;
        else if (facilitatorLoad[Tyler] >= 2)
            fitness -= 0.4;
        
        // Facilitator load continued
        // ◦ If any facilitator scheduled for _consecutive_ time slots: Same rules as for SLA 191 and SLA 101 in consecutive time slots—see below.
        //      A section of SLA 191 and a section of SLA 101 are overseen in consecutive time slots (e.g., 10 AM & 11 AM): +0.5
        //       ◦ In this case only (consecutive time slots), one of the activities is in Roman or Beach, and the other isn’t: -0.4
        //           ▪ It’s fine if neither is in one of those buildings, of activity; we just want to avoid having consecutive activities being widely separated.

        //TODO implement the above check?


        // Activity-specific adjustments:
        // • The 2 sections of SLA 101 are more than 4 hours apart: + 0.5
        // • Both sections of SLA 101 are in the same time slot: -0.5
        //
        // • The 2 sections of SLA 191 are more than 4 hours apart: + 0.5
        // • Both sections of SLA 191 are in the same time slot: -0.5
        //
        // • A section of SLA 191 and a section of SLA 101 are overseen in consecutive time slots (e.g., 10 AM & 11 AM): +0.5
        //   ◦ In this case only (consecutive time slots), one of the activities is in Roman or Beach, and the other isn’t: -0.4
        //      ▪ It’s fine if neither is in one of those buildings, of activity; we just want to avoid having consecutive activities being widely separated.
        //
        // • A section of SLA 191 and a section of SLA 101 are taught separated by 1 hour (e.g., 10 AM & 12:00 Noon): + 0.25
        // • A section of SLA 191 and a section of SLA 101 are taught in the same time slot: -0.25

        // Check time difference between parts A&B of SLA101
        var SLA101Diff = TimeDiff(activityToTimeSlot[SLA101A], activityToTimeSlot[SLA101B]);
        if (SLA101Diff > 4)
            fitness += 0.5;
        else if (SLA101Diff == 0)
            fitness -= 0.5;

        // Check time difference between parts A&B of SLA191
        var SLA191Diff = TimeDiff(activityToTimeSlot[SLA191A], activityToTimeSlot[SLA191B]);
        if (SLA191Diff > 4)
            fitness += 0.5;
        else if (SLA191Diff == 0)
            fitness -= 0.5;

        // Calculate time differences between SLA191A/B and SLA101A/B
        var SLA191Ato101ADiff = TimeDiff(activityToTimeSlot[SLA191A], activityToTimeSlot[SLA101A]);
        var SLA191Ato101BDiff = TimeDiff(activityToTimeSlot[SLA191A], activityToTimeSlot[SLA101B]);
        var SLA191Bto101ADiff = TimeDiff(activityToTimeSlot[SLA191B], activityToTimeSlot[SLA101A]);
        var SLA191Bto101BDiff = TimeDiff(activityToTimeSlot[SLA191B], activityToTimeSlot[SLA101B]);

        // Lambda function for calculating the difference and fitness adjustment
        var Check191to101 = (int timeDiff, Activity A, Activity B) =>
        {
            // A time difference of one means they are sequential
            if (timeDiff == 1)
            {
                if (activityToRoom[A].name.StartsWith("Roman"))
                {
                    if (activityToRoom[B].name.StartsWith("Roman"))
                        fitness += 0.5;
                    else
                        fitness -= 0.4;
                }
                else if (activityToRoom[A].name.StartsWith("Beach"))
                {
                    if (activityToRoom[B].name.StartsWith("Beach"))
                        fitness += 0.5;
                    else
                        fitness -= 0.4;
                }
            }
            // A time difference of two means one hour gap between classes.
            else if (timeDiff == 2) 
            {
                fitness += 0.25;
            }
            // A time difference of zero means the classes are in the same timeslot.
            else if (timeDiff == 0)
                fitness -= 0.25;
        };

        // Run lambda function for all combinations
        Check191to101(SLA191Ato101ADiff, SLA191A, SLA101A);
        Check191to101(SLA191Ato101BDiff, SLA191A, SLA101B);
        Check191to101(SLA191Bto101ADiff, SLA191B, SLA101A);
        Check191to101(SLA191Bto101BDiff, SLA191B, SLA101B);

        return fitness;
    }

    // Gets the difference in hour between two timeslots
    private int TimeDiff(TimeSlot start, TimeSlot end)
    {
        return Math.Abs(start - end);
    }

    // Runs the provided action on every timeslot for every room
    private void ForEvery(Action<List<Slot>> action)
    {
        foreach (var room in allSlots)
        {
            foreach (var slots in room)
            {
                action.Invoke(slots);
            }
        }
    }

    public override string ToString()
    {
        List<string> lines = [];

        for (int i = 0; i < allSlots.Length; i++)
        {
            lines.Add(Rooms[i].name);
            for (int j = 0; j < TimeSlotCount; j++)
            {
                lines.Add($"\t{(TimeSlot)j}: {string.Join(", ", allSlots[i][j])}");
            }
        }

        return string.Join('\n', lines);
    }
}