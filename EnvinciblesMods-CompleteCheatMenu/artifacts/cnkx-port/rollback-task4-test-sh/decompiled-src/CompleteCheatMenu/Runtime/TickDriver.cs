using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Runtime;

internal static class TickDriver
{
	private static bool _wasFlying;

	private static bool _wasBoatFlying;

	internal static void FixedTick()
	{
		if (Refs.InGame)
		{
			if (VisualCheats.FreeCamActive)
			{
				MovementCheats.FreezePlayer();
			}
			else if (CheatState.Fly && MovementCheats.FlyAvailable)
			{
				MovementCheats.FlyStep(CheatState.FlyHorizontalSpeed, CheatState.FlyVerticalSpeed);
				_wasFlying = true;
			}
			else if (_wasFlying)
			{
				MovementCheats.StopFly();
				_wasFlying = false;
			}
			if (CheatState.BoatFly && BoatCheats.FlyAvailable)
			{
				BoatCheats.FlyStep(CheatState.BoatFlySpeed);
				_wasBoatFlying = true;
			}
			else if (_wasBoatFlying)
			{
				BoatCheats.ToggleFly(on: false);
				_wasBoatFlying = false;
			}
			GamblingCheats.FixedStep();
		}
	}

	internal static void Tick()
	{
		if (VisualCheats.FreeCamActive)
		{
			VisualCheats.FreeCamStep(CheatState.FreeCamSpeed, CheatState.FreeCamSensitivity);
		}
		if (CheatState.EspEnabled && Refs.InGame)
		{
			VisualCheats.Rescan(CheatState.EspCreatures, CheatState.EspItems, CheatState.EspPlayers);
		}
	}
}
