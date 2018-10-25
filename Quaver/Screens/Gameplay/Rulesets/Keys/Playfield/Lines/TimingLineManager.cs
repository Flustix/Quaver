using System.Collections.Generic;
using Quaver.API.Maps;

namespace Quaver.Screens.Gameplay.Rulesets.Keys.Playfield.Lines
{
    public class TimingLineManager
    {
        /// <summary>
        ///     Timing Line object pool.
        /// </summary>
        private Queue<TimingLine> Pool { get; set; }

        /// <summary>
        ///     Timing Line information. Generated by this class with qua object.
        /// </summary>
        private Queue<TimingLineInfo> Info { get; set; }

        /// <summary>
        ///     Reference to the ruleset this HitObject manager is for.
        /// </summary>
        public GameplayRulesetKeys Ruleset { get; }

        /// <summary>
        ///     Initial size for the object pool
        /// </summary>
        private int InitialPoolSize { get; } = 6;

        /// <summary>
        ///     The position at which the next TimingLine must be at in order to add a new TimingLine object to the pool.
        /// </summary>
        private float CreateObjectPosition { get; set; } = 1500;

        /// <summary>
        ///     The position at which the earliest TimingLine object must be at before its recycled.
        /// </summary>
        private float RecycleObjectPosition { get; set; } = -1500;

        /// <summary>
        ///     Convert from BPM to measure length in milliseconds. (4 beats)
        /// </summary>
        public float BpmToMeasureLengthMs { get; } = 4 * 60 * 1000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="ruleset"></param>
        public TimingLineManager(GameplayRuleset ruleset)
        {
            // Set Reference variables
            Ruleset = (GameplayRulesetKeys)ruleset;

            // Set Time Line Y offset from skin
            // todo: offset the timing lines so that they are snapped to the center of the hit body of any skin
            /*
            var reference = SkinManager.Skin.Keys[Ruleset.Mode].NoteHoldHitObjects[0][0];
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            var offset = playfield.LaneSize * reference.Height / reference.Width / 2;
            TimingLineObject.GlobalYOffset = GameplayRulesetKeys.IsDownscroll ? offset + 3 : offset + 1;
            */

            GenerateTimingLineInfo(ruleset.Map);
            InitializeObjectPool();
        }

        /// <summary>
        ///     Generate Timing Line Information for the map
        /// </summary>
        /// <param name="map"></param>
        private void GenerateTimingLineInfo(Qua map)
        {
            Info = new Queue<TimingLineInfo>();
            var index = 0;

            // set initial increment that will update songPos by 4 beat lengths
            var songPos = map.TimingPoints[index].StartTime;
            var increment = BpmToMeasureLengthMs / map.TimingPoints[index].Bpm;

            // Create first Timing Line Info
            var offset = Ruleset.Screen.TrackManager.GetPositionFromTime(songPos);
            var info = new TimingLineInfo(songPos, offset);
            Info.Enqueue(info);

            // Generate Timing Lines
            while (songPos < map.Length)
            {
                // Update songpos with increment
                songPos += increment;

                // If songPos exceeds the next timing point (if next timing point exists):
                // - Reset songPos to the next timing point and update increment
                // - subtract Timing Point StartTime by 1 to add more tolerance when finding next Timing Point
                if (index + 1 < map.TimingPoints.Count && songPos >= map.TimingPoints[index + 1].StartTime - 1)
                {
                    index++;
                    songPos = map.TimingPoints[index].StartTime;
                    increment = BpmToMeasureLengthMs / map.TimingPoints[index].Bpm;
                }

                // Create Timing Line Info
                offset = Ruleset.Screen.TrackManager.GetPositionFromTime(songPos);
                info = new TimingLineInfo(songPos, offset);
                Info.Enqueue(info);
            }
        }

        /// <summary>
        ///     Initialize the Timing Line Object Pool
        /// </summary>
        private void InitializeObjectPool()
        {
            // Initialize pool
            Pool = new Queue<TimingLine>();

            // Create pool objects equal to the initial pool size or total objects that will be displayed on screen initially
            for (var i = 0; i < Info.Count && (i < InitialPoolSize || Info.Peek().TrackOffset - Ruleset.Screen.TrackManager.Position < CreateObjectPosition); i++)
                CreatePoolObject(Info.Dequeue());
        }

        /// <summary>
        ///     Update every object in the Timing Line Object Pool and create new objects if necessary
        /// </summary>
        public void UpdateObjectPool()
        {
            // Update line positions
            foreach (var line in Pool)
                line.UpdateSpritePosition(Ruleset.Screen.TrackManager.Position);

            // Recycle necessary pool objects
            while (Pool.Count > 0 && Pool.Peek().TrackOffset <= RecycleObjectPosition)
            {
                var line = Pool.Dequeue();
                if (Info.Count > 0)
                {
                    line.Info = Info.Dequeue();
                    line.UpdateSpritePosition(Ruleset.Screen.TrackManager.Position);
                    Pool.Enqueue(line);
                }
            }

            // Create new pool objects if they are in range
            while (Info.Count > 0 && Info.Peek().TrackOffset - Ruleset.Screen.TrackManager.Position < CreateObjectPosition)
                CreatePoolObject(Info.Dequeue());
        }

        /// <summary>
        ///     Create and add new Timing Line Object to the Object Pool
        /// </summary>
        /// <param name="info"></param>
        private void CreatePoolObject(TimingLineInfo info)
        {
            var line = new TimingLine(Ruleset, info);
            line.UpdateSpritePosition(Ruleset.Screen.TrackManager.Position);
            Pool.Enqueue(line);
        }
    }
}
