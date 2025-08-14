using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Godot;
using Microsoft.VisualBasic;

namespace USG;

using PieceID = string;

// base piece generator class for generating pieces on a PieceQueue.
[Tool] [GlobalClass]
public partial class PieceGenerator : Resource
{
	// An array of the possible pieces the generator may output. Ideally unordered and unique.
	[Export] protected PieceID[] availablePieces;
	/// A buffer of the next pieces to come.
	protected List<PieceID> buffer; // TODO: this may be better as a linked list? not many random accesses

	public int BufferSize => buffer.Count;

	public PieceGenerator() : base()
	{
		buffer = new List<PieceID>();
		availablePieces = [];
	}

	public PieceGenerator(PieceID[] availablePieces) : base()
	{
		this.availablePieces = availablePieces;
		buffer = new List<PieceID>();
	}

	// Fills the buffer with a set of new pieces
	public virtual void GeneratePieces()
	{
		throw new NotImplementedException($"Abstract class {nameof(PieceGenerator)} can't execute method GeneratePieces");
	}

	public void EmptyBuffer()
	{
		buffer.Clear();
	}

	public PieceID[] TakeBuffer()
	{
		PieceID[] output = new PieceID[BufferSize];
		buffer.CopyTo(output);
		EmptyBuffer();
		return output;
	}

	// Inserts the given array of pieces at the start of the buffer.
	protected void AddToBuffer(PieceID[] pieces)
	{
		buffer.InsertRange(0, pieces);
	}

	// Pops and returns the last piece in the buffer
	public PieceID GetNext()
	{  
		PieceID output = buffer.Last();
		buffer.RemoveAt(BufferSize - 1);
		return output;
	}



}
