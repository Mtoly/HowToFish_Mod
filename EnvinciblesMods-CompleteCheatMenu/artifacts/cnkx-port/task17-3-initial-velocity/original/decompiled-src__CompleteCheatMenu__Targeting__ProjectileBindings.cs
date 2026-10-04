using System.Reflection;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Targeting;

internal static class ProjectileBindings
{
	internal static MethodInfo AddProjectile => GameBinder.Method("ProjectileManager", "AddProjectile", 8);

	internal static MethodInfo AddProjectiles => GameBinder.Method("ProjectileManager", "AddProjectiles", 8);

	internal static MethodInfo UpdateProjectileScan => GameBinder.Method("ProjectileManager", "UpdateProjectileScan", 2);

	internal static MethodInfo AddToRemoveQueue => GameBinder.Method("ProjectileManager", "AddToRemoveQueue", 1);

	internal static FieldInfo PlayerProjectiles => GameBinder.Field("ProjectileManager", "_playerProjectiles");

	internal static FieldInfo CatchUpSpeed => GameBinder.Field("ProjectileManager", "_catchUpSpeed");

	internal static FieldInfo SqrMaxProjectileRange => GameBinder.Field("ProjectileManager", "_sqrMaxProjRange");

	internal static FieldInfo Id => GameBinder.Field("Projectile", "Id");

	internal static FieldInfo Owner => GameBinder.Field("Projectile", "Owner");

	internal static FieldInfo IsLocal => GameBinder.Field("Projectile", "IsLocal");

	internal static FieldInfo Position => GameBinder.Field("Projectile", "Position");

	internal static FieldInfo Velocity => GameBinder.Field("Projectile", "Velocity");

	internal static FieldInfo FromNpc => GameBinder.Field("Projectile", "FromNpc");

	internal static PropertyInfo ProjectileGravity => GameBinder.Property("WeaponInfo", "ProjectileGravity");

	internal static bool LifecycleAvailable => AddProjectile != null && AddProjectiles != null && AddToRemoveQueue != null && Id != null && Owner != null && IsLocal != null && FromNpc != null;

	internal static bool GuidanceAvailable => LifecycleAvailable && UpdateProjectileScan != null && Position != null && Velocity != null && ProjectileGravity != null;

	internal static void ResolveAll()
	{
		_ = AddProjectile;
		_ = AddProjectiles;
		_ = UpdateProjectileScan;
		_ = AddToRemoveQueue;
		_ = PlayerProjectiles;
		_ = CatchUpSpeed;
		_ = SqrMaxProjectileRange;
		_ = Id;
		_ = Owner;
		_ = IsLocal;
		_ = Position;
		_ = Velocity;
		_ = FromNpc;
		_ = ProjectileGravity;
	}
}
