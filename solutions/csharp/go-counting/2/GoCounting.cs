public enum Owner
{
    None,
    Black,
    White
}

public class GoCounting
{
    public static List<List<char>> Board; 
    public static int N; //rows
    public static int M; //columns
    public static string inputString;
    
    public GoCounting(string input)
    {
        //1. process the input and turn it into a matrix. 
        M = input.IndexOf('\n', 0, input.Length);

        if (M == -1)
        {
            //single row board.
            N = 1;
            M = input.Length;
        }
        else{
            // number of rows N is going to be the count of newline characters + 1
            N = input.Count(c => c == '\n') + 1;
        }
        inputString = input;
        InitializeBoard(input); 
    }

    public static List<(int, int)> GetCoordFromSearch(List<List<char>> SearchBoard)
    {        
        List<(int, int)> result = new List<(int, int)>(); 
        for(int i=0; i<N; i++)
        {
            for(int j=0; j<M; j++)
            {
                if (WasSearched(j, i, SearchBoard))
                {
                    result.Add((j, i));
                }
            }
        }
        return result;
    }

    public static bool EndSearch(string s)
    {
        string endChars = "nSBW";
        return s.All(c => endChars.Contains(c));
    }

    public static (char left, char right, char up, char down) GetNeighbors(List<List<char>>Board, int x, int y)
    {
        char l = 'n';
        char r = 'n';
        char u = 'n';
        char d = 'n';
        if (x > 0) 
        {
            l = Board[y][x-1];
        }
        if(x < M-1)
        {
            r = Board[y][x+1];
        }
        if(y < N-1)
        {
            d = Board[y+1][x];
        }
        if(y > 0)
        {
            u = Board[y-1][x];
        }
        return (l, r, u, d); 
            
    }

    public static Owner MergeSearchResults(Owner left, Owner right, Owner up, Owner down)
    {
        var ownerList = new List<Owner>(){left, right, up, down}; 
        if (ownerList.All(o => o == Owner.Black)) return Owner.Black;
        if (ownerList.All(o => o == Owner.White)) return Owner.White;
        return Owner.None;
    }

    public static (Owner owner, bool FoundWhite, bool FoundBlack) SearchBoardRecursive(List<List<char>>SearchBoard, bool hasFoundWhite, bool hasFoundBlack, int x, int y)
    {
        SearchBoard[y][x] = 'S';

        if (hasFoundWhite && hasFoundBlack)
        {
             return (Owner.None, true, true);
        }
        
        //1. look around at top, left, right, down. 
        var (left, right, up, down) = GetNeighbors(SearchBoard, x, y); 
        string s = string.Concat(left, right, up, down);
        if (s.Any(c => Black(c)))
        {
            hasFoundBlack = true;
        }

        if (s.Any(c => White(c)))
        {
            hasFoundWhite = true;
        }

        if (hasFoundBlack)
        {
            if (hasFoundWhite)
            {
                return (Owner.None, hasFoundWhite, hasFoundBlack);
            }
        }
        if (hasFoundWhite)
        {
            if (hasFoundBlack)
            {
                return (Owner.None, hasFoundWhite, hasFoundBlack);
            }
        }
        
        //2. if nowhere left to go return. 
        bool searchOver = EndSearch(s); 
        if (searchOver)
        {
            if (hasFoundBlack) // we already checked if we found both white or black above so no need to recheck. 
            {
                return (Owner.Black, hasFoundWhite, hasFoundBlack);
            }
            if (hasFoundWhite)
            {
                return (Owner.White, hasFoundWhite, hasFoundBlack);
            }
            return (Owner.None, hasFoundWhite, hasFoundBlack); //hasn't found white or black but search is over.
        }
        
        //3. otherwise recurse.
        Owner LeftOwner, RightOwner, UpOwner, DownOwner;
        LeftOwner = RightOwner = UpOwner = DownOwner = Owner.None;
        
        if (Empty(left))
        {
            (LeftOwner, hasFoundWhite, hasFoundBlack) = SearchBoardRecursive(SearchBoard, hasFoundWhite, hasFoundBlack, x-1, y);
        }
        
        if (Empty(right))
        {
            (RightOwner, hasFoundWhite, hasFoundBlack)  = SearchBoardRecursive(SearchBoard, hasFoundWhite, hasFoundBlack, x+1, y);
        }

        if (Empty(up))
        {
            (UpOwner, hasFoundWhite, hasFoundBlack)  = SearchBoardRecursive(SearchBoard, hasFoundWhite, hasFoundBlack, x, y-1);
        }

        if (Empty(down))
        {
            (DownOwner, hasFoundWhite, hasFoundBlack)  = SearchBoardRecursive(SearchBoard, hasFoundWhite, hasFoundBlack, x, y+1);
        }

        if (hasFoundWhite && hasFoundBlack)
        {
            return (Owner.None, true, true);
        }

        //4. Merge the results.
        var mergedOwner = MergeSearchResults(LeftOwner, RightOwner, UpOwner, DownOwner);
        if (mergedOwner == Owner.None)
        {
            if (hasFoundBlack && !hasFoundWhite)
            {
                mergedOwner = Owner.Black;
            }
            else if(hasFoundWhite && !hasFoundBlack)
            {
                mergedOwner = Owner.White;
            }
        }
        return (mergedOwner, hasFoundWhite, hasFoundBlack);
    }

