namespace TacticalSlide.NetworkSync
{
    public static class SlideAuthorityPolicy
    {
        // Missing/uninitialized owners are not permission to drive another body.
        public static bool CanDrive(bool movementExists, bool ownerExists, bool localOwner)
        {
            return movementExists && ownerExists && localOwner;
        }
    }
}
