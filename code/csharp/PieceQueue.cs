using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace USG;

using PieceID = string;

public class PieceQueue
{
    private PieceGenerator pieceGenerator;
    private Queue<PieceID> queue;

    public const int MAX_EXPECTED_NEXT_VISIBILITY = 7;

    public PieceQueue(PieceGenerator pieceGenerator)
    {
        this.queue = new Queue<PieceID>();
        this.pieceGenerator = pieceGenerator;
    }

    private void FillQueue()
    {
        if(pieceGenerator.BufferSize == 0)
        {
            pieceGenerator.GeneratePieces();
        }
        PieceID[] newPieces = pieceGenerator.TakeBuffer();
        for(int i = newPieces.Length - 1; i >= 0; i--)
        {
            queue.Enqueue(newPieces[i]);
        }
    }

    public PieceID GetNext(int nextIdx = 0)
    { 
        if(nextIdx < MAX_EXPECTED_NEXT_VISIBILITY)
        {
            while(nextIdx >= queue.Count())
            {
                FillQueue();
            }
            if(nextIdx == 0)
            {
                return queue.Peek();
            } else {
                return queue.ElementAt(nextIdx);
            }
        } else {
            throw new IndexOutOfRangeException($"Can't get piece at next index {nextIdx} (max is {MAX_EXPECTED_NEXT_VISIBILITY - 1})");
        }
    } 

    public void Pop()
    {
        queue.Dequeue();
    }
}