public class SpiralMatrix
{

    static int Mod(int a, int b)
    {
        return ((a % b) + b) % b;
    }
    
    private static void PrintMatrix(int[,] m, int size)
    {
        for(int j=0; j<size; j++)
        {
            Console.Write("| ");
            for(int i=0; i<size; i++)
            {
                 Console.Write($"{m[j,i]}, ");   
            }
            Console.WriteLine("|");
        }
    }
    public static int[,] GetMatrix(int size)
    {
        if (size == 1)
        {
            var r = new int[1,1];
            r[0, 0] = 1;
        }
        var matrix = new int[size, size]; 
        int n = size*size;

        int row = 0;
        int col = 0;

        int sideLength = size; 
        int direction = 0;
        int relativeIndex = 0;
        int spiral = 0;
        for(int i=0; i<n; i++)
        {
            int v = i+1;
            if (relativeIndex % sideLength == 0 && i != 0)
            {
                direction = (direction + 1) % 4;
                
                relativeIndex = 1;
                
                if (direction == Mod(3 - spiral - (size - sideLength), 4)) //every time we go in the 
                {
                    sideLength--;
                    spiral++; 
                }

                if (direction == 0)
                {
                    col++;
                }
                if(direction == 1)
                {
                    row++;
                }
                if(direction == 2)
                {
                    col--;
                }
                if(direction == 3)
                {
                    row--;
                }
                
            }

            matrix[row, col] = v; 

            if (direction == 0 && ((relativeIndex + 1) % sideLength != 0))
            {
                col++;
            }
            if(direction == 1 && ((relativeIndex + 1) % sideLength != 0))
            {
                row++;
            }
            if(direction == 2 && ((relativeIndex + 1) % sideLength != 0))
            {
                col--;
            }
            if(direction == 3 && ((relativeIndex + 1) % sideLength != 0))
            {
                row--;
            }
            relativeIndex++;
            //PrintMatrix(matrix, size); 
        }
        return matrix;
    }
}
