using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using UnityEngine;
using BepInEx;

namespace ShotgunKinSwapper {

    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, ModName, VERSION)]
    public class ShotgunKinSwapper : BaseUnityPlugin {

        public const string GUID = "ApacheThunder.etg.ShotgunKinSwapper";
        public const string ModName = "ShotgunKinSwapper";
        public const string VERSION = "1.1.0";

        public static tk2dSpriteCollectionData shotgunkinskinCollection;

        public static Hook GameManagerHook;
        
        
        public void Start() { ETGModMainBehaviour.WaitForGameManagerStart(GMStart); }

        public void GMStart(GameManager gameManager) {
            ETGMod.Assets.SetupSpritesFromAssembly(Assembly.GetExecutingAssembly(), ModName + "/Sprites");
            DoFoyerChecks();
        }
        
        private void GameManager_Awake(Action<GameManager> orig, GameManager self) {
            orig(self);
            DoFoyerChecks();
        }
        
        public void DoFoyerChecks() {
            GameObject FoyerCheckController = new GameObject("ShotgunMod Foyer Checker", new Type[] { typeof(ShotGunEnableController) });
        }
    
    }

    public class ShotGunEnableController : BraveBehaviour {

        public ShotGunEnableController() { m_HasTriggerd = false; }

        private bool m_HasTriggerd;

        public void Awake() { }

        public void Start() { }

        public void Update() {
            if (m_HasTriggerd) { return; }
            if (Foyer.DoIntroSequence && Foyer.DoMainMenu) { return; }
            if (ShotgunKinSwapper.GameManagerHook == null) {
                ShotgunKinSwapper.GameManagerHook = new Hook(
                    typeof(GameManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance),
                    typeof(ShotgunKinSwapper).GetMethod("GameManager_Awake", BindingFlags.NonPublic | BindingFlags.Instance),
                    typeof(GameManager)
                );
            }
            CharacterCostumeSwapper[] m_Characters = FindObjectsOfType<CharacterCostumeSwapper>();
            if (m_Characters != null && m_Characters.Length > 0) {
                CharacterCostumeSwapper BulletManSelector = null;
                foreach (CharacterCostumeSwapper m_Character in m_Characters) {
                    if (m_Character?.TargetLibrary?.name == "Playable_Shotgun_Man_Swap_Animation") {
                        ShotgunKinSwapper.shotgunkinskinCollection = m_Character.TargetLibrary.clips[0].frames[0].spriteCollection;
                        BulletManSelector = m_Character; break;
                    }
                }
                if (BulletManSelector) {
                    bool Allow = (GameStatsManager.Instance.GetFlag(GungeonFlags.SECRET_BULLETMAN_SEEN_05) && GameStatsManager.Instance.GetCharacterSpecificFlag(BulletManSelector.TargetCharacter, CharacterSpecificGungeonFlags.KILLED_PAST));
                    if (Allow) {
                        FieldInfo m_active = typeof(CharacterCostumeSwapper).GetField("m_active", BindingFlags.Instance | BindingFlags.NonPublic);
                        m_active.SetValue(BulletManSelector, true);
                        if (ShotgunKinSwapper.shotgunkinskinCollection) {
                            Tools.ApplyCustomTexture(ShotgunKinSwapper.shotgunkinskinCollection, Tools.GetTextureFromResource("Textures\\Playable_Shotgun_Man_Swap.png"));
                        }
                        BulletManSelector.AlternateCostumeSprite.renderer.enabled = true;
                        BulletManSelector.CostumeSprite.renderer.enabled = false;
                    }
                    m_HasTriggerd = true;
                    Destroy(gameObject);
                }
            }
        }

        protected override void OnDestroy() { base.OnDestroy(); }
    }
}