    public Tuple<Owner, HashSet<(int, int)>> Territory((int, int) coord)
    {
        if (!inputString.Contains("B") && !inputString.Contains("W"))
        {
            var returnSet = new HashSet<(int, int)>(); 
            // get all coordinates from the board. 
            for(int j=0; j<N; j++)
            {
                for(int i=0; i<M; i++)
                {
                    returnSet.Add((i, j));
                }
            }
            return (Owner.None, returnSet).ToTuple(); 
        }
        // given a coordinate x,y return an (owner, territory) tuple where territory is the set of x,y coordinates of that territory, 
        int x = coord.Item1;
        int y = coord.Item2;

        if (y > N-1 || x > M-1 || x < 0 || y < 0) throw new ArgumentException($"Invalid x,y coordinate = ({x},{y})");

        if (IsEmpty(x, y))
        {
            // main algorithm to determine what players territory this belongs to or none. 
            // idea, we can scan across the rows and down the columns. 
            // if we move left or right and hit the edge then we need to check the other direction. 
            // if we hit black moving left black is the only possible owner for this territory. 
            // if we ever hit white now from any traversal from this point noone owns the territory. 
            // assumption: there will always be at least 1 black and 1 white piece on the board.
            // board is split into subsections ;
            // we have a subsection where only black is reachable, where only white is reachable, where both are reachable. 
            //idea: recursively expand outward from a given point in all directions passing in: hasHitWhite and hasHitBlack, if we ever
            // hit both we return no owner, if we run out of places to look and we have hit one or the other we return that one. 
            // have to keep track of which spots have been searched.
            var searchBoard = GetSearchBoard(Board);

            var (owner, _, _) = SearchBoardRecursive(searchBoard, false, false, x, y);
            // ok you still have to accumulate all the actual coordinates seen in the search.
            // those would all be marked S on the search board. it would not have been able to go anywhere beyond a single territory. 

            var coords = GetCoordFromSearch(searchBoard); 
            return (owner, coords.ToHashSet()).ToTuple(); 
        }
        else
        {
            return (Owner.None, new HashSet<(int, int)>()).ToTuple();
        }     
    }

    public Dictionary<Owner, HashSet<(int, int)>> Territories()
    {
        var result = new Dictionary<Owner, HashSet<(int, int)>>(); 
        result[Owner.None] = new HashSet<(int, int)>();
        result[Owner.Black] = new HashSet<(int, int)>();
        result[Owner.White] = new HashSet<(int, int)>();

        // run the algorithm on the first empty space. 
        // run the algorithm on the next empty space thats not part of any territory. 
        // run until no empty spaces are not included in some territory result set.... 

        HashSet<(int, int)> processedPoints = new(); 
        List<(int, int)> emptySpots = new();
        for(int i=0; i<N; i++)
        {
            for(int j=0; j<M; j++)
            {
                if(IsEmpty(j, i))
                {
                    emptySpots.Add((j, i)); 
                }
            }
        }
        foreach(var point in emptySpots)
        {
            if(processedPoints.Contains(point))
            {
                continue;
            }
            processedPoints.Add(point); 
            var (owner, coords) = Territory(point); 
            
            processedPoints.UnionWith(coords); 
            result[owner].UnionWith(coords); 
        }
        return result;
    }

    public static void InitializeBoard(string input)
    {
        input = input.Replace("\n", ""); 
        Board = new List<List<char>>(); 
        for (int i=0; i<N; i++) //y
        {
            Board.Add(new List<char>());
            for (int j=0; j<M; j++) //x
            {
                Board[i].Add(' ');
            }
        }
        // ' ' means empty, B means black W means white.
        int sindex = 0;
        for (int j=0; j<N; j++) //y
        {
            for (int i=0; i<M; i++) //x
            {
                char c = input[sindex];
                if (!Valid(c))
                {
                    throw new ArgumentException("Invalid input provided.");
                }
                sindex++;

                Board[j][i] = c;
            }
        }
    }
    public static List<List<char>> GetSearchBoard(List<List<char>> board)
    {
        List<List<char>> deepCopy = board.Select(innerList => innerList.ToList()).ToList();
        return deepCopy;

    }
    public static bool IsWhite(int x, int y)
    {
        return White(Board[y][x]);
    }
    public static bool IsBlack(int x, int y)
    {
        return Black(Board[y][x]);
    }
    public static bool IsEmpty(int x, int y)
    {
        return Empty(Board[y][x]);
    }
    public static bool WasSearched(int x, int y, List<List<char>>SearchBoard)
    {
        return Searched(SearchBoard[y][x]);
    }
    
    public static bool White(char c) => c == 'W';
    public static bool Black(char c) => c == 'B';
    public static bool Empty(char c) => c == ' ';
    public static bool Searched(char c) => c == 'S';
    public static bool Edge(char c) => c == 'n';
    public static bool Valid(char c) => White(c) || Black(c) || Empty(c);
}
