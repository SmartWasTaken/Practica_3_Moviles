using UnityEngine;

namespace _Game.Scripts.Visuals
{
    public class LiquidWobble : MonoBehaviour
    {
        public Renderer liquidRenderer;
        
        [Header("Configuración Líquido")]
        public float MaxWobble = 0.05f;
        public float WobbleSpeed = 1f;
        public float Recovery = 1f;

        // Estado interno
        private float _wobbleAmountX;
        private float _wobbleAmountZ;
        private float _wobbleAmountToAddX;
        private float _wobbleAmountToAddZ;
        private float _pulse;
        private float _time = 0.5f;

        private void Update()
        {
            _time += Time.deltaTime;

            _wobbleAmountToAddX = Mathf.Lerp(_wobbleAmountToAddX, 0, Time.deltaTime * Recovery);
            _wobbleAmountToAddZ = Mathf.Lerp(_wobbleAmountToAddZ, 0, Time.deltaTime * Recovery);

            _pulse = 2 * Mathf.PI * WobbleSpeed;
            _wobbleAmountX = _wobbleAmountToAddX * Mathf.Sin(_pulse * _time);
            _wobbleAmountZ = _wobbleAmountToAddZ * Mathf.Sin(_pulse * _time);
            if (liquidRenderer != null)
            {
                liquidRenderer.material.SetFloat("_WobbleX", _wobbleAmountX);
                liquidRenderer.material.SetFloat("_WobbleZ", _wobbleAmountZ);
            }

            Vector3 velocity = (lastPos - transform.position) / Time.deltaTime;
            Vector3 angularVelocity = GetAngularVelocity();
            lastPos = transform.position;
            lastRot = transform.rotation;
        }

        private Vector3 lastPos;
        private Quaternion lastRot;

        public void AddWobble(float amount)
        {
            _wobbleAmountToAddX += amount;
            _wobbleAmountToAddZ += amount;
            
            _wobbleAmountToAddX = Mathf.Clamp(_wobbleAmountToAddX, -MaxWobble, MaxWobble);
            _wobbleAmountToAddZ = Mathf.Clamp(_wobbleAmountToAddZ, -MaxWobble, MaxWobble);
        }
        
        Vector3 GetAngularVelocity()
        {
            Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRot);
            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
            return axis * (angle * Mathf.Deg2Rad / Time.deltaTime);
        }
    }
}