namespace SuperNewRoles.Roles.API{
    public abstract class CustomRoleOptionBase{
        protected int OptId;

        protected abstract CustomRoleOptionBase CreateRoleOption();
        protected abstract void CreateOtherOption(CustomRoleOptionBase parentOpt);

        protected abstract void Setup();
    }
}