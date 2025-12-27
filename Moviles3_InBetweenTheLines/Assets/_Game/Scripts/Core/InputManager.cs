using UnityEngine;

namespace _Game.Scripts.Core.InputSystem
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Configuración Micrófono")]
        [SerializeField] private float _micSensitivity = 100f;
        [SerializeField] private float _micThreshold = 0.1f;
        private AudioClip _micClip;
        private string _micDevice;
        private bool _isMicInitialized;

        [Header("Configuración Giroscopio")]
        [SerializeField] private float _gyroSmoothing = 5f;
        private Quaternion _gyroAttitude;

        private Vector3 _currentAcceleration;
        private Vector3 _editorRotation;

        public Vector3 Acceleration => _currentAcceleration;
        public Quaternion GyroRotation => _gyroAttitude;
        public float MicLoudness { get; private set; }
        public bool IsTouching => Input.touchCount > 0 || Input.GetMouseButton(0);
        public Vector2 TouchPosition => Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _currentAcceleration = Vector3.down;
            _gyroAttitude = Quaternion.identity;

            InitializeGyro();
            InitializeMicrophone();
        }

        private void Update()
        {
            UpdateGyro();
            UpdateAcceleration();
            UpdateMicrophone();
        }

        private void InitializeGyro()
        {
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
                _gyroAttitude = Input.gyro.attitude;
            }
        }

        private void InitializeMicrophone()
        {
            if (Microphone.devices.Length > 0)
            {
                _micDevice = Microphone.devices[0];
                _micClip = Microphone.Start(_micDevice, true, 10, 44100);
                _isMicInitialized = true;
            }
        }

        private void UpdateGyro()
        {
            #if UNITY_EDITOR
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    _editorRotation.y -= Input.GetAxis("Mouse X") * 5f;
                    _editorRotation.x += Input.GetAxis("Mouse Y") * 5f;
                }
                
                Quaternion simRot = Quaternion.Euler(_editorRotation.x, _editorRotation.y, 0);
                _gyroAttitude = Quaternion.Slerp(_gyroAttitude, simRot, Time.deltaTime * _gyroSmoothing);
            #else
                if (SystemInfo.supportsGyroscope)
                {
                    Quaternion q = Input.gyro.attitude;
                    Quaternion unityRot = new Quaternion(q.x, q.y, -q.z, -q.w);
                    unityRot = Quaternion.Euler(90, 90, 0) * unityRot;
                    _gyroAttitude = Quaternion.Slerp(_gyroAttitude, unityRot, Time.deltaTime * _gyroSmoothing);
                }
            #endif
        }

        private void UpdateAcceleration()
        {
            #if UNITY_EDITOR
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _currentAcceleration = Random.onUnitSphere * 3.0f;
                }
                else
                {
                    _currentAcceleration = Vector3.Lerp(_currentAcceleration, Vector3.down, Time.deltaTime * 5f);
                }

                float x = Input.GetAxis("Horizontal");
                float y = Input.GetAxis("Vertical");
                
                if (x != 0 || y != 0)
                {
                    _currentAcceleration += new Vector3(x, 0, y);
                }
            #else
                _currentAcceleration = Input.acceleration;
            #endif
        }

        private void UpdateMicrophone()
        {
            if (!_isMicInitialized) return;

            float[] data = new float[256];
            int micPosition = Microphone.GetPosition(_micDevice) - 256 + 1;
            if (micPosition < 0) return;

            _micClip.GetData(data, micPosition);
            
            float sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += data[i] * data[i];
            }
            
            float rms = Mathf.Sqrt(sum / 256);
            MicLoudness = rms * _micSensitivity;

            if (MicLoudness < _micThreshold) MicLoudness = 0;
        }

        public bool IsShaking(float threshold = 2.0f)
        {
            return _currentAcceleration.sqrMagnitude >= threshold * threshold;
        }
    }
}