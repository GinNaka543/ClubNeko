using UnityEngine;
using System.Collections.Generic;

namespace ClubNeko.Data
{
    /// <summary>
    /// キャラクタープロファイル用ScriptableObject
    /// VRMキャラクターの設定と管理
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterProfile", menuName = "Club Neko/Character Profile")]
    public class CharacterProfile : ScriptableObject
    {
        [Header("Basic Info")]
        public string characterName = "New Character";
        public string characterID;
        public Sprite characterIcon;
        public GameObject vrmPrefab;
        
        [Header("Character Type")]
        public CharacterType characterType = CharacterType.Player;
        public CharacterRole role = CharacterRole.AllRounder;
        
        [Header("Stats")]
        [Range(1, 10)] public int power = 5;
        [Range(1, 10)] public int speed = 5;
        [Range(1, 10)] public int accuracy = 5;
        [Range(1, 10)] public int stamina = 5;
        [Range(1, 10)] public int luck = 5;
        
        [Header("Golf Abilities")]
        public float drivingDistance = 250f;
        public float puttingAccuracy = 0.8f;
        public float windResistance = 0.5f;
        public float weatherAdaptability = 0.5f;
        
        [Header("Special Abilities")]
        public List<SpecialAbility> specialAbilities = new List<SpecialAbility>();
        
        [Header("Team Synergy")]
        public List<string> synergyPartners = new List<string>();
        public float synergyBonus = 1.2f;
        
        [Header("Personality")]
        [TextArea(3, 5)]
        public string personalityDescription;
        public VoiceType voiceType = VoiceType.Normal;
        public List<string> catchPhrases = new List<string>();
        
        [Header("Animations")]
        public RuntimeAnimatorController animatorController;
        public AnimationClip idleAnimation;
        public AnimationClip walkAnimation;
        public AnimationClip runAnimation;
        public AnimationClip swingAnimation;
        public AnimationClip victoryAnimation;
        public AnimationClip defeatAnimation;
        
        [Header("Cat Theme")]
        public bool isCatThemed = false;
        public CatPersonality catPersonality = CatPersonality.Friendly;
        public GameObject catAccessory;
        
        [Header("VRM Settings")]
        public bool allowUserCustomization = true;
        public bool supportBlendShapes = true;
        public List<string> allowedBlendShapes = new List<string>();
        
        [Header("Unlock Conditions")]
        public bool isUnlocked = false;
        public int unlockLevel = 1;
        public int unlockCost = 0;
        public string unlockRequirement;
    }
    
    public enum CharacterType
    {
        Player,
        NPC,
        Partner,
        Opponent,
        Special
    }
    
    public enum CharacterRole
    {
        PowerHitter,    // パワー重視
        Technician,     // テクニック重視
        Speedster,      // スピード重視
        AllRounder,     // バランス型
        Support,        // サポート型
        Trickster       // トリッキー型
    }
    
    public enum VoiceType
    {
        Normal,
        Cheerful,
        Cool,
        Mysterious,
        Energetic,
        Gentle,
        Cat
    }
    
    public enum CatPersonality
    {
        Friendly,       // 人懐っこい
        Aloof,         // ツンデレ
        Playful,       // 遊び好き
        Lazy,          // のんびり
        Curious,       // 好奇心旺盛
        Mischievous    // いたずら好き
    }
    
    [System.Serializable]
    public class SpecialAbility
    {
        public string abilityName;
        public string description;
        public Sprite icon;
        public float cooldown = 30f;
        public float duration = 5f;
        public AbilityType type;
        
        public enum AbilityType
        {
            PowerBoost,
            AccuracyBoost,
            WindControl,
            WeatherImmunity,
            TeamBuff,
            OpponentDebuff,
            SpecialShot
        }
    }
}