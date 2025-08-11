using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClubNeko.Input
{
    /// <summary>
    /// Nintendo Switch風コントローラー入力管理
    /// Unity Input Systemを使用してJoy-Con操作をエミュレート
    /// </summary>
    public class SwitchInputController : MonoBehaviour
    {
        [Header("Input References")]
        public PlayerInput playerInput;
        
        [Header("Joy-Con State")]
        public JoyConState leftJoyCon = new JoyConState();
        public JoyConState rightJoyCon = new JoyConState();
        
        [Header("Button States")]
        public bool buttonA;  // 決定（東）
        public bool buttonB;  // キャンセル（南）
        public bool buttonX;  // メニュー（北）
        public bool buttonY;  // アクション（西）
        public bool buttonPlus;  // スタート
        public bool buttonMinus; // セレクト
        public bool buttonHome;  // ホーム
        public bool buttonCapture; // キャプチャー
        
        [Header("Analog Sticks")]
        public Vector2 leftStick;
        public Vector2 rightStick;
        public bool leftStickButton;  // L3
        public bool rightStickButton; // R3
        
        [Header("Triggers & Bumpers")]
        public bool buttonL;
        public bool buttonR;
        public bool buttonZL;
        public bool buttonZR;
        public float triggerLValue;
        public float triggerRValue;
        
        [Header("D-Pad")]
        public bool dpadUp;
        public bool dpadDown;
        public bool dpadLeft;
        public bool dpadRight;
        
        [Header("Motion Controls")]
        public bool motionControlsEnabled = true;
        public Vector3 leftJoyConAcceleration;
        public Vector3 rightJoyConAcceleration;
        public Vector3 leftJoyConGyro;
        public Vector3 rightJoyConGyro;
        
        [Header("HD Rumble")]
        public bool hdRumbleEnabled = true;
        private Gamepad gamepad;
        
        // イベント
        public event System.Action<ButtonType> OnButtonPressed;
        public event System.Action<ButtonType> OnButtonReleased;
        
        private void Start()
        {
            InitializeInput();
        }
        
        private void InitializeInput()
        {
            // Input Systemの初期化
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
                if (playerInput == null)
                {
                    playerInput = gameObject.AddComponent<PlayerInput>();
                }
            }
            
            // ゲームパッドの取得
            gamepad = Gamepad.current;
            
            // デバイス接続イベント
            InputSystem.onDeviceChange += OnDeviceChange;
        }
        
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is Gamepad)
            {
                if (change == InputDeviceChange.Added)
                {
                    gamepad = device as Gamepad;
                    Debug.Log($"[Switch Input] Controller connected: {device.displayName}");
                }
                else if (change == InputDeviceChange.Removed)
                {
                    if (gamepad == device)
                    {
                        gamepad = null;
                        Debug.Log($"[Switch Input] Controller disconnected: {device.displayName}");
                    }
                }
            }
        }
        
        private void Update()
        {
            if (gamepad == null)
            {
                gamepad = Gamepad.current;
                if (gamepad == null) return;
            }
            
            UpdateButtonStates();
            UpdateAnalogSticks();
            UpdateTriggers();
            UpdateDPad();
            UpdateMotionControls();
        }
        
        private void UpdateButtonStates()
        {
            // Aボタン（Switch配置：右）
            bool newButtonA = gamepad.buttonEast.isPressed;
            if (newButtonA && !buttonA) OnButtonPressed?.Invoke(ButtonType.A);
            if (!newButtonA && buttonA) OnButtonReleased?.Invoke(ButtonType.A);
            buttonA = newButtonA;
            
            // Bボタン（Switch配置：下）
            bool newButtonB = gamepad.buttonSouth.isPressed;
            if (newButtonB && !buttonB) OnButtonPressed?.Invoke(ButtonType.B);
            if (!newButtonB && buttonB) OnButtonReleased?.Invoke(ButtonType.B);
            buttonB = newButtonB;
            
            // Xボタン（Switch配置：上）
            bool newButtonX = gamepad.buttonNorth.isPressed;
            if (newButtonX && !buttonX) OnButtonPressed?.Invoke(ButtonType.X);
            if (!newButtonX && buttonX) OnButtonReleased?.Invoke(ButtonType.X);
            buttonX = newButtonX;
            
            // Yボタン（Switch配置：左）
            bool newButtonY = gamepad.buttonWest.isPressed;
            if (newButtonY && !buttonY) OnButtonPressed?.Invoke(ButtonType.Y);
            if (!newButtonY && buttonY) OnButtonReleased?.Invoke(ButtonType.Y);
            buttonY = newButtonY;
            
            // スタート/セレクト
            buttonPlus = gamepad.startButton.isPressed;
            buttonMinus = gamepad.selectButton.isPressed;
            
            // L/Rボタン
            buttonL = gamepad.leftShoulder.isPressed;
            buttonR = gamepad.rightShoulder.isPressed;
            
            // スティックボタン
            leftStickButton = gamepad.leftStickButton.isPressed;
            rightStickButton = gamepad.rightStickButton.isPressed;
        }
        
        private void UpdateAnalogSticks()
        {
            // 左スティック
            leftStick = gamepad.leftStick.ReadValue();
            leftJoyCon.stick = leftStick;
            
            // 右スティック
            rightStick = gamepad.rightStick.ReadValue();
            rightJoyCon.stick = rightStick;
            
            // デッドゾーン適用
            if (leftStick.magnitude < 0.1f) leftStick = Vector2.zero;
            if (rightStick.magnitude < 0.1f) rightStick = Vector2.zero;
        }
        
        private void UpdateTriggers()
        {
            // ZLトリガー
            buttonZL = gamepad.leftTrigger.isPressed;
            triggerLValue = gamepad.leftTrigger.ReadValue();
            
            // ZRトリガー
            buttonZR = gamepad.rightTrigger.isPressed;
            triggerRValue = gamepad.rightTrigger.ReadValue();
        }
        
        private void UpdateDPad()
        {
            Vector2 dpad = gamepad.dpad.ReadValue();
            dpadUp = dpad.y > 0.5f;
            dpadDown = dpad.y < -0.5f;
            dpadLeft = dpad.x < -0.5f;
            dpadRight = dpad.x > 0.5f;
        }
        
        private void UpdateMotionControls()
        {
            if (!motionControlsEnabled) return;
            
            // Unity Input Systemのモーションコントロール
            if (Accelerometer.current != null)
            {
                Vector3 acceleration = Accelerometer.current.acceleration.ReadValue();
                leftJoyConAcceleration = acceleration;
                rightJoyConAcceleration = acceleration;
            }
            
            if (UnityEngine.InputSystem.Gyroscope.current != null)
            {
                Vector3 gyro = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();
                leftJoyConGyro = gyro;
                rightJoyConGyro = gyro;
            }
        }
        
        /// <summary>
        /// HD振動を再生
        /// </summary>
        public void PlayHDRumble(float lowFrequency, float highFrequency, float duration)
        {
            if (!hdRumbleEnabled || gamepad == null) return;
            
            StartCoroutine(PlayRumbleCoroutine(lowFrequency, highFrequency, duration));
        }
        
        private IEnumerator PlayRumbleCoroutine(float lowFreq, float highFreq, float duration)
        {
            gamepad.SetMotorSpeeds(lowFreq, highFreq);
            yield return new WaitForSeconds(duration);
            gamepad.SetMotorSpeeds(0, 0);
        }
        
        /// <summary>
        /// ボタンアイコン取得（UI表示用）
        /// </summary>
        public string GetButtonIcon(ButtonType button)
        {
            switch (button)
            {
                case ButtonType.A: return "Ⓐ";
                case ButtonType.B: return "Ⓑ";
                case ButtonType.X: return "Ⓧ";
                case ButtonType.Y: return "Ⓨ";
                case ButtonType.L: return "Ⓛ";
                case ButtonType.R: return "Ⓡ";
                case ButtonType.ZL: return "ZL";
                case ButtonType.ZR: return "ZR";
                case ButtonType.Plus: return "＋";
                case ButtonType.Minus: return "－";
                default: return "?";
            }
        }
        
        /// <summary>
        /// コントローラーの接続状態
        /// </summary>
        public bool IsControllerConnected()
        {
            return gamepad != null && gamepad.enabled;
        }
        
        /// <summary>
        /// デバッグ表示
        /// </summary>
        private void OnGUI()
        {
            if (!Application.isEditor) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.Label("=== Switch Controller Debug ===");
            GUILayout.Label($"Connected: {IsControllerConnected()}");
            GUILayout.Label($"A: {buttonA} B: {buttonB} X: {buttonX} Y: {buttonY}");
            GUILayout.Label($"L: {buttonL} R: {buttonR}");
            GUILayout.Label($"ZL: {buttonZL} ({triggerLValue:F2}) ZR: {buttonZR} ({triggerRValue:F2})");
            GUILayout.Label($"Left Stick: {leftStick}");
            GUILayout.Label($"Right Stick: {rightStick}");
            GUILayout.Label($"D-Pad: ↑{dpadUp} ↓{dpadDown} ←{dpadLeft} →{dpadRight}");
            GUILayout.EndArea();
        }
        
        private void OnDestroy()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0, 0);
            }
        }
    }
    
    [System.Serializable]
    public class JoyConState
    {
        public Vector2 stick;
        public bool trigger;
        public bool bumper;
        public bool stickButton;
        public Vector3 acceleration;
        public Vector3 gyroscope;
    }
    
    public enum ButtonType
    {
        A, B, X, Y,
        L, R, ZL, ZR,
        Plus, Minus,
        Home, Capture,
        LeftStick, RightStick,
        DPadUp, DPadDown, DPadLeft, DPadRight
    }
}