using UnityEngine;

namespace _Game.Scripts.Puzzles
{
    public class BatteryPuzzle : PuzzleBase
    {
        void Update()
        {
            if (isSolved) return;

#if !UNITY_EDITOR
            if (SystemInfo.batteryStatus == BatteryStatus.Charging)
            {
                CompletePuzzle();
            }
#endif
            
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CompletePuzzle();
            }
#endif
        }
    }
}