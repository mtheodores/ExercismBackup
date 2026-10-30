using System.Text;
using System;
using System.Linq; 

public static class LargestSeriesProduct
{
    public static long GetLargestProduct(string digits, int span) 
    {
        if (digits == String.Empty && span != 0)
        {
            throw new ArgumentException("invalid");
        }

        if (digits.Length > 0 && span == 0)
        {
            return 1;
        }

        if (digits == String.Empty && span == 0)
        {
            return 1;
        }

        if(span > digits.Length || span < 0)
        {
            throw new ArgumentException("invalid");
        }

        if(digits.ToCharArray().Any(c => c < 48 || c > 57))
        {
            throw new ArgumentException("Invalid digits in array, try again with 0-9");
        }

        //["214305768", 4]
          //[2,2,8,24,1,5,35,210,1680]
        
        Dictionary<int, int> prefixProducts = new(); 
        int runningSpan = 0;
        int runningProduct = 1;
        int maxProduct = 0;
        for(int i=0; i<digits.Length; i++)
        {
            if (digits[i] == 48) //0 can never create a maximal product. So just ignore the 0. 
            {
                runningProduct = 1;
                prefixProducts[i] = 1;
                runningSpan = 0;
                continue;
            }
            
            //still accumulating span values
            if (runningSpan < span) 
            {
                runningProduct = runningProduct * (digits[i] - 48); 
                runningSpan++;
            }
            // chop off old value and add new one
            else{ 
                runningProduct = (prefixProducts[i-1] / (digits[i-span] - 48)) * (digits[i] - 48);
            }
            
            prefixProducts[i] = runningProduct;
            
            if (runningSpan == span && runningProduct > maxProduct)
            {
                maxProduct = runningProduct;
            }

        }
        return maxProduct;
        
    }
}


//long result = charListSorted.Aggregate(1, (acc, next) => acc * (next - 48));
//return result;