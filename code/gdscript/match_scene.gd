extends Node2D
class_name MatchScene


const BoardDisplay: CSharpScript = preload("res://code/csharp/BoardDisplay.cs")

@export var board: BoardDisplay

func _ready() -> void:
	var screenSize: Vector2 = DisplayServer.window_get_size()
	board.PlaceBoardCenterAt(screenSize / 2 + 20*Vector2.UP)
