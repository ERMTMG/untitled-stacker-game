using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Godot;
namespace USG;

using PieceID = string;

/*
Contains all the necessary data about the different pieces, bags, etc.
*/

public static partial class Pieces 
{
    public static readonly ReadOnlyDictionary<PieceID, Piece> PiecesMap = new Dictionary<PieceID, Piece>()
    {
        ["T"] = new Piece("T", new int[,]{{-1, 7,-1},
                                          { 7, 7, 7},
                                          {-2, 0,-2}}),

        ["I"] = new Piece("I", new int[,]{{0,0,0,0},
                                          {5,5,5,5},
                                          {0,0,0,0},
                                          {0,0,0,0}}),

        ["O"] = new Piece("O", new int[,]{{3,3},
                                          {3,3}}),

        ["S"] = new Piece("S", new int[,]{{-1, 4, 4},
                                          { 4, 4,-1},
                                          { 0, 0, 0}}),

        ["Z"] = new Piece("Z", new int[,]{{ 1, 1,-1},
                                          {-1, 1, 1},
                                          { 0, 0, 0}}),

        ["L"] = new Piece("L", new int[,]{{-1,-1, 2},
                                          { 2, 2, 2},
                                          { 0, 0, 0}}),
        
        ["J"] = new Piece("J", new int[,]{{ 6,-1,-1},
                                          { 6, 6, 6},
                                          { 0, 0, 0}}),

        // PENTOMINOS
    
        ["T5"] = new Piece("T5", new int[,]{{0,7,0},
                                            {0,7,0},
                                            {7,7,7}}, spawningRowOffset: 3),
        
        ["I5"] = new Piece("I5", new int[,]{{0,0,0,0,0},
                                            {0,0,0,0,0},
                                            {5,5,5,5,5},
                                            {0,0,0,0,0},
                                            {0,0,0,0,0}}, spawningRowOffset: 3),

        ["X"]  = new Piece("X",  new int[,]{{0,3,0},
                                            {3,3,3},
                                            {0,3,0}}, spawningRowOffset: 3),

        ["S5"] = new Piece("S5", new int[,]{{0,4,4},
                                            {0,4,0},
                                            {4,4,0}}, spawningRowOffset: 3),

        ["Z5"] = new Piece("Z5", new int[,]{{1,1,0},
                                            {0,1,0},
                                            {0,1,1}}, spawningRowOffset: 3),
        
        ["P"]  = new Piece("P",  new int[,]{{3,3,0},
                                            {3,3,3},
                                            {0,0,0}}),
        
        ["Q"]  = new Piece("Q",  new int[,]{{0,4,4},
                                            {4,4,4},
                                            {0,0,0}}),
        
        ["L5"] = new Piece("L5", new int[,]{{0,0,0,0},
                                            {0,0,0,2},
                                            {2,2,2,2},
                                            {0,0,0,0}}, spawningRowOffset: 3),

        ["J5"] = new Piece("J5", new int[,]{{0,0,0,0},
                                            {6,0,0,0},
                                            {6,6,6,6},
                                            {0,0,0,0}}, spawningRowOffset: 3),

        ["U"]  = new Piece("U",  new int[,]{{1,0,1},
                                            {1,1,1},
                                            {0,0,0}}),
        
        ["N"]  = new Piece("N",  new int[,]{{0,0,0,0},
                                            {6,6,0,0},
                                            {0,6,6,6},
                                            {0,0,0,0}}, spawningRowOffset: 3),
        
        ["H"]  = new Piece("H",  new int[,]{{0,0,0,0},
                                            {0,0,7,7},
                                            {7,7,7,0},
                                            {0,0,0,0}}, spawningRowOffset: 3),
        
        ["F"]  = new Piece("F",  new int[,]{{0,2,0},
                                            {0,2,2},
                                            {2,2,0}}, spawningRowOffset: 3),
        
        ["E"]  = new Piece("E",  new int[,]{{0,5,0},
                                            {5,5,0},
                                            {0,5,5}}, spawningRowOffset: 3),
        
        ["Y"]  = new Piece("Y",  new int[,]{{0,0,0,0},
                                            {0,0,7,0},
                                            {7,7,7,7},
                                            {0,0,0,0}}, spawningRowOffset: 3),
        
        ["R"]  = new Piece("R",  new int[,]{{0,0,0,0},
                                            {0,1,0,0},
                                            {1,1,1,1},
                                            {0,0,0,0}}, spawningRowOffset: 3),
        
        ["V"]  = new Piece("V",  new int[,]{{6,0,0},
                                            {6,0,0},
                                            {6,6,6}}, spawningRowOffset: 3),
        
        ["W"]  = new Piece("W",  new int[,]{{7,0,0},
                                            {7,7,0},
                                            {0,7,7}}, spawningRowOffset: 3),
    }.AsReadOnly();


    public static readonly PieceID[] TetrominosBag = [
        "T", "I", "O", "S", "Z", "L", "J"
    ];

    public static readonly PieceID[] PentominosBag = [
        "T5", "I5", "X", "S5", "Z5", "P", "Q", "L5", "J5", 
        "U", "N", "H", "F", "E", "Y", "R", "V", "W"
    ];

    public static readonly PieceID[] CombinedTetrominosPentominosBag = [
        "T", "I", "O", "S", "Z", "L", "J",
        "T5", "I5", "X", "S5", "Z5", "P", "Q", "L5", "J5", 
        "U", "N", "H", "F", "E", "Y", "R", "V", "W"
    ];



    public static readonly ReadOnlyDictionary<string, Texture2D> PiecePreviewSprites 
	= new Dictionary<string, Texture2D>{
		["T"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/tetromino/T.png"),
		["I"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/tetromino/I.png"),
		["O"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/tetromino/O.png"),
		["L"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/tetromino/L.png"),
		["J"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/tetromino/J.png"),
		["S"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/tetromino/S.png"),
		["Z"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/tetromino/Z.png"),

		["T5"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/T5.png"),
		["I5"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/I5.png"),
		["X"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/X.png"),
		["S5"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/S5.png"),
		["Z5"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/Z5.png"),
		["P"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/P.png"),
		["Q"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/Q.png"),
		["L5"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/L5.png"),
		["J5"] = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/J5.png"),
		["U"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/U.png"),
		["N"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/N.png"),
		["H"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/H.png"),
		["F"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/F.png"),
		["E"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/E.png"),
		["Y"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/Y.png"),
		["R"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/R.png"),
		["V"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/V.png"),
		["W"]  = GD.Load<Texture2D>("res://assets/graphics/piece_renders/pentomino/W.png"),
	}.AsReadOnly();
    
};
