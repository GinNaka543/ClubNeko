using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ClubNeko.Golf
{
    /// <summary>
    /// 動的天候システム - ゼルダTotK風の美しい天候変化
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        [Header("Current Weather")]
        public WeatherType currentWeather = WeatherType.Sunny;
        public float weatherIntensity = 1.0f;
        
        [Header("Weather Prefabs")]
        public GameObject rainEffect;
        public GameObject windEffect;
        public GameObject fogEffect;
        public GameObject stormEffect;
        
        [Header("Wind Settings")]
        public Vector3 windDirection = Vector3.right;
        public float windStrength = 0f;
        public float windVariation = 0.2f;
        
        [Header("Visual Settings")]
        public Light sunLight;
        public Light moonLight;
        public Gradient sunColor;
        public Gradient fogColor;
        public AnimationCurve fogDensityCurve;
        
        [Header("Post Processing")]
        public Volume weatherVolume;
        public VolumeProfile sunnyProfile;
        public VolumeProfile rainyProfile;
        public VolumeProfile foggyProfile;
        public VolumeProfile stormyProfile;
        
        [Header("Sky Settings")]
        public Material skyboxMaterial;
        public Gradient skyTintGradient;
        public AnimationCurve cloudDensity;
        
        [Header("Audio")]
        public AudioSource weatherAudioSource;
        public AudioClip rainSound;
        public AudioClip windSound;
        public AudioClip thunderSound;
        
        private float transitionTimer = 0f;
        private float transitionDuration = 5f;
        private WeatherType targetWeather;
        private bool isTransitioning = false;
        
        private ParticleSystem rainParticles;
        private ParticleSystem windParticles;
        private WindZone windZone;
        
        private void Start()
        {
            InitializeWeatherEffects();
            ApplyWeather(currentWeather, true);
        }
        
        private void Update()
        {
            // 天候遷移の更新
            if (isTransitioning)
            {
                UpdateWeatherTransition();
            }
            
            // 風の変動
            UpdateWindVariation();
            
            // 時間による光の変化
            UpdateLighting();
        }
        
        public void ChangeWeather(WeatherType newWeather, float duration = 5f)
        {
            if (newWeather == currentWeather) return;
            
            targetWeather = newWeather;
            transitionDuration = duration;
            transitionTimer = 0f;
            isTransitioning = true;
        }
        
        private void UpdateWeatherTransition()
        {
            transitionTimer += Time.deltaTime;
            float t = transitionTimer / transitionDuration;
            
            if (t >= 1f)
            {
                t = 1f;
                isTransitioning = false;
                currentWeather = targetWeather;
            }
            
            // スムーズな遷移
            weatherIntensity = Mathf.Lerp(0f, 1f, t);
            ApplyWeather(targetWeather, false);
        }
        
        private void ApplyWeather(WeatherType weather, bool immediate = false)
        {
            float intensity = immediate ? 1f : weatherIntensity;
            
            switch (weather)
            {
                case WeatherType.Sunny:
                    ApplySunnyWeather(intensity);
                    break;
                case WeatherType.Rainy:
                    ApplyRainyWeather(intensity);
                    break;
                case WeatherType.Windy:
                    ApplyWindyWeather(intensity);
                    break;
                case WeatherType.Stormy:
                    ApplyStormyWeather(intensity);
                    break;
                case WeatherType.Foggy:
                    ApplyFoggyWeather(intensity);
                    break;
            }
        }
        
        private void ApplySunnyWeather(float intensity)
        {
            // 晴天の設定
            windStrength = Mathf.Lerp(windStrength, 5f, intensity);
            
            if (sunLight != null)
            {
                sunLight.intensity = Mathf.Lerp(0.5f, 1.2f, intensity);
                sunLight.color = sunColor.Evaluate(intensity);
            }
            
            // エフェクトを無効化
            SetWeatherEffectActive(rainEffect, false);
            SetWeatherEffectActive(fogEffect, false);
            
            // ポストプロセシング
            if (weatherVolume != null && sunnyProfile != null)
            {
                weatherVolume.profile = sunnyProfile;
            }
            
            // オーディオ
            FadeWeatherAudio(null, intensity);
        }
        
        private void ApplyRainyWeather(float intensity)
        {
            // 雨天の設定
            windStrength = Mathf.Lerp(windStrength, 15f, intensity);
            
            if (sunLight != null)
            {
                sunLight.intensity = Mathf.Lerp(1.2f, 0.3f, intensity);
                sunLight.color = Color.Lerp(Color.white, new Color(0.7f, 0.7f, 0.8f), intensity);
            }
            
            // 雨エフェクト
            SetWeatherEffectActive(rainEffect, true);
            if (rainParticles != null)
            {
                var emission = rainParticles.emission;
                emission.rateOverTime = Mathf.Lerp(0, 500, intensity);
            }
            
            // ポストプロセシング
            if (weatherVolume != null && rainyProfile != null)
            {
                weatherVolume.profile = rainyProfile;
            }
            
            // オーディオ
            FadeWeatherAudio(rainSound, intensity);
        }
        
        private void ApplyWindyWeather(float intensity)
        {
            // 強風の設定
            windStrength = Mathf.Lerp(5f, 30f, intensity);
            
            // 風エフェクト
            SetWeatherEffectActive(windEffect, true);
            if (windParticles != null)
            {
                var velocityOverLifetime = windParticles.velocityOverLifetime;
                velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(windStrength);
            }
            
            // オーディオ
            FadeWeatherAudio(windSound, intensity);
        }
        
        private void ApplyStormyWeather(float intensity)
        {
            // 嵐の設定
            windStrength = Mathf.Lerp(10f, 40f, intensity);
            
            if (sunLight != null)
            {
                sunLight.intensity = Mathf.Lerp(0.5f, 0.1f, intensity);
                sunLight.color = Color.Lerp(Color.white, new Color(0.4f, 0.4f, 0.5f), intensity);
            }
            
            // 雨と風のエフェクト
            SetWeatherEffectActive(rainEffect, true);
            SetWeatherEffectActive(windEffect, true);
            SetWeatherEffectActive(stormEffect, true);
            
            // 雷のタイミング
            if (Random.Range(0f, 1f) < 0.01f * intensity)
            {
                StartCoroutine(LightningStrike());
            }
            
            // ポストプロセシング
            if (weatherVolume != null && stormyProfile != null)
            {
                weatherVolume.profile = stormyProfile;
            }
            
            // オーディオ
            FadeWeatherAudio(thunderSound, intensity);
        }
        
        private void ApplyFoggyWeather(float intensity)
        {
            // 霧の設定
            windStrength = Mathf.Lerp(windStrength, 3f, intensity);
            
            if (sunLight != null)
            {
                sunLight.intensity = Mathf.Lerp(1f, 0.2f, intensity);
            }
            
            // 霧エフェクト
            SetWeatherEffectActive(fogEffect, true);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = Mathf.Lerp(0.01f, 0.15f, intensity);
            RenderSettings.fogColor = fogColor.Evaluate(intensity);
            
            // ポストプロセシング
            if (weatherVolume != null && foggyProfile != null)
            {
                weatherVolume.profile = foggyProfile;
            }
        }
        
        private void UpdateWindVariation()
        {
            // 風のランダムな変動
            float variation = Mathf.PerlinNoise(Time.time * 0.1f, 0f) * windVariation;
            windDirection = Quaternion.Euler(0, variation * 30f, 0) * windDirection.normalized;
            
            if (windZone != null)
            {
                windZone.windMain = windStrength;
                windZone.windTurbulence = windStrength * 0.5f;
                windZone.transform.rotation = Quaternion.LookRotation(windDirection);
            }
        }
        
        private void UpdateLighting()
        {
            // 時間帯による光の変化（簡易版）
            float timeOfDay = (Time.time % 120f) / 120f; // 2分で1日
            
            if (sunLight != null)
            {
                float sunAngle = timeOfDay * 360f - 90f;
                sunLight.transform.rotation = Quaternion.Euler(sunAngle, 30f, 0f);
            }
            
            // スカイボックスの更新
            if (skyboxMaterial != null)
            {
                skyboxMaterial.SetFloat("_Rotation", timeOfDay * 360f);
                skyboxMaterial.SetColor("_Tint", skyTintGradient.Evaluate(timeOfDay));
            }
        }
        
        private IEnumerator LightningStrike()
        {
            // 雷の閃光
            float originalIntensity = sunLight.intensity;
            sunLight.intensity = 3f;
            sunLight.color = Color.white;
            
            yield return new WaitForSeconds(0.1f);
            
            sunLight.intensity = originalIntensity;
            
            // 雷鳴
            if (weatherAudioSource != null && thunderSound != null)
            {
                weatherAudioSource.PlayOneShot(thunderSound);
            }
        }
        
        private void InitializeWeatherEffects()
        {
            // パーティクルシステムの取得
            if (rainEffect != null)
                rainParticles = rainEffect.GetComponent<ParticleSystem>();
            
            if (windEffect != null)
                windParticles = windEffect.GetComponent<ParticleSystem>();
            
            // WindZoneの作成
            GameObject windZoneObj = new GameObject("WindZone");
            windZoneObj.transform.parent = transform;
            windZone = windZoneObj.AddComponent<WindZone>();
            windZone.mode = WindZoneMode.Directional;
        }
        
        private void SetWeatherEffectActive(GameObject effect, bool active)
        {
            if (effect != null)
            {
                effect.SetActive(active);
            }
        }
        
        private void FadeWeatherAudio(AudioClip clip, float volume)
        {
            if (weatherAudioSource == null) return;
            
            if (clip != null)
            {
                if (weatherAudioSource.clip != clip)
                {
                    weatherAudioSource.clip = clip;
                    weatherAudioSource.Play();
                    weatherAudioSource.loop = true;
                }
                weatherAudioSource.volume = Mathf.Lerp(weatherAudioSource.volume, volume, Time.deltaTime);
            }
            else
            {
                weatherAudioSource.volume = Mathf.Lerp(weatherAudioSource.volume, 0f, Time.deltaTime);
                if (weatherAudioSource.volume < 0.01f)
                {
                    weatherAudioSource.Stop();
                }
            }
        }
        
        public Vector3 GetWindForce()
        {
            return windDirection * windStrength * 0.1f;
        }
        
        public float GetVisibilityModifier()
        {
            return currentWeather switch
            {
                WeatherType.Sunny => 1.0f,
                WeatherType.Rainy => 0.7f,
                WeatherType.Windy => 0.9f,
                WeatherType.Stormy => 0.1f,
                WeatherType.Foggy => 0.05f,
                _ => 1.0f
            };
        }
        
        public float GetPowerModifier()
        {
            return currentWeather switch
            {
                WeatherType.Sunny => 1.0f,
                WeatherType.Rainy => 0.8f,
                WeatherType.Windy => 0.9f,
                WeatherType.Stormy => 0.6f,
                WeatherType.Foggy => 0.95f,
                _ => 1.0f
            };
        }
    }
    
    public enum WeatherType
    {
        Sunny,    // 晴天
        Rainy,    // 雨
        Windy,    // 強風
        Stormy,   // 嵐
        Foggy     // 霧
    }
}