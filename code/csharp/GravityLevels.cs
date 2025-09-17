using System;
using System.Collections.Generic;
using Godot;

namespace USG;

public static class GravityLevels
{
	public const int MIN_LEVEL = 0;
	public const int MAX_LEVEL = 20;
    public static readonly List<double> Levels = [
        0.0, // Level 0
        0.0161616,
		0.02101,
		0.02696,
	    0.0352,
		0.0469,
		0.0636,
		0.0877,
		0.1234,
		0.1773,
		0.2604,
		0.3876,
	    0.5952,
		0.9259, 
        1.5151,
		2.3809,
		3.5533,
		5.2294,
		7.5385,
		10.6638,
		20.0, // Level 20 
    ];
}