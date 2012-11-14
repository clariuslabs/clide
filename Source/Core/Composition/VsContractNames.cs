namespace Clide.Composition
{
    /// <summary>
    /// Exposes the contract names for exports of built-in Visual Studio 
    /// services.
    /// </summary>
    public static class VsContractNames
    {
        private const string Prefix = "Dynamics.VisualStudio.";

        /// <summary>
        /// Contract name for accessing IComponentModel.
        /// </summary>
        public const string IComponentModel = Prefix + "IComponentModel";

        /// <summary>
        /// Contract name for importing <see cref="IVsUIShell"/>.
        /// </summary>
        public const string IVsUIShell = Prefix + "IVsUIShell";

        /// <summary>
        /// Contract name for importing <see cref="IVsShell"/>.
        /// </summary>
        public const string IVsShell = Prefix + "IVsShell";

        /// <summary>
        /// Contract name for importing <see cref="EnvDTE.DTE"/>.
        /// </summary>
        public const string DTE = Prefix + "DTE";
    }
}
