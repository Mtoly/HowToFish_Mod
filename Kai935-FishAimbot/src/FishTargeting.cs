using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishAimbotMod
{
    public static class FishTargeting
    {
        private sealed class TargetEntry
        {
            internal readonly Creature Creature;
            internal readonly Transform Transform;
            internal string Name;
            internal bool IsAlbatross;
            internal bool IsSeagull;

            internal TargetEntry(Creature creature)
            {
                Creature = creature;
                Transform = creature.transform;
                UpdateName(creature.name);
            }

            internal void UpdateName(string name)
            {
                Name = name;
                IsAlbatross = name.IndexOf("albatross", StringComparison.OrdinalIgnoreCase) >= 0;
                IsSeagull = name.IndexOf("seagull", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private static readonly List<TargetEntry> Targets = new List<TargetEntry>(128);

        internal static void Initialize()
        {
            Targets.Clear();
            Creature[] existing = UnityEngine.Object.FindObjectsByType<Creature>();
            for (int i = 0; i < existing.Length; i++)
            {
                Register(existing[i]);
            }
        }

        internal static void Register(Creature creature)
        {
            if (creature == null)
            {
                return;
            }

            for (int i = 0; i < Targets.Count; i++)
            {
                if (ReferenceEquals(Targets[i].Creature, creature))
                {
                    return;
                }
            }

            Targets.Add(new TargetEntry(creature));
        }

        public static Transform GetBestTarget(Vector3 camPos, Vector3 lookDir, float maxFov)
        {
            Transform bestTarget = null;
            float bestScore = float.MaxValue;
            float halfFov = maxFov * 0.5f;

            for (int i = 0; i < Targets.Count; i++)
            {
                TargetEntry entry = Targets[i];
                Creature creature = entry.Creature;

                if (creature == null)
                {
                    Targets.RemoveAt(i);
                    i--;
                    continue;
                }

                string currentName = creature.name;
                if (!ReferenceEquals(entry.Name, currentName) && entry.Name != currentName)
                {
                    entry.UpdateName(currentName);
                }

                if (!creature.gameObject.activeInHierarchy || creature.IsDead || entry.IsSeagull)
                {
                    continue;
                }

                Fish fish = creature as Fish;
                if (fish != null && fish.AttachedRod != null)
                {
                    continue;
                }

                Vector3 delta = entry.Transform.position - camPos;
                float distance = delta.magnitude;
                Vector3 direction = distance > 0.00001f ? delta / distance : Vector3.zero;
                float angle = Vector3.Angle(lookDir, direction);
                if (angle > halfFov)
                {
                    continue;
                }

                float score = entry.IsAlbatross ? angle * 0.2f : distance + angle * 0.5f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = entry.Transform;
                }
            }

            return bestTarget;
        }
    }
}
