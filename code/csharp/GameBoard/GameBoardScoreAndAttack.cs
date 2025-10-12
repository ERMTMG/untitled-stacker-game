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
    static readonly long[] PERFECT_CLEAR_LINE_CLEAR_SCORES = [800, 1200, 1800, 2000, 3000];
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
    
    private void AddScoreFromClear(int totalRowsCleared, PiecePlacementInformation pieceInfo, bool perfectClear)
    {
        long totalScore = 0;
        int level = info.GameLevel;
        if(perfectClear)
        {
            totalScore = PERFECT_CLEAR_LINE_CLEAR_SCORES[totalRowsCleared - 1];
        } else switch(pieceInfo.Spin)
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
    
    static readonly decimal[] NORMAL_LINE_CLEAR_ATTACK = [0, 0.5m, 1, 2, 4, 6, 10, 14, 20, 26, 34];
    static readonly decimal[] SPIN_LINE_CLEAR_ATTACK = [0, 2, 4, 6, 8, 12, 16, 22, 30, 40, 52];
    const decimal PERFECT_CLEAR_ATTACK = 10;
    
    private decimal GetBackToBackBonus(decimal normalAttack)
    {
        return 1;
    }
    
    private decimal GetCorrespondingComboAttack(decimal normalAttack)
    {
        return normalAttack * (1 + ComboValue * 0.1m);
    }
    
    private int GetAttackFromClear(int totalRowsCleared, PiecePlacementInformation pieceInfo, bool perfectClear)
    {
        decimal totalAttack = 0;
        switch(pieceInfo.Spin)
        {
            case SpinType.TrueSpin:
                totalAttack = (totalRowsCleared >= SPIN_LINE_CLEAR_ATTACK.Length) ? 
                    SPIN_LINE_CLEAR_ATTACK[^1] : 
                    SPIN_LINE_CLEAR_ATTACK[totalRowsCleared];
                break;
            default:
                totalAttack = (totalRowsCleared >= NORMAL_LINE_CLEAR_ATTACK.Length) ?
                    NORMAL_LINE_CLEAR_ATTACK[^1] :
                    NORMAL_LINE_CLEAR_ATTACK[totalRowsCleared];
                break;
        }
        if(currentB2BValue > 0)
        {
            totalAttack += GetBackToBackBonus(totalAttack);
        }
        if(ComboActive)
        {
            totalAttack = GetCorrespondingComboAttack(totalAttack);
        }
        if(perfectClear)
        {
            totalAttack += PERFECT_CLEAR_ATTACK;
        }
        return (int)totalAttack;
    }
}