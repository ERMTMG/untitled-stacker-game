using System;
using Godot;

namespace USG;

public partial class GameBoard : Node
{
    const long SOFT_DROP_SCORE_PER_ROW = 1;
    const long HARD_DROP_SCORE_PER_ROW = 2;
    static readonly long[] NORMAL_LINE_CLEAR_SCORES = [100, 300, 500, 800, 1100, 1500, 1900, 2400, 2900, 3500];
    static readonly long[] SPIN_LINE_CLEAR_SCORES = [800, 1200, 1600, 2000, 2400, 2800, 3200, 3600, 4000];
    static readonly long[] SPIN_MINI_LINE_CLEAR_SCORES = [200, 400, 600, 800, 1000, 1200, 1400, 1600, 1800];
    static readonly long[] PERFECT_CLEAR_LINE_CLEAR_SCORES = [800, 1200, 1800, 2000];
    const long COMBO_SCORE_BONUS = 50;
    const long CLEARLESS_SPIN_SCORE_BONUS = 400;
    const long CLEARLESS_MINI_SPIN_SCORE_BONUS = 100;
    
    private static long AddB2BMultiplierToScoreValue(long previousValue)
    {
        return previousValue + previousValue >> 1;
    }
    
    private void AddScore(long amount)
    {
        info.AddScore(amount);
        ScoreAdded?.Invoke(amount);
    }
    
    private void AddScoreFromPiecePlacement(SpinType spinned, bool clearedLines)
    {
        if(spinned != SpinType.NoSpin && !clearedLines)
        {
            AddScore(spinned == SpinType.TrueSpin ? CLEARLESS_SPIN_SCORE_BONUS : CLEARLESS_MINI_SPIN_SCORE_BONUS);
        }
    }
    
    private void AddScoreFromClear(int totalRowsCleared, PiecePlacementInformation pieceInfo)
    {
        long totalScore = 0;
        int level = info.GameLevel;
        switch(pieceInfo.Spin)
        {
            case SpinType.NoSpin:
                totalScore = NORMAL_LINE_CLEAR_SCORES[totalRowsCleared - 1];
                break;
            case SpinType.SpinMini:
                totalScore = SPIN_MINI_LINE_CLEAR_SCORES[totalRowsCleared - 1];
                break;
            case SpinType.TrueSpin:
                totalScore = SPIN_LINE_CLEAR_SCORES[totalRowsCleared - 1];
                break;
            default: break;
        }
        if(ComboValue > 0)
        {
            totalScore += COMBO_SCORE_BONUS * ComboValue;
        }
        ClearInfo clearInfo = new ClearInfo(totalRowsCleared, "-", pieceInfo.Spin);
        if(clearInfo.IsDifficult() && this.currentB2BValue >= 0)
        {
            totalScore = AddB2BMultiplierToScoreValue(totalScore);
        }
        // TODO: Detect perfect clears and add corresponding score
        totalScore *= long.Max(level, 1);
        AddScore(totalScore);
    }
    
    private void AddScoreFromHardDrop(int rowsDropped)
    {
        AddScore(HARD_DROP_SCORE_PER_ROW * rowsDropped);
    }
    
    private void AddScoreFromSoftDropSingleRow()
    {
        AddScore(SOFT_DROP_SCORE_PER_ROW);
    }
}