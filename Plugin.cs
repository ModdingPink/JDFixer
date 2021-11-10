﻿using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
//using TournamentAssistant;

namespace JDFixer
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static Harmony harmony;
        public static StandardLevelDetailViewController leveldetail;
        public static MissionSelectionMapViewController missionselection;
        //private static IPA.Loader.PluginMetadata hasTA;

        [Init]
        public void Init(IPA.Logging.Logger logger, Config conf)
        {
            Logger.log = logger;
            PluginConfig.Instance = conf.Generated<PluginConfig>();
        }


        [OnStart]
        public void OnApplicationStart()
        {
            //Config.Read();

            harmony = new Harmony("com.zephyr.BeatSaber.JDFixer");
            harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh += BSEvents_lateMenuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.difficultySelected += BSEvents_difficultySelected;
            //UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

            //BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.instance.AddTab("JDFixerOnline", "JDFixer.UI.BSML.modifierOnlineUI.bsml", UI.ModifierUI.instance, BeatSaberMarkupLanguage.GameplaySetup.MenuType.Online);
        }

        // For when user selects a map with only 1 difficulty or selects a map but does not click a difficulty
        private void BSEvents_lateMenuSceneLoadedFresh(ScenesTransitionSetupDataSO obj)
        {
            //Logger.log.Debug(obj.scenes[0].sceneName);


            // Note: Is there a need to check if these are null? Are they ever null?
            //var leveldetail = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
            if (leveldetail != null)
            {
                leveldetail.didChangeContentEvent += Leveldetail_didChangeContentEvent;
            }

            // When in Campaigns, set Map JD and Reaction Time displays to show zeroes
            //var missionselection = Resources.FindObjectsOfTypeAll<MissionSelectionMapViewController>().FirstOrDefault();
            if (missionselection != null)
            {
                missionselection.didSelectMissionLevelEvent += Missionselection_didSelectMissionLevelEvent;
            }
        }

        // QOL: When in Campaigns, set Map JD and Reaction Time displays to show zeroes to prevent misleading player
        private void Missionselection_didSelectMissionLevelEvent(MissionSelectionMapViewController arg1, MissionNode arg2)
        {
            BeatmapInfo.SetSelected(null);
        }


        // For when user selects a map with only 1 difficulty or selects a map but does not click a difficulty
        private void Leveldetail_didChangeContentEvent(StandardLevelDetailViewController arg1, StandardLevelDetailViewController.ContentType arg2)
        {
            if (arg1 != null && arg1.selectedDifficultyBeatmap != null)
            {
                BeatmapInfo.SetSelected(arg1.selectedDifficultyBeatmap);
            }

            // Note: This works to display 0s in Campaigns
            // but also happens after every map in Solo until a diff is clicked again
            /*else if (arg1 == null)
            {
                BeatmapInfo.SetSelected(null);
            }*/
        }

        // For when user explicitly clicks on a difficulty (Doesn't work if map has only 1 diff)
        private void BSEvents_difficultySelected(StandardLevelDetailViewController arg1, IDifficultyBeatmap level)
        {
            BeatmapInfo.SetSelected(level);
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            //Config.Write();

            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh -= BSEvents_lateMenuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.difficultySelected -= BSEvents_difficultySelected;
            //UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

            leveldetail.didChangeContentEvent -= Leveldetail_didChangeContentEvent;
            missionselection.didSelectMissionLevelEvent -= Missionselection_didSelectMissionLevelEvent;

            harmony.UnpatchAll("com.zephyr.BeatSaber.JDFixer");
        }
    }
}
