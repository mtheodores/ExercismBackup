using System;
using System.Collections.Generic;

public record State(int BucketOne, int BucketTwo);
public record ExploreGraphResult(State state, Bucket? bucket); 
public record Vertex(State state, int distance);

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

//once a vertex ever is processed by the queue never allow it to be processed again. 
public class UniqueVertexQueue
{
    private readonly Queue<Vertex> queue = new();
    private readonly HashSet<State> stateSet = new();

    public bool Enqueue(Vertex v)
    {
        // if we have ever seen this state, return false. 
        if (!this.stateSet.Add(v.state))
        {
            return false;
        }
        this.queue.Enqueue(v);
        return true;
    }

    public Vertex Dequeue()
    {
        //when we dequeue we do not remove the state from the hashset. 
        Vertex v = this.queue.Dequeue();
        return v;
    }

    public int Count()
    {
        return this.queue.Count(); 
    }
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

    //State transition functions below.
    public static State? FillBucketTwo(State oldState)
    {
        if (oldState.BucketTwo == bucketSizes[Bucket.Two])
        {
            return null;
        }
        if (Bucket.Two != startBucket)
        {
            if (oldState.BucketOne== 0)
            {
                return null;
            }
        }
        return new State(oldState.BucketOne, bucketSizes[Bucket.Two]);
    }

    public static State? FillBucketOne(State oldState)
    {
        if (oldState.BucketOne == bucketSizes[Bucket.One]) 
        {
            return null;
        }
        if (startBucket != Bucket.One)
        {
            if (oldState.BucketTwo == 0)
            {
                return null; //we cannot fill the other bucket while start bucket is empty.
            }
        }
        return new State(bucketSizes[Bucket.One], oldState.BucketTwo);
    }

    public static State? EmptyBucketOne(State oldState)
    {
        if (oldState.BucketOne ==  0) 
        {
            return null;
        }
        if (Bucket.One == startBucket)
        {
            // if the other bucket is full we cant empty the start bucket.
            if (oldState.BucketTwo == bucketSizes[Bucket.Two])
            {
                return null;
            }
        }
        return new State(0, oldState.BucketTwo);
    }

    public static State? EmptyBucketTwo(State oldState)
    {
        if (oldState.BucketTwo ==  0) 
        {
            return null;
        }
        if (Bucket.Two == startBucket)
        {
            // if the other bucket is full we cant empty the start bucket.
            if (oldState.BucketOne == bucketSizes[Bucket.One])
            {
                return null;
            }
        }
        return new State(oldState.BucketOne, 0);
    }

    public static State? PourBucketBA(State oldState)
    {
        // pour B into A.
        if (oldState.BucketTwo == 0 || oldState.BucketOne == bucketSizes[Bucket.One])
        {
            return null;
        }
        int one, two;
        one = two = -1;
        
        int remainingCapacityA = bucketSizes[Bucket.One] - oldState.BucketOne;
        if (oldState.BucketTwo > remainingCapacityA)
        {
            two = oldState.BucketTwo - remainingCapacityA;
            one = bucketSizes[Bucket.One]; 
            return new State(one, two);
        }
        else if (oldState.BucketTwo < remainingCapacityA)
        {
            one = oldState.BucketTwo + oldState.BucketOne;
            two = 0;
            return new State(one, two);
        }
        else{
            // if B is this start bucket this isn't allowed because B would be left empty
            if (startBucket == Bucket.Two) 
            {
                return null;
            }
            one = bucketSizes[Bucket.One];
            two = 0; 
            return new State(one, two);
        }
        
    }

