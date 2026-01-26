using Godot;
using System;
using System.Diagnostics;

namespace USG;

public partial class BeforeGameMenu : Control, ISceneDataReceiver, ISceneDataEmitter
{
    public enum GeneratorType
    {
        BagPieceGenerator,
        BagPlusXPieceGenerator
    };
    
    public enum PieceSetType
    {
        Tetrominos,
        Pentominos,
        TetroAndPentominos,
        AllUpToPentominos
    };
    
    private PackedScene gotoScene;
    private BoardSettings settings;
    private GeneratorType generatorType;
    private PieceSetType pieceSet;
    [Export] private Button goButton;
    [Export] private SpinBox boardWidthValue;
    [Export] private SpinBox boardHeightValue;
    [Export] private OptionButton generatorOption;
    [Export] private OptionButton pieceSetOption;
    [Export] private OptionButton kickTableOption;
    
    public void InitData(SceneData data)
    {
        if(data is GamemodeSettingsSceneData gamemodeData)
        {
            this.gotoScene = gamemodeData.GameScene;
            this.settings = gamemodeData.Settings;
        }
    }

    private void GenerateGeneratorInSettings()
    {
        string[] generatorPieceset = pieceSet switch
        {
            PieceSetType.Tetrominos => Pieces.TetrominosBag,
            PieceSetType.Pentominos => Pieces.PentominosBag,
            PieceSetType.TetroAndPentominos => Pieces.CombinedTetrominosPentominosBag,
            PieceSetType.AllUpToPentominos => throw new NotImplementedException(),
            _ => throw new UnreachableException()
        };
        PieceGenerator generator = generatorType switch
        {
            GeneratorType.BagPieceGenerator => new BagPieceGenerator(generatorPieceset),
            GeneratorType.BagPlusXPieceGenerator => new BagPlusXPieceGenerator(generatorPieceset, 2),
            _ => throw new UnreachableException()
        };
        this.settings.Generator = generator;
    }
    
    public SceneData GetData()
    {
        GenerateGeneratorInSettings();
        return new GamemodeSettingsSceneData(this.gotoScene, this.settings);
    }

    public override void _Ready()
    {
        base._Ready();
        goButton.Pressed += OnGoButtonPressed;
        boardWidthValue.ValueChanged += OnBoardWidthValueChanged;
        boardHeightValue.ValueChanged += OnBoardHeightValueChanged;
        generatorOption.ItemSelected += OnGeneratorItemSelected;
        pieceSetOption.ItemSelected += OnPieceSetItemSelected;
        kickTableOption.ItemSelected += OnKickTableItemSelected;
    }

    private void OnBoardWidthValueChanged(double value)
    {
        if(double.IsInteger(value))
        {
            this.settings.BoardWidth = (int)value;
        }
    }
    
    private void OnBoardHeightValueChanged(double value)
    {
        if(double.IsInteger(value))
        {
            this.settings.BoardHeight = (int)value;
        }
    }
    
    private void OnGeneratorItemSelected(long itemIndex)
    {
        this.generatorType = itemIndex switch
        {
            0 => GeneratorType.BagPieceGenerator,
            1 => GeneratorType.BagPlusXPieceGenerator,
            _ => throw new UnreachableException()
        };
    }
    
    private void OnPieceSetItemSelected(long itemIndex)
    {
        this.pieceSet = itemIndex switch
        {
            0 => PieceSetType.Tetrominos,
            1 => PieceSetType.Pentominos,
            2 => PieceSetType.TetroAndPentominos,
            3 => PieceSetType.AllUpToPentominos,
            _ => throw new UnreachableException()
        };
    }
    
    private void OnKickTableItemSelected(long itemIndex)
    {
        this.settings.SetKickTable = itemIndex switch
        {
            0 => KickTableValue.SRSKickTable,
            1 => KickTableValue.TechminoKickTable,
            2 => KickTableValue.ASCRotationSystem,
            3 => KickTableValue.NoKicks,
            _ => throw new UnreachableException()
        };
    }
    
    private void OnGoButtonPressed()
    {
        GetTree().CreateTimer(0.25).Timeout += () =>
        {
            SceneManager.Instance.SwitchScene(this.gotoScene, SceneTransitionScreen.TransitionKind.WipeToRight);
        };
    }
}
