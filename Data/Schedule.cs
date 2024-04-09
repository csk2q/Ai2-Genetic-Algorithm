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
                allSlots[i][j] = new List<Slot>(1);
        }
    }

    public static Schedule RandomizedSchedule(Random rand)
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

    public double Fitness()
    {
        // Note: Implementation focuses on aligning closely the requirement's wording over performance.
        // TODO combine checks to reduce number of passes over the schedule

        Dictionary<Activity, TimeSlot> activityToTimeSlot = [];
        Dictionary<Activity, Room> activityToRoom = [];


        // For each activity, fitness starts at 0.
        double fitness = 0;
        Dictionary<Facilitator, int> facilitatorLoad = [];
        foreach (var faclitator in Enum.GetValues<Facilitator>())
            facilitatorLoad[faclitator] = 0;

        // For every room
        for (int roomIndex = 0; roomIndex < allSlots.Length; roomIndex++)
        {
            var roomCapacity = Rooms[roomIndex].capacity;

            // For every timeslot for a room
            foreach (var slots in allSlots[roomIndex])
            {
                // Activity is scheduled at the same time in the same room as another of the activities: -0.5
                // -0.5 for every extra activity in a room at the same time.
                fitness -= (slots.Count - 1) * -0.5;

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
            Dictionary<Facilitator, int> facilitatorLoadTimeslot = [];
            foreach (var faclitator in Enum.GetValues<Facilitator>())
                facilitatorLoadTimeslot[faclitator] = 0;

            // For every room
            for (int roomIndex = 0; roomIndex < allSlots.Length; roomIndex++)
            {
                //For each slot in the timeslot
                foreach (var slot in allSlots[roomIndex][timeslotId])
                {
                    facilitatorLoadTimeslot[slot.facilitator] += 1;

                    activityToTimeSlot.Add(slot.activity, (TimeSlot)timeslotId);
                    activityToRoom.Add(slot.activity, Rooms[roomIndex]);
                }
            }

            // Facilitator load:
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


        // Facilitator load:
        // ◦ Activity facilitator is scheduled for only 1 activity in this time slot: + 0.2
        // ◦ Activity facilitator is scheduled for more than one activity at the same time: - 0.2
        // ◦ Facilitator is scheduled to oversee more than 4 activities total: -0.5
        //
        // ◦ Facilitator is scheduled to oversee 1 or 2 activities*: -0.4
        //      ▪ Exception: Dr. Tyler is committee chair and has other demands on his time.
        //          *No penalty if he’s only required to oversee < 2 activities.
        // ◦ If any facilitator scheduled for _consecutive_ time slots: Same rules as for SLA 191 and SLA 101 in consecutive time slots—see below.
        //      A section of SLA 191 and a section of SLA 101 are overseen in consecutive time slots (e.g., 10 AM & 11 AM): +0.5
        //       ◦ In this case only (consecutive time slots), one of the activities is in Roman or Beach, and the other isn’t: -0.4
        //           ▪ It’s fine if neither is in one of those buildings, of activity; we just want to avoid having consecutive activities being widely separated.

        // TODO Ask if facilitator checking is correct?
        // Optimal is a faclitator teaching 3 classes and Tyler teaching 1 or 0?

        var normalFacilitators = Enum.GetValues<Facilitator>().ToList();
        normalFacilitators.Remove(Facilitator.Tyler);
        foreach (var faclitator in normalFacilitators)
        {
            if (facilitatorLoad[faclitator] > 4)
                fitness -= 0.5;
            else if (facilitatorLoad[faclitator] == 1 || facilitatorLoad[faclitator] == 2)
                fitness -= 0.4;
        }

        // Penalty if Dr. Tyler is teaching more than one class.
        if (facilitatorLoad[Tyler] > 4)
            fitness -= 0.4;
        else if (facilitatorLoad[Tyler] >= 2)
            fitness -= 0.4;
        
        
        // Activity-specific adjustments:
        // • The 2 sections of SLA 101 are more than 4 hours apart: + 0.5
        // • Both sections of SLA 101 are in the same time slot: -0.5
        
        // • The 2 sections of SLA 191 are more than 4 hours apart: + 0.5
        // • Both sections of SLA 191 are in the same time slot: -0.5
        
        //TODO continue with the below.
        // • A section of SLA 191 and a section of SLA 101 are overseen in consecutive time slots (e.g., 10 AM & 11 AM): +0.5
        //   ◦ In this case only (consecutive time slots), one of the activities is in Roman or Beach, and the other isn’t: -0.4
        //      ▪ It’s fine if neither is in one of those buildings, of activity; we just want to avoid having consecutive activities being widely separated.
        
        // • A section of SLA 191 and a section of SLA 101 are taught separated by 1 hour (e.g., 10 AM & 12:00 Noon): + 0.25
        // • A section of SLA 191 and a section of SLA 101 are taught in the same time slot: -0.25
        
        var SLA101Diff = TimeDiff(activityToTimeSlot[SLA101A], activityToTimeSlot[SLA101B]);
        if (SLA101Diff > 4)
            fitness += 0.5;
        else if (SLA101Diff == 0)
            fitness -= 0.5;
        
        var SLA191Diff = TimeDiff(activityToTimeSlot[SLA191A], activityToTimeSlot[SLA191B]);
        if (SLA191Diff > 4)
            fitness += 0.5;
        else if (SLA191Diff == 0)
            fitness -= 0.5;
        
        var SLA191Ato101ADiff = TimeDiff(activityToTimeSlot[SLA191A], activityToTimeSlot[SLA101A]);
        var SLA191Ato101BDiff = TimeDiff(activityToTimeSlot[SLA191A], activityToTimeSlot[SLA101B]);
        var SLA191Bto101ADiff = TimeDiff(activityToTimeSlot[SLA191B], activityToTimeSlot[SLA101A]);
        var SLA191Bto101BDiff = TimeDiff(activityToTimeSlot[SLA191B], activityToTimeSlot[SLA101B]);

        var Check191to101 = (int timeDiff, Activity A, Activity B) => {
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
            else if (timeDiff == 2) // Two means one hour gap between classes.
            {
                fitness += 0.25;
            }
            else if (timeDiff == 0)
                fitness -= 0.25;
        };

        Check191to101(SLA191Ato101ADiff, SLA191A, SLA101A);
        Check191to101(SLA191Ato101BDiff, SLA191A, SLA101B);
        Check191to101(SLA191Bto101ADiff, SLA191B, SLA101A);
        Check191to101(SLA191Bto101BDiff, SLA191B, SLA101B);

        //TODO pick back up here.

        

        return fitness;
    }

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