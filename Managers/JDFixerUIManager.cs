﻿using JDFixer.Interfaces;
using System;
using System.Collections.Generic;
using Zenject;


namespace JDFixer.Managers
{
    class JDFixerUIManager : IInitializable, IDisposable
    {
        public static StandardLevelDetailViewController levelDetail;
        public static MissionSelectionMapViewController missionSelection;
        private readonly List<IBeatmapInfoUpdater> beatmapInfoUpdaters;


        [Inject]
        public JDFixerUIManager(StandardLevelDetailViewController standardLevelDetailViewController, MissionSelectionMapViewController missionSelectionMapViewController, List<IBeatmapInfoUpdater> iBeatmapInfoUpdaters)
        {
            //Logger.log.Debug("JDFixerUIManager()");

            levelDetail = standardLevelDetailViewController;
            missionSelection = missionSelectionMapViewController;
            beatmapInfoUpdaters = iBeatmapInfoUpdaters;
        }


        public void Initialize()
        {
            //Logger.log.Debug("Initialize()");

            levelDetail.didChangeDifficultyBeatmapEvent += LevelDetail_didChangeDifficultyBeatmapEvent;
            levelDetail.didChangeContentEvent += LevelDetail_didChangeContentEvent;

            missionSelection.didSelectMissionLevelEvent += MissionSelection_didSelectMissionLevelEvent;
        }


        public void Dispose()
        {
            //Logger.log.Debug("Dispose()");

            levelDetail.didChangeDifficultyBeatmapEvent -= LevelDetail_didChangeDifficultyBeatmapEvent;
            levelDetail.didChangeContentEvent -= LevelDetail_didChangeContentEvent;

            missionSelection.didSelectMissionLevelEvent -= MissionSelection_didSelectMissionLevelEvent;
        }


        private void LevelDetail_didChangeDifficultyBeatmapEvent(StandardLevelDetailViewController arg1, IDifficultyBeatmap arg2)
        {
            //Logger.log.Debug("LevelDetail_didChangeDifficultyBeatmapEvent()");

            if (arg1 != null && arg2 != null)
            {
                DiffcultyBeatmapUpdated(arg2);
            }
        }


        private void LevelDetail_didChangeContentEvent(StandardLevelDetailViewController arg1, StandardLevelDetailViewController.ContentType arg2)
        {
            //Logger.log.Debug("LevelDetail_didChangeContentEvent()");          
            
            if (arg1 != null && arg1.selectedDifficultyBeatmap != null)
            {
                //Logger.log.Debug("NJS: " + arg1.selectedDifficultyBeatmap.noteJumpMovementSpeed);
                //Logger.log.Debug("Offset: " + arg1.selectedDifficultyBeatmap.noteJumpStartBeatOffset);

                DiffcultyBeatmapUpdated(arg1.selectedDifficultyBeatmap);
            }
        }


        private void MissionSelection_didSelectMissionLevelEvent(MissionSelectionMapViewController arg1, MissionNode arg2)
        {
            if (Plugin.cc_installed && arg2.missionData != null)
            {
                Logger.log.Debug("In CC, MissionNode exists");

                /*
                Logger.log.Debug("MissionNode - missionid: " + arg2.missionId); //"<color=#0a92ea>[STND]</color> Holdin' Oneb28Easy-1"
                Logger.log.Debug("MissionNode - difficulty: " + arg2.missionData.beatmapDifficulty); // "Easy" etc
                Logger.log.Debug("MissionNode - characteristic: " + arg2.missionData.beatmapCharacteristic.serializedName); //"Standard" etc

                Logger.log.Debug("MissionNode - missionhelp: " + arg2.missionData.missionHelp); // (CustomCampaigns.Campaign.Missions.CustomMissionHelpSO)
                */


                // These are useless:
                /*
                Logger.log.Debug("MissionNode - name: " + arg2.name); // "MissionNode has name MissionNode_1(Clone)" for everything
                Logger.log.Debug("MissionNode - letter: " + arg2.letterPartName); // Empty
                Logger.log.Debug("MissionNode - number: " + arg2.numberPartName); // -1 for everything
                Logger.log.Debug("MissionNode - formatted name: " + arg2.formattedMissionNodeName); // Empty
                Logger.log.Debug("MissionNode - tag: " + arg2.tag); // "Untagged"

                Logger.log.Debug("MissionNode - name: " + arg2.missionData.name); // Empty
                Logger.log.Debug("MissionNode - objectives: " + arg2.missionData.missionObjectives[0].ToString()); // Seems always Empty
                Logger.log.Debug("MissionNode - duration: " + arg2.missionData.level.songDuration); // Crashes everytime, Seems there is no level in CC
                */



                /*
                [DEBUG @ 21:40:52 | JDFixer] MissionNode exists
                [DEBUG @ 21:40:52 | JDFixer] MissionNode - missionid: < color =#c27ef2> Wildcard1d3bcHard-1
                [DEBUG @ 21:40:52 | JDFixer] MissionNode - difficulty: Hard
                [DEBUG @ 21:40:52 | JDFixer] MissionNode - characteristic: Standard
                [DEBUG @ 21:40:52 | JDFixer] MissionNode - missionhelp: (CustomCampaigns.Campaign.Missions.CustomMissionHelpSO)
                [DEBUG @ 21:40:52 | JDFixer] CC Level: custom_level_35B125930B0F475431AFCFF0362711D98CFEEAA6
                */

                if (MissionSelectionPatch.cc_level != null)
                {
                    Logger.log.Debug("CC Level: " + MissionSelectionPatch.cc_level.levelID);

                    IDifficultyBeatmap difficulty_beatmap = CustomCampaigns.Utils.BeatmapUtils.GetMatchingBeatmapDifficulty(MissionSelectionPatch.cc_level.levelID, arg2.missionData.beatmapCharacteristic, arg2.missionData.beatmapDifficulty);

                    Logger.log.Debug("MissionNode Diff: " + difficulty_beatmap.difficulty);
                    Logger.log.Debug("MissionNode Offset: " + difficulty_beatmap.noteJumpStartBeatOffset);
                    Logger.log.Debug("MissionNode NJS: " + difficulty_beatmap.noteJumpMovementSpeed);

                    DiffcultyBeatmapUpdated(difficulty_beatmap);
                }
            }

            // QOL: When in Base Campaigns, set Map JD and Reaction Time displays to show zeroes to prevent misleading player
            else
            {
                DiffcultyBeatmapUpdated(null);
            }
        }


        private void DiffcultyBeatmapUpdated(IDifficultyBeatmap difficultyBeatmap)
        {
            //Logger.log.Debug("DiffcultyBeatmapUpdated()");

            foreach (var beatmapInfoUpdater in beatmapInfoUpdaters)
            {
                beatmapInfoUpdater.BeatmapInfoUpdated(new BeatmapInfo(difficultyBeatmap));
            }
        }
    }
}
