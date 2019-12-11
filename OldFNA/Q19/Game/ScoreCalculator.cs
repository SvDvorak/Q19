using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Q19
{
    class ScoreCalculator
    {
        public class ScoreResult
        {
            public int[] PlayerScores;
            public int[] PlayerTotalScores;
            public int BonusScore;
            public int StartTotalScore;
            public int TotalScore;

            public ScoreResult(int[] playerScores, int[] playerTotalScores, int bonusScore, int startTotalScore,
                int totalScore)
            {
                PlayerScores = playerScores;
                PlayerTotalScores = playerTotalScores;
                BonusScore = bonusScore;
                StartTotalScore = startTotalScore;
                TotalScore = totalScore;
            }
        }

        public ScoreResult Calculate(GameLevel previousLevel)
        {
            // HERE IS DATA TO BE DISPLAYED ON SCORE SCREEN
            var playerScores = new int[4]; // <- dim or hide players which aren't playing
            var playerTotalScores = new int[4]; // show on gameoverscreen <- dim or hide players which aren't playing
            int bonusScore = 0;
            int startTotalScore = Q19Game.Instance.Scene.TotalScore; // count from start to TotalScore after calculations below
            ///////////////////////

            // Count player scores
            Q19Game.Instance.Scene.Visit(e =>
            {
                if (e is PuzzlePiece pp && pp.IsPlacedInPuzzle && pp.PlacedBy != null)
                    playerScores[(int)pp.PlacedBy.Index]++;
            });

            Q19Game.Instance.Scene.Visit(e =>
            {
                if (e is Player p)
                {
                    var playerScore = playerScores[(int)p.Index];
                    p.TotalScore += playerScore;
                    playerTotalScores[(int)p.Index] = p.TotalScore;
                    Q19Game.Instance.Scene.TotalScore += playerScore;
                }
            });

            // Calc bonus
            var puzzleBonus = 0;
            previousLevel.Visit(e =>
            {
                if (e is Puzzle p)
                {
                    var filled = p.PlacedPieces() / (float)p.PiecesCount;
                    bool complete = filled == 1;
                    if (filled > 0.5f)
                    {
                        // see calc doc on drive for score balance
                        var bonus = filled - 0.5f;
                        bonus *= 10;
                        bonus *= bonus;
                        if (complete)
                            bonus *= 1.5f;
                        bonus /= 2;
                        puzzleBonus += (int)(bonus / (previousLevel.Puzzles.Length / 1.5f));
                    }
                }
            });

            // Relic penalty
            var relicBonus = previousLevel.RelicScore;

            // Time/Fire bonus
            var fireBonus = 0;
            previousLevel.Visit(e =>
            {
                if (e is Fire f)
                {
                    var fireLeft = MathHelper.Max(0, f.Light.LightSource.Size / previousLevel.Settings.MaxLight);
                    fireBonus += (int)(fireLeft * LevelSettings.FireBonusFull);
                }
            });

            bonusScore += puzzleBonus + relicBonus + fireBonus;
            Q19Game.DebugBonus = $"{Q19Game.Instance.Scene.TotalScore - startTotalScore} / {puzzleBonus} / {relicBonus} / {fireBonus}";
            Q19Game.Instance.Scene.TotalScore += bonusScore;

            return new ScoreResult(playerScores, playerTotalScores, bonusScore, startTotalScore, Q19Game.Instance.Scene.TotalScore);
        }
    }
}