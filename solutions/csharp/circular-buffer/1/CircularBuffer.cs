using System;

public class CircularBuffer<T>
{
    public int size; 
    private int head; 
    private int tail;
    private int count;
    private List<T> data;
    public CircularBuffer(int capacity)
    {
        this.size = capacity;
        this.head = 0; 
        this.tail = 0; //when head = tail the buffer is empty.
        this.count = 0;
        this.data = Enumerable.Repeat(default(T), capacity).ToList(); 
        
    }

    public T Read()
    {
        if (this.count == 0)
        {
            throw new InvalidOperationException("Cannot read from empty buffer");
        }
        var val = this.data[this.tail];
        this.tail = (this.tail + 1) % size; 
        this.count--;
        return val; 
    }

    public void Write(T value)
    {
        if (this.count == this.size)
        {
            throw new InvalidOperationException("The buffer is full, please use Overwrite method to overwrite existing elements.");
        }
        
        this.data[head] = value; 
        this.head = (this.head + 1) % this.size;
        this.count++; 
    }

    public void Overwrite(T value)
    {
        if (this.count < this.size)
        {
            Write(value);
            return;
        }
        this.data[head] = value;
        this.head = (this.head + 1) % this.size;
        this.tail = (this.tail + 1) % size; 
    }

    public void Clear()
    {
        this.head = 0;
        this.tail = 0;
        this.count = 0; 
    }
}