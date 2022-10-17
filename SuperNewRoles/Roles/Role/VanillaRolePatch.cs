


namespace SuperNewRoles.Roles
{
    public static class VanillaRolePatch
    {
        public static bool IsVanillaRole(this RoleId role) =>
            role is
            RoleId.Crewmate or
            RoleId.Engineer or
            RoleId.Impostor or
            RoleId.Shapeshifter or
            RoleId.GuardianAngel;
        public static bool IsVanillaRole(this PlayerControl player) => player.GetRole().IsVanillaRole();
    }
}