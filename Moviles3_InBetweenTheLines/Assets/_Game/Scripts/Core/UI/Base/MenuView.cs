using UnityEngine;
using UnityEngine.Events;

namespace _Game.Scripts.Core.UI.Base
{
    public abstract class MenuView : MonoBehaviour
    {
        [Header("Configuración Base")]
        [SerializeField] protected bool _startHidden = true;
        
        public UnityEvent OnOpen;
        public UnityEvent OnClose;

        protected virtual void Awake()
        {
            if (_startHidden)
            {
                gameObject.SetActive(false);
            }
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
            OnOpen?.Invoke();
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
            OnClose?.Invoke();
        }
    }
}