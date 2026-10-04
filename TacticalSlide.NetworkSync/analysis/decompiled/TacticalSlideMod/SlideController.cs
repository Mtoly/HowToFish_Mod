using BepInEx;
using UnityEngine;

namespace TacticalSlideMod;

public class SlideController : MonoBehaviour
{
	private float _slideTimer;

	private float _cooldownTimer;

	private float _slideBufferTimer;

	private float _refFovVel;

	private float _refPitchVel;

	private float _refHeightVel;

	private float _scrapeAudioTimer;

	public static SlideController Instance { get; private set; }

	public static bool IsSliding { get; private set; } = false;

	public static float SlideProgress { get; private set; } = 0f;

	public static Vector3 CurrentSlideDir { get; private set; } = Vector3.forward;

	public static float CurrentFovOffset { get; private set; } = 0f;

	public static float CurrentCameraPitchOffset { get; private set; } = 0f;

	public static float CurrentCameraRollOffset { get; private set; } = 0f;

	public static float CurrentCameraHeightOffset { get; private set; } = 0f;

	public static float CurrentLegForwardOffset { get; private set; } = 0f;

	public static float CurrentBodyLeanAngle { get; private set; } = 0f;

	public static void Initialize()
	{
		if (Instance == null)
		{
			GameObject obj = new GameObject("TacticalSlide_Controller");
			Object.DontDestroyOnLoad(obj);
			Instance = obj.AddComponent<SlideController>();
		}
	}

	private void Update()
	{
		if (!TacticalSlidePlugin.ModEnabled.Value)
		{
			if (IsSliding)
			{
				CancelSlide();
			}
			return;
		}
		if (_cooldownTimer > 0f)
		{
			_cooldownTimer -= Time.deltaTime;
		}
		if (UnityInput.Current.GetKeyDown(TacticalSlidePlugin.SlideKey.Value) || UnityInput.Current.GetKeyDown(TacticalSlidePlugin.AltSlideKey.Value))
		{
			_slideBufferTimer = 0.25f;
		}
		else if (_slideBufferTimer > 0f)
		{
			_slideBufferTimer -= Time.deltaTime;
		}
		UpdateVisualInterpolation();
	}

	public void CheckAndConsumeSlideInput(PlayerMovement movement)
	{
		if (!(movement == null) && movement.gameObject.activeInHierarchy)
		{
			bool flag = movement.Sprinting || (movement.Input.y > 0.1f && UnityInput.Current.GetKey(KeyCode.LeftShift));
			if (!IsSliding && _slideBufferTimer > 0f && movement.Grounded && _cooldownTimer <= 0f && flag)
			{
				_slideBufferTimer = 0f;
				StartSlide(movement);
			}
		}
	}

	private void StartSlide(PlayerMovement movement)
	{
		IsSliding = true;
		_slideTimer = TacticalSlidePlugin.SlideDuration.Value;
		SlideProgress = 0f;
		Rigidbody component = movement.GetComponent<Rigidbody>();
		Vector3 vector = ((component != null) ? new Vector3(component.linearVelocity.x, 0f, component.linearVelocity.z) : Vector3.zero);
		Vector3 vector2;
		if (vector.sqrMagnitude > 1.5f)
		{
			vector2 = vector.normalized;
		}
		else
		{
			Vector3 vector3 = Vector3.forward;
			if (Player.LocalPlayer != null && Player.LocalPlayer.CamObject != null)
			{
				vector3 = Vector3.ProjectOnPlane(Player.LocalPlayer.CamObject.forward, Vector3.up).normalized;
			}
			else if (movement.transform != null)
			{
				vector3 = Vector3.ProjectOnPlane(movement.transform.forward, Vector3.up).normalized;
			}
			Vector2 input = movement.Input;
			vector2 = ((!(input.sqrMagnitude > 0.1f)) ? vector3 : (Quaternion.LookRotation(vector3, Vector3.up) * new Vector3(input.x, 0f, input.y)).normalized);
		}
		vector2.y = 0f;
		CurrentSlideDir = vector2.normalized;
		try
		{
			AudioManager.PlayGlobalClip("PunchSwoosh", variation: true, 0.65f, 0.02f);
			string stepSound = SurfaceManager.GetStepSound(movement.CurGroundTransform, movement.CurFeetWorldPos);
			if (!string.IsNullOrEmpty(stepSound))
			{
				AudioManager.PlayRandomGlobalClip(stepSound, 1, GameInfo.StepSoundCount, variation: true, 0.75f, 0.02f);
			}
			ParticleManager.Play("Smoke", movement.CurFeetWorldPos, Vector3.up);
		}
		catch
		{
		}
	}

