using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ClubNeko.Core;

namespace ClubNeko.UI
{
    /// <summary>
    /// メインメニュー画面の管理
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject mainPanel;
        public GameObject modeSelectPanel;
        public GameObject characterSelectPanel;
        public GameObject settingsPanel;
        public GameObject creditsPanel;
        
        [Header("Main Menu Buttons")]
        public Button playButton;
        public Button houseButton;
        public Button characterButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;
        
        [Header("Mode Select")]
        public Button singlePlayerButton;
        public Button multiplayerButton;
        public Button tournamentButton;
        public Button backFromModeButton;
        
        [Header("Cat Elements")]
        public ParticleSystem catPawEffect;
        public AudioSource meowSound;
        public GameObject[] catDecorations;
        
        [Header("Animations")]
        public Animator logoAnimator;
        public float panelTransitionTime = 0.3f;
        
        private GameManager gameManager;
        private AudioManager audioManager;
        
        private void Start()
        {
            InitializeUI();
            SetupButtonListeners();
            ShowMainPanel();
            
            // 猫のデコレーションをランダムに配置
            PlaceCatDecorations();
        }
        
        private void InitializeUI()
        {
            gameManager = GameManager.Instance;
            audioManager = FindFirstObjectByType<AudioManager>();
            
            // ロゴアニメーション開始
            if (logoAnimator != null)
            {
                logoAnimator.SetTrigger("Intro");
            }
            
            // BGM開始
            if (audioManager != null)
            {
                audioManager.PlayMenuMusic();
            }
        }
        
        private void SetupButtonListeners()
        {
            // メインメニューボタン
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);
            
            if (houseButton != null)
                houseButton.onClick.AddListener(OnHouseClicked);
            
            if (characterButton != null)
                characterButton.onClick.AddListener(OnCharacterClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            
            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
            
            // モード選択ボタン
            if (singlePlayerButton != null)
                singlePlayerButton.onClick.AddListener(OnSinglePlayerClicked);
            
            if (multiplayerButton != null)
                multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
            
            if (tournamentButton != null)
                tournamentButton.onClick.AddListener(OnTournamentClicked);
            
            if (backFromModeButton != null)
                backFromModeButton.onClick.AddListener(() => ShowMainPanel());
        }
        
        private void OnPlayClicked()
        {
            PlayCatEffect();
            ShowPanel(modeSelectPanel);
        }
        
        private void OnHouseClicked()
        {
            PlayCatEffect();
            StartCoroutine(LoadHouseScene());
        }
        
        private void OnCharacterClicked()
        {
            PlayCatEffect();
            ShowPanel(characterSelectPanel);
        }
        
        private void OnSettingsClicked()
        {
            PlayCatEffect();
            ShowPanel(settingsPanel);
        }
        
        private void OnCreditsClicked()
        {
            PlayCatEffect();
            ShowPanel(creditsPanel);
        }
        
        private void OnQuitClicked()
        {
            PlayCatEffect();
            StartCoroutine(QuitGame());
        }
        
        private void OnSinglePlayerClicked()
        {
            PlayCatEffect();
            gameManager.gameMode = GameMode.SinglePlayer;
            StartCoroutine(LoadGolfScene());
        }
        
        private void OnMultiplayerClicked()
        {
            PlayCatEffect();
            gameManager.gameMode = GameMode.Multiplayer;
            // マルチプレイヤーロビーへ
            ShowPanel(null); // TODO: マルチプレイヤーロビーパネル
        }
        
        private void OnTournamentClicked()
        {
            PlayCatEffect();
            gameManager.gameMode = GameMode.Tournament;
            // トーナメント設定へ
            ShowPanel(null); // TODO: トーナメントパネル
        }
        
        private void ShowPanel(GameObject panel)
        {
            StartCoroutine(TransitionToPanel(panel));
        }
        
        private void ShowMainPanel()
        {
            ShowPanel(mainPanel);
        }
        
