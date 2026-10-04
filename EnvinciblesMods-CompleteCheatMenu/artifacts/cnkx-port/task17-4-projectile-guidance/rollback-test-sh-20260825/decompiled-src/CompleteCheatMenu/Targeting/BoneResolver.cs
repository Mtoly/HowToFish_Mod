using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class BoneResolver
{
	private static readonly string[] HeadTokens = { "head", "skull", "neck" };
	private static readonly string[] ChestTokens = { "chest", "spine", "body", "torso" };
	private static readonly string[] PelvisTokens = { "pelvis", "hips", "hip" };

	internal static List<AimPoint> Resolve(Transform root, bool player, float fixedHeight)
	{
		List<AimPoint> points = new List<AimPoint>(8);
		if (root == null)
		{
			return points;
		}
		HashSet<int> seen = new HashSet<int>();
		if (player)
		{
			ResolveHumanoid(root, points, seen);
		}
		else
		{
			ResolveNamedHierarchy(root, points, seen);
			ResolveSkinnedBones(root, points, seen);
		}
		if (RendererBoundsResolver.TryResolve(root, out var bounds))
		{
			AddPosition(points, RendererBoundsResolver.UpperPoint(bounds), "bounds-upper", "renderer-bounds");
			AddPosition(points, bounds.center, "body-center", "renderer-bounds");
		}
		AddPosition(points, root.position + root.up * fixedHeight, "fixed-height", "root-offset");
		return points;
	}

	private static void ResolveHumanoid(Transform root, List<AimPoint> points, HashSet<int> seen)
	{
		Animator animator = root.GetComponentInChildren<Animator>(includeInactive: true);
		if (animator == null)
		{
			return;
		}
		AddTransform(points, seen, animator.GetBoneTransform(HumanBodyBones.Head), "head", "animator");
		AddTransform(points, seen, animator.GetBoneTransform(HumanBodyBones.Chest), "chest", "animator");
		AddTransform(points, seen, animator.GetBoneTransform(HumanBodyBones.Hips), "pelvis", "animator");
	}

	private static void ResolveNamedHierarchy(Transform root, List<AimPoint> points, HashSet<int> seen)
	{
		Transform head = null;
		Transform chest = null;
		Transform pelvis = null;
		Stack<Transform> pending = new Stack<Transform>();
		pending.Push(root);
		while (pending.Count > 0)
		{
			Transform current = pending.Pop();
			ClassifyFirst(current, ref head, ref chest, ref pelvis);
			for (int i = current.childCount - 1; i >= 0; i--)
			{
				pending.Push(current.GetChild(i));
			}
		}
		AddTransform(points, seen, head, "head", "named-bone");
		AddTransform(points, seen, chest, "chest", "named-bone");
		AddTransform(points, seen, pelvis, "pelvis", "named-bone");
	}

	private static void ResolveSkinnedBones(Transform root, List<AimPoint> points, HashSet<int> seen)
	{
		Transform head = null;
		Transform chest = null;
		Transform pelvis = null;
		SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		for (int i = 0; i < renderers.Length; i++)
		{
			Transform[] bones = renderers[i].bones;
			for (int j = 0; j < bones.Length; j++)
			{
				ClassifyFirst(bones[j], ref head, ref chest, ref pelvis);
			}
		}
		AddTransform(points, seen, head, "head", "skinned-bone");
		AddTransform(points, seen, chest, "chest", "skinned-bone");
		AddTransform(points, seen, pelvis, "pelvis", "skinned-bone");
	}

	private static void ClassifyFirst(Transform candidate, ref Transform head, ref Transform chest, ref Transform pelvis)
	{
		if (candidate == null)
		{
			return;
		}
		string value = candidate.name ?? string.Empty;
		if (head == null && ContainsAny(value, HeadTokens))
		{
			head = candidate;
		}
		else if (chest == null && ContainsAny(value, ChestTokens))
		{
			chest = candidate;
		}
		else if (pelvis == null && ContainsAny(value, PelvisTokens))
		{
			pelvis = candidate;
		}
	}

	private static bool ContainsAny(string value, string[] tokens)
	{
		for (int i = 0; i < tokens.Length; i++)
		{
			if (value.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	private static void AddTransform(List<AimPoint> points, HashSet<int> seen, Transform transform, string bone, string source)
	{
		if (transform != null && seen.Add(transform.GetInstanceID()))
		{
			points.Add(new AimPoint(transform, transform.position, bone, source));
		}
	}

	private static void AddPosition(List<AimPoint> points, Vector3 position, string bone, string source)
	{
		for (int i = 0; i < points.Count; i++)
		{
			if ((points[i].Position - position).sqrMagnitude <= 0.000001f)
			{
				return;
			}
		}
		points.Add(new AimPoint(null, position, bone, source));
	}
}
