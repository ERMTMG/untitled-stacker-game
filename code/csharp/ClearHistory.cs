using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace USG;

public record struct ClearInfo
{
	public readonly int Lines;
	public readonly string Piece;
	public readonly SpinType Spin;

	public ClearInfo(int lines, string piece, SpinType spin)
	{
		Lines = lines;
		Piece = piece;
		Spin = spin;
	}

	public readonly bool IsDifficult() => (Lines >= 4 || Spin != SpinType.NoSpin);
}

public class ClearHistory
{
	const int DEFAULT_CAPACITY = 256;
	private List<ClearInfo> data;


	public ClearHistory()
	{
		data = new List<ClearInfo>(DEFAULT_CAPACITY);
	}

	public int Size => data.Count;

	public ClearInfo LatestClear => data.Last();

	public void PushClear(ClearInfo clear)
	{
		data.Add(clear);
	}
}
