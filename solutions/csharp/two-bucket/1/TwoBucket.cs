using System;
using System.Collections.Generic;

public enum Bucket
{
    One,
    Two
}

public class TwoBucketResult
{
    public int Moves { get; set; }
    public Bucket GoalBucket { get; set; }
    public int OtherBucket { get; set; }
}

public class TwoBucket
{
    public static Dictionary<Bucket, int>bucketSizes;
    
    public static Bucket startBucket;
    
    public TwoBucket(int bucketOne, int bucketTwo, Bucket starterBucket)
    {
        bucketSizes = new Dictionary<Bucket, int>(); 
        
        bucketSizes.Add(Bucket.One, bucketOne);
        bucketSizes.Add(Bucket.Two, bucketTwo);

        startBucket = starterBucket;
    }

    public static (bool Legal, int One, int Two) FillBucketTwo(int a, int b)
    {
        if (b == bucketSizes[Bucket.Two])
            return (false, -1, -1); //no-op
        if (Bucket.Two != startBucket)
        {
            if (a == 0)
            {
                return (false, -1, -1); //we cannot fill the other bucket while start bucket is empty.
            }
        }
        b = bucketSizes[Bucket.Two];
        return (true, a, b); 
    }

    public static (bool Legal, int One, int Two) FillBucketOne(int a, int b)
    {
        if (a == bucketSizes[Bucket.One]) 
            return (false, -1, -1); //no-op
        if (startBucket != Bucket.One)
        {
            if (b == 0)
            {
                return (false, -1, -1); //we cannot fill the other bucket while start bucket is empty.
            }
        }
        a = bucketSizes[Bucket.One];
        return (true, a, b); 
    }

    public static (bool Legal, int One, int Two) EmptyBucketOne(int a, int b)
    {
        if (bucketSizes[Bucket.One] ==  0) 
            return (false, -1, -1); //noop
        if (Bucket.One == startBucket)
        {
            // if the other bucket is full we cant empty the start bucket.
            if (b == bucketSizes[Bucket.Two])
            {
                return (false, -1, -1);
            }
        }
        return (true, 0, b);
    }

    public static (bool Legal, int One, int Two) EmptyBucketTwo(int a, int b)
    {
        if (bucketSizes[Bucket.Two] ==  0) 
            return (false, -1, -1); //noop
        if (Bucket.Two == startBucket)
        {
            // if the other bucket is full we cant empty the start bucket.
            if (a == bucketSizes[Bucket.One])
            {
                return (false, -1, -1);
            }
        }
        return (true, a, 0);
    }

    public static (bool Legal, int One, int Two) PourBucketBA(int a, int b)
    {
        // pour B into A.
        if (b == 0)
            return (false, -1, -1);
        
        int remainingCapacityA = bucketSizes[Bucket.One] - a;
        if (b > remainingCapacityA)
        {
            b = b - remainingCapacityA;
            a = bucketSizes[Bucket.One];
            return (true, a, b);
        }
        else if (b < remainingCapacityA)
        {
            a = b + a;
            b = 0;
            return (true, a, b);
        }
        else{
            // if B is this start bucket this isn't allowed because B would be left empty
            if (startBucket == Bucket.Two) return (false, -1, -1);

            a = bucketSizes[Bucket.One];
            b = 0; 
            return (true, a, b); 
        }
        
    }

    public static (bool Legal, int One, int Two) PourBucketAB(int a, int b)
    {
        // pour A into B.
        if (a == 0)
        {
            return (false, -1, -1); 
        }
        
        int remainingCapacityB = bucketSizes[Bucket.Two] - b;
        if (a > remainingCapacityB)
        {
            a = a - remainingCapacityB;
            b = bucketSizes[Bucket.Two];
            return (true, a, b);
        }
        else if (a < remainingCapacityB)
        {
            b = b + a;
            a = 0;
            return (true, a, b);
        }
        else{
            // if A is this start bucket this isn't allowed because A would be left empty
            if (startBucket == Bucket.One) return (false, -1, -1);

            b = bucketSizes[Bucket.Two];
            a = 0; 
            return (true, a, b); 
        }
        
    }