	public void ApplySlidePhysics(PlayerMovement movement, Rigidbody rig, ref Vector3 curVel)
	{
		if (!IsSliding || movement == null || rig == null)
		{
			return;
		}
		_slideTimer -= Time.fixedDeltaTime;
		float num = Mathf.Max(0.1f, TacticalSlidePlugin.SlideDuration.Value);
		SlideProgress = Mathf.Clamp01(1f - _slideTimer / num);
		if (_slideTimer <= 0f)
		{
			EndSlide(movement);
			return;
		}
		_scrapeAudioTimer -= Time.fixedDeltaTime;
		if (_scrapeAudioTimer <= 0f && movement.Grounded)
		{
			_scrapeAudioTimer = 0.16f;
			try
			{
				string stepSound = SurfaceManager.GetStepSound(movement.CurGroundTransform, movement.CurFeetWorldPos);
				if (!string.IsNullOrEmpty(stepSound))
				{
					AudioManager.PlayRandomGlobalClip(stepSound, 1, GameInfo.StepSoundCount, variation: true, 0.5f, 0.05f);
				}
			}
			catch
			{
			}
		}
		float num2 = Mathf.Lerp(TacticalSlidePlugin.SlideBoostMulti.Value, 0.45f, Mathf.Pow(SlideProgress, 1.6f));
		if (TacticalSlidePlugin.EnableSlopeBoost.Value && movement.Grounded)
		{
			float slipAngle = movement.SlipAngle;
			if (slipAngle > 5f)
			{
				num2 += Mathf.Clamp(slipAngle / 25f, 0f, 0.9f);
			}
		}
		float num3 = 9.5f * num2;
		Vector3 vector = CurrentSlideDir;
		if (movement.OnBoat && (bool)BoatManager.Boat && (bool)BoatManager.Boat.VisualBoat)
		{
			vector = BoatManager.Boat.VisualBoat.TransformDirection(BoatManager.Boat.VisualBoat.InverseTransformDirection(CurrentSlideDir));
		}
		Vector3 vector2 = vector * num3;
		vector2.y = rig.linearVelocity.y;
		rig.linearVelocity = vector2;
		curVel = vector2;
	}

	public void OnSlideHop(PlayerMovement movement, Rigidbody rig)
	{
		if (IsSliding && !(movement == null) && !(rig == null))
		{
			float value = TacticalSlidePlugin.SlideHopBoost.Value;
			Vector3 linearVelocity = new Vector3(rig.linearVelocity.x, 0f, rig.linearVelocity.z) * value;
			linearVelocity.y = rig.linearVelocity.y;
			rig.linearVelocity = linearVelocity;
			IsSliding = false;
			_cooldownTimer = 0.25f;
		}
	}

	public void EndSlide(PlayerMovement movement)
	{
		IsSliding = false;
		_cooldownTimer = 0.25f;
	}

	public void CancelSlide()
	{
		IsSliding = false;
		_slideTimer = 0f;
		SlideProgress = 0f;
		_slideBufferTimer = 0f;
		_cooldownTimer = 0.15f;
	}

	private void UpdateVisualInterpolation()
	{
		float target = 0f;
		float target2 = 0f;
		float target3 = 0f;
		float b = 0f;
		float b2 = 0f;
		if (IsSliding)
		{
			if (TacticalSlidePlugin.EnableFovWarp.Value)
			{
				target = Mathf.Lerp(12f, 2f, SlideProgress);
			}
			if (TacticalSlidePlugin.EnableCameraTilt.Value)
			{
				target2 = -3.5f;
				target3 = -0.28f;
				b = -16f;
				b2 = 0.45f;
			}
		}
		CurrentFovOffset = Mathf.SmoothDamp(CurrentFovOffset, target, ref _refFovVel, 0.07f);
		CurrentCameraPitchOffset = Mathf.SmoothDamp(CurrentCameraPitchOffset, target2, ref _refPitchVel, 0.07f);
		CurrentCameraHeightOffset = Mathf.SmoothDamp(CurrentCameraHeightOffset, target3, ref _refHeightVel, 0.07f);
		CurrentBodyLeanAngle = Mathf.Lerp(CurrentBodyLeanAngle, b, Time.deltaTime * 18f);
		CurrentLegForwardOffset = Mathf.Lerp(CurrentLegForwardOffset, b2, Time.deltaTime * 18f);
	}
}
