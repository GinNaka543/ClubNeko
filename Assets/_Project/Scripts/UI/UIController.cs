using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClubNeko.UI
{
    /// <summary>
    /// ゲーム内UIコントローラー
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [Header("HUD Elements")]
        public GameObject hudPanel;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI holeNumberText;
        public TextMeshProUGUI parText;
        public TextMeshProUGUI strokeText;
        public TextMeshProUGUI windText;
        public Image weatherIcon;
        
        [Header("Weapon Selection")]
        public GameObject weaponPanel;
        public Button[] weaponButtons;
        public Image currentWeaponIcon;
        public TextMeshProUGUI weaponNameText;
        public Slider powerSlider;
        
        [Header("Mini Map")]
        public RawImage miniMapImage;
        public RectTransform playerMarker;
        public RectTransform holeMarker;
        public RectTransform[] teamMarkers;
        
        [Header("Team Info")]
        public GameObject teamPanel;
        public TextMeshProUGUI[] teamMemberNames;
        public Image[] teamMemberAvatars;
        public TextMeshProUGUI teamScoreText;
        
        [Header("Notifications")]
        public GameObject notificationPanel;
        public TextMeshProUGUI notificationText;
        public float notificationDuration = 3f;
        
        [Header("Cat UI Elements")]
        public Image catMascot;
        public Animator catAnimator;
        public GameObject[] catPawPrints;
        
        [Header("Weather Icons")]
        public Sprite sunnyIcon;
        public Sprite rainyIcon;
        public Sprite windyIcon;
        public Sprite stormyIcon;
        public Sprite foggyIcon;
        
        private Queue<string> notificationQueue = new Queue<string>();
        private Coroutine notificationCoroutine;
        
        private void Start()
        {
            InitializeUI();
            SetupWeaponButtons();
        }
        
        private void InitializeUI()
        {
            // HUDの初期化
            if (hudPanel != null)
                hudPanel.SetActive(true);
            
            // 武器パネルの初期化
            if (weaponPanel != null)
                weaponPanel.SetActive(true);
            
            // チームパネルの初期化
            if (teamPanel != null)
                teamPanel.SetActive(false);
            
            // 通知パネルの初期化
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
            
            // 猫マスコット初期化
            if (catMascot != null)
            {
                StartCoroutine(AnimateCatMascot());
            }
        }
        
        private void SetupWeaponButtons()
        {
            if (weaponButtons == null) return;
            
            for (int i = 0; i < weaponButtons.Length; i++)
            {
                int weaponIndex = i;
                weaponButtons[i].onClick.AddListener(() => OnWeaponSelected(weaponIndex));
            }
        }
        
        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score:+#;-#;0}";
                
                // スコアに応じて色を変更
                if (score < 0)
                    scoreText.color = Color.green;
                else if (score > 0)
                    scoreText.color = Color.red;
                else
                    scoreText.color = Color.white;
            }
        }
        
        public void UpdateHoleInfo(int holeNumber, int par)
        {
            if (holeNumberText != null)
                holeNumberText.text = $"Hole {holeNumber}";
            
            if (parText != null)
                parText.text = $"Par {par}";
        }
        
        public void UpdateStrokeCount(int strokes)
        {
            if (strokeText != null)
                strokeText.text = $"Strokes: {strokes}";
        }
        
        public void UpdateWeather(WeatherType weather, Vector3 windForce)
        {
            // 天候アイコン更新
            if (weatherIcon != null)
            {
                switch (weather)
                {
                    case WeatherType.Sunny:
                        weatherIcon.sprite = sunnyIcon;
                        break;
                    case WeatherType.Rainy:
                        weatherIcon.sprite = rainyIcon;
                        break;
                    case WeatherType.Windy:
                        weatherIcon.sprite = windyIcon;
                        break;
                    case WeatherType.Stormy:
                        weatherIcon.sprite = stormyIcon;
                        break;
                    case WeatherType.Foggy:
                        weatherIcon.sprite = foggyIcon;
                        break;
                }
            }
            
            // 風力表示更新
            if (windText != null)
            {
                float windStrength = windForce.magnitude;
                string windDirection = GetWindDirection(windForce);
                windText.text = $"Wind: {windStrength:F1}m/s {windDirection}";
            }
        }
        
        private string GetWindDirection(Vector3 wind)
        {
            if (wind.magnitude < 0.1f) return "";
            
            float angle = Mathf.Atan2(wind.x, wind.z) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            
            if (angle < 22.5f || angle >= 337.5f) return "↑";
            if (angle < 67.5f) return "↗";
            if (angle < 112.5f) return "→";
            if (angle < 157.5f) return "↘";
            if (angle < 202.5f) return "↓";
            if (angle < 247.5f) return "↙";
            if (angle < 292.5f) return "←";
            return "↖";
        }
        
        public void OnWeaponSelected(int weaponIndex)
        {
            // 武器選択エフェクト
            PlayCatPawEffect(weaponButtons[weaponIndex].transform.position);
            
            // 武器名更新
            string[] weaponNames = { "Driver", "Iron", "Putter", "Bazooka", "Bat", "Air Cannon" };
            if (weaponNameText != null && weaponIndex < weaponNames.Length)
            {
                weaponNameText.text = weaponNames[weaponIndex];
            }
            
            // 猫アニメーション
            if (catAnimator != null)
            {
                catAnimator.SetTrigger("WeaponChange");
            }
        }
        
        public void UpdatePowerSlider(float power)
        {
            if (powerSlider != null)
            {
                powerSlider.value = power;
                
                // パワーに応じて色を変更
                Color sliderColor = Color.Lerp(Color.green, Color.red, power);
                powerSlider.fillRect.GetComponent<Image>().color = sliderColor;
            }
        }
        
        public void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            notificationQueue.Enqueue(message);
            
            if (notificationCoroutine == null)
            {
                notificationCoroutine = StartCoroutine(ProcessNotifications());
            }
        }
        
        private IEnumerator ProcessNotifications()
        {
            while (notificationQueue.Count > 0)
            {
                string message = notificationQueue.Dequeue();
                
                if (notificationPanel != null && notificationText != null)
                {
                    notificationText.text = message;
                    notificationPanel.SetActive(true);
                    
                    // フェードイン
                    CanvasGroup canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                        canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
                    
                    float fadeTime = 0.3f;
                    float timer = 0;
                    
                    while (timer < fadeTime)
                    {
                        timer += Time.deltaTime;
                        canvasGroup.alpha = timer / fadeTime;
                        yield return null;
                    }
                    
                    yield return new WaitForSeconds(notificationDuration);
                    
                    // フェードアウト
                    timer = 0;
                    while (timer < fadeTime)
                    {
                        timer += Time.deltaTime;
                        canvasGroup.alpha = 1f - (timer / fadeTime);
                        yield return null;
                    }
                    
                    notificationPanel.SetActive(false);
                }
                
                yield return new WaitForSeconds(0.5f);
            }
            
            notificationCoroutine = null;
        }
        
        public void UpdateTeamInfo(string[] memberNames, int teamScore)
        {
            if (teamPanel != null)
                teamPanel.SetActive(true);
            
            // メンバー名更新
            if (teamMemberNames != null)
            {
                for (int i = 0; i < teamMemberNames.Length && i < memberNames.Length; i++)
                {
                    teamMemberNames[i].text = memberNames[i];
                }
            }
            
            // チームスコア更新
            if (teamScoreText != null)
            {
                teamScoreText.text = $"Team Score: {teamScore:+#;-#;0}";
            }
        }
        
        public void UpdateMiniMap(Vector3 playerPos, Vector3 holePos)
        {
            if (playerMarker != null && holeMarker != null)
            {
                // ミニマップ上の位置を計算
                Vector2 mapSize = new Vector2(200f, 200f);
                float worldToMapScale = 0.5f;
                
                Vector2 playerMapPos = new Vector2(
                    playerPos.x * worldToMapScale,
                    playerPos.z * worldToMapScale
                );
                
                Vector2 holeMapPos = new Vector2(
                    holePos.x * worldToMapScale,
                    holePos.z * worldToMapScale
                );
                
                playerMarker.anchoredPosition = playerMapPos;
                holeMarker.anchoredPosition = holeMapPos;
            }
        }
        
        private void PlayCatPawEffect(Vector3 worldPos)
        {
            if (catPawPrints != null && catPawPrints.Length > 0)
            {
                // ワールド座標をスクリーン座標に変換
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                
                GameObject pawPrint = catPawPrints[Random.Range(0, catPawPrints.Length)];
                if (pawPrint != null)
                {
                    pawPrint.transform.position = screenPos;
                    pawPrint.SetActive(true);
                    
                    // 1秒後に非表示
                    StartCoroutine(HidePawPrint(pawPrint));
                }
            }
        }
        
        private IEnumerator HidePawPrint(GameObject pawPrint)
        {
            yield return new WaitForSeconds(1f);
            pawPrint.SetActive(false);
        }
        
        private IEnumerator AnimateCatMascot()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(5f, 10f));
                
                if (catAnimator != null)
                {
                    // ランダムなアニメーション再生
                    string[] animations = { "Meow", "Stretch", "Tail", "Blink" };
                    catAnimator.SetTrigger(animations[Random.Range(0, animations.Length)]);
                }
            }
        }
        
        public void ShowVictoryScreen(bool isWinner)
        {
            if (isWinner)
            {
                ShowNotification("🐱 Victory! Nyaa~", NotificationType.Victory);
                if (catAnimator != null)
                    catAnimator.SetTrigger("Victory");
            }
            else
            {
                ShowNotification("Better luck next time!", NotificationType.Defeat);
                if (catAnimator != null)
                    catAnimator.SetTrigger("Defeat");
            }
        }
    }
    
    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Victory,
        Defeat
    }
    
    public enum WeatherType
    {
        Sunny,
        Rainy,
        Windy,
        Stormy,
        Foggy
    }
}