    public TwoBucketResult Measure(int goal)
    {
        if (bucketSizes[Bucket.One] < goal && bucketSizes[Bucket.Two] < goal)
        {
            throw new ArgumentException("Impossible.");
        }
        // bucket 1: 7L
        // bucket 2: 11L

        //bucket 1 is holding 7, bucket 2 is holding 8, (7, 8)
        // empty bucket 1 ---> (0, 8)
        // pour bucket 1 into bucket 2 ---> (4, 11)
        // you may not end up in a state where the starting bucket is empty and the other bucket is full. 

        //1. fill startbucket; 

        //example, (3, 5, BucketOne), measure(1)
        // (3, 0), goal = 1.
        // (0, 3)        or         (3, 5)
        // ((3, 3), (0, 0))        
        // ((3, 5))
        // (())
        // track states we have reached and the amount of steps it took to reach the state. 
        var States = new Dictionary<(int, int), int>(); 
        Console.WriteLine($"Starting bucket: {startBucket}");
        var theState = (startBucket == Bucket.One) ? (bucketSizes[Bucket.One], 0) : (0, bucketSizes[Bucket.Two]);
        if (theState.Item1 == goal)
        {
            return new TwoBucketResult {Moves = 1, GoalBucket = Bucket.One, OtherBucket = theState.Item2};
        }
        if (theState.Item2 == goal)
        {
            return new TwoBucketResult {Moves = 1, GoalBucket = Bucket.Two, OtherBucket = theState.Item1};
        }
        States.Add(theState, 1); 
        Console.WriteLine($"Starting state: ({theState.Item1}, {theState.Item2})");
        bool found = false;

        //todo: change it to be a queue, no? 
        // isnt this problem basically BFS on the graph of states? 
        // yes thats correct only BFS is a shortest path algorith, 
        var StateQueue = new Queue<(int, int)>(); 
        StateQueue.Enqueue(theState);

        List<Func<int, int, (bool, int, int)>>functions = new List<Func<int, int, (bool, int, int)>>();
        functions.Add(EmptyBucketTwo);
        functions.Add(EmptyBucketOne);
        functions.Add(FillBucketTwo);
        functions.Add(FillBucketOne);
        functions.Add(PourBucketBA);
        functions.Add(PourBucketAB);
        (bool, int, int, Bucket?) result = (false, 0, 0, null);
        var stepsFinal = -1;
        int loopCount = 0;
        while (!found && !(StateQueue.Count == 0))
        {
            //1. Dequeue the state to process from the stack and get current minimal steps to get there.
            var (one, two) = StateQueue.Dequeue(); 
            
            Console.WriteLine($"Dequeued State: ({one}, {two})");
            
            int steps = States[(one, two)];
            
            //2. get new possible states.
            int index = 0;
            foreach (var function in functions)
            {
                Console.WriteLine($"Considering function: {index}");
                index++;
                //3. update state dictionary. 
                //4. if we found the target state return. 
                //5. if we already found the state dont add new state to the queue
                //6. otherwise Enqueue it for further processing. 
                result = ExecuteWithProcessing(function, States, StateQueue, steps, one, two, goal);
                if (result.Item1)
                {
                    found = true;
                    Console.WriteLine($"Goal: {goal}, satisfied by: ({result.Item2}, {result.Item3})");
                    stepsFinal = steps+1;
                    break;
                }
            }
            loopCount++;
            if(loopCount > 100) throw new ArgumentException("Looks like infinite loop");
        }
        if (!found || result.Item1 == null)
        {
            throw new ArgumentException("Failed.");
        }
        return new TwoBucketResult {Moves = stepsFinal, GoalBucket = result.Item4 ?? Bucket.One, OtherBucket = result.Item4 == Bucket.One ? result.Item3 : result.Item2};
    }
    
        public static (bool Found, int One, int Two, Bucket? bucket) ExecuteWithProcessing(Func<int, int, (bool, int, int)>stateChange, Dictionary<(int, int), int> States, Queue<(int, int)> StateQueue, int steps, int one, int two, int goal)
        {
            (bool, int, int) result = (false, 0, 0);
            var isGoalResult = (false, Bucket.One);
            
            result = stateChange(one, two);
            if (!result.Item1) return (false, 0, 0, null);
            isGoalResult = IsGoal(result.Item2, result.Item3, goal);
            if (isGoalResult.Item1)
            {
                return (true, result.Item2, result.Item3, isGoalResult.Item2);
            }
            UpdateStates(result.Item1, result.Item2, result.Item3, steps, States, StateQueue);
            return (false, 0, 0, null);
        }

        public static void UpdateStates(bool legal, int a, int b, int steps, Dictionary<(int, int), int> States, Queue<(int, int)> StateQueue)
        {
            if (legal) //not a node in the graph. 
            {
                // Dont you have to check not if it contains it but if it contains it at equal or fewer steps? 
                // review BFS algorithm once. 
                if (!States.ContainsKey((a, b))) //equivalent to .visited on a node in the graph.... 
                {
                    StateQueue.Enqueue((a, b));
                    States.Add((a, b), steps + 1);
                    Console.WriteLine($"---> Enqueued State: ({a}, {b}) with steps {steps+1}");
                }
            }
        }

        public static (bool goalReached, Bucket bucket) IsGoal(int a, int b, int goal)
        {
            if (a == goal)
            {
                return (true, Bucket.One);
            }
            if (b == goal)
            {
                return (true, Bucket.Two);
            }
            return (false, Bucket.One);
        }
        
}
