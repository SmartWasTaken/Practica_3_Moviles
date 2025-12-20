using UnityEngine;

namespace _Game.Scripts.Core.Menu
{
    public interface IMenuState
    {
        void Enter();
        void Exit();
        void UpdateState();
    }
}