        private IEnumerator TransitionToPanel(GameObject targetPanel)
        {
            // 現在のパネルをフェードアウト
            GameObject currentPanel = GetActivePanel();
            if (currentPanel != null)
            {
                CanvasGroup currentGroup = currentPanel.GetComponent<CanvasGroup>();
                if (currentGroup == null)
                    currentGroup = currentPanel.AddComponent<CanvasGroup>();
                
                float timer = 0;
                while (timer < panelTransitionTime)
                {
                    timer += Time.deltaTime;
                    currentGroup.alpha = 1f - (timer / panelTransitionTime);
                    yield return null;
                }
                
                currentPanel.SetActive(false);
            }
            
            // ターゲットパネルをフェードイン
            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
                CanvasGroup targetGroup = targetPanel.GetComponent<CanvasGroup>();
                if (targetGroup == null)
                    targetGroup = targetPanel.AddComponent<CanvasGroup>();
                
                float timer = 0;
                targetGroup.alpha = 0;
                while (timer < panelTransitionTime)
                {
                    timer += Time.deltaTime;
                    targetGroup.alpha = timer / panelTransitionTime;
                    yield return null;
                }
            }
        }
        
        private GameObject GetActivePanel()
        {
            GameObject[] panels = { mainPanel, modeSelectPanel, characterSelectPanel, settingsPanel, creditsPanel };
            foreach (var panel in panels)
            {
                if (panel != null && panel.activeSelf)
                    return panel;
            }
            return null;
        }
        
        private IEnumerator LoadGolfScene()
        {
            // ローディング画面表示
            ShowLoadingScreen(true);
            
            yield return new WaitForSeconds(0.5f);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GolfCourse");
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                UpdateLoadingProgress(progress);
                yield return null;
            }
        }
        
        private IEnumerator LoadHouseScene()
        {
            ShowLoadingScreen(true);
            
            yield return new WaitForSeconds(0.5f);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("House");
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                UpdateLoadingProgress(progress);
                yield return null;
            }
        }
        
        private void PlayCatEffect()
        {
            // 猫の足跡エフェクト
            if (catPawEffect != null)
            {
                catPawEffect.Play();
            }
            
            // ニャーンサウンド
            if (meowSound != null && Random.Range(0f, 1f) > 0.7f)
            {
                meowSound.pitch = Random.Range(0.9f, 1.1f);
                meowSound.Play();
            }
        }
        
        private void PlaceCatDecorations()
        {
            if (catDecorations == null) return;
            
            foreach (var decoration in catDecorations)
            {
                if (decoration != null)
                {
                    // ランダムな揺れアニメーション
                    CatDecoration catDeco = decoration.AddComponent<CatDecoration>();
                    catDeco.StartAnimation();
                }
            }
        }
        
        private void ShowLoadingScreen(bool show)
        {
            // TODO: ローディング画面の表示/非表示
        }
        
        private void UpdateLoadingProgress(float progress)
        {
            // TODO: ローディング進捗更新
        }
        
        private IEnumerator QuitGame()
        {
            // フェードアウト
            yield return new WaitForSeconds(0.5f);
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
    
    /// <summary>
    /// 猫デコレーションアニメーション
    /// </summary>
    public class CatDecoration : MonoBehaviour
    {
        private float swaySpeed;
        private float swayAmount;
        private Vector3 originalPosition;
        
        public void StartAnimation()
        {
            swaySpeed = Random.Range(1f, 3f);
            swayAmount = Random.Range(0.1f, 0.3f);
            originalPosition = transform.position;
        }
        
        private void Update()
        {
            if (originalPosition != Vector3.zero)
            {
                float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
                transform.position = originalPosition + new Vector3(sway, 0, 0);
                transform.rotation = Quaternion.Euler(0, 0, sway * 10f);
            }
        }
    }
    
    /// <summary>
    /// オーディオマネージャー（仮）
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public AudioSource bgmSource;
        public AudioClip menuMusic;
        public AudioClip gameMusic;
        
        public void PlayMenuMusic()
        {
            if (bgmSource != null && menuMusic != null)
            {
                bgmSource.clip = menuMusic;
                bgmSource.Play();
            }
        }
        
        public void PlayGameMusic()
        {
            if (bgmSource != null && gameMusic != null)
            {
                bgmSource.clip = gameMusic;
                bgmSource.Play();
            }
        }
    }
}