    public static State? PourBucketAB(State oldState)
    {
        // pour A into B.
        if (oldState.BucketOne == 0 || oldState.BucketTwo == bucketSizes[Bucket.Two])
        {
            return null;
        }
        int one, two;
        one = two = -1;
        int remainingCapacityB = bucketSizes[Bucket.Two] - oldState.BucketTwo; //5
        if (oldState.BucketOne > remainingCapacityB)
        {
            one = oldState.BucketOne - remainingCapacityB;
            two = bucketSizes[Bucket.Two];
            return new State(one, two);
        }
        else if (oldState.BucketOne < remainingCapacityB)
        {
            two = oldState.BucketTwo + oldState.BucketOne;
            one = 0;
            return new State(one, two);
        }
        else{
            // if A is this start bucket this isn't allowed because A would be left empty
            if (startBucket == Bucket.One) 
            {
                return null;
            }

            two = bucketSizes[Bucket.Two];
            one = 0; 
            return new State(one, two);
        }
        
    }


    public TwoBucketResult Measure(int goal)
    {
        if (bucketSizes[Bucket.One] < goal && bucketSizes[Bucket.Two] < goal)
        {
            throw new ArgumentException("Impossible to reach goal; bucket sizes are too small.");
        }

        var initialState = (startBucket == Bucket.One) ? new State(bucketSizes[Bucket.One], 0) : new State(0, bucketSizes[Bucket.Two]);
        if (initialState.BucketOne == goal)
        {
            return new TwoBucketResult {Moves = 1, GoalBucket = Bucket.One, OtherBucket = initialState.BucketTwo};
        }
        if (initialState.BucketTwo == goal)
        {
            return new TwoBucketResult {Moves = 1, GoalBucket = Bucket.Two, OtherBucket = initialState.BucketOne};
        }

        var vertexQueue = new UniqueVertexQueue();
        vertexQueue.Enqueue(new Vertex(initialState, 1));
        List<Func<State, State?>>stateTransitions = GetStateTransitions();
        
        ExploreGraphResult? result = null;
        var stepsFinal = -1;
        while (result == null && vertexQueue.Count() > 0)
        {
            //1. Dequeue the state to process from the stack and get current minimal steps to get there.
            var currentVertex = vertexQueue.Dequeue(); 
            //2. do BFS on the graph of states
            int forLoopCounterDebug=1;
            foreach (var transition in stateTransitions)
            {
                forLoopCounterDebug++;
                result = ExploreGraph(transition, vertexQueue, currentVertex, goal);
                if (result != null) //non null means we found the node.
                {
                    stepsFinal = currentVertex.distance + 1;
                    break;
                }
            }
        }

        if (result == null || result.bucket == null || stepsFinal == -1)
        {
            throw new ArgumentException($"Goal {goal} is not reachable.");
        }
        return new TwoBucketResult {Moves = stepsFinal, 
                                    GoalBucket = result.bucket ?? Bucket.One, 
                                    OtherBucket = result.bucket == Bucket.One ? result.state.BucketTwo : result.state.BucketOne};
    }
    
        public static ExploreGraphResult? ExploreGraph(Func<State, State?>stateChange, 
                                                      UniqueVertexQueue vertexQueue, 
                                                      Vertex currentVertex,
                                                      int goal)
        {

            // explore possible unreached state in the graph
            var newState = stateChange(currentVertex.state);
            if (newState == null) 
            {
                //not a valid node from this state transition.
                return null;
            }

            // Did we find the node we are looking for.
            Bucket? goalBucket = IsGoal(newState, goal);
            if (goalBucket != null)
            {
                return new ExploreGraphResult(newState, goalBucket!);
            }

            // If its a new state it will enqueue, otherwise it will return false silently. 
            vertexQueue.Enqueue(new Vertex(newState, currentVertex.distance + 1));
            return null;
        }

        public static Bucket? IsGoal(State state, int goal)
        {
            if (state.BucketOne == goal)
            {
                return Bucket.One;
            }
            if (state.BucketTwo == goal)
            {
                return Bucket.Two;
            }
            return null;
        }

        public static List<Func<State, State?>> GetStateTransitions()
        {
            var functions = new List<Func<State, State?>>();
            functions.Add(EmptyBucketTwo);
            functions.Add(EmptyBucketOne);
            functions.Add(FillBucketTwo);
            functions.Add(FillBucketOne);
            functions.Add(PourBucketBA);
            functions.Add(PourBucketAB);
            return functions;
        }
}
