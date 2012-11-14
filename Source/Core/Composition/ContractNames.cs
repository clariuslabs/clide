namespace Clide.Composition
{
    /// <summary>
    /// Exposes the contract names for exports of general purpose services which 
    /// are not VS-specific types.
    /// </summary>
    public static class ContractNames
    {
        private const string Prefix = "Dynamics.";

        /// <summary>
        /// Contract name for accessing ICompositionService.
        /// </summary>
        public const string ICompositionService = Prefix + "ICompositionService";

        /// <summary>
        /// Contract name for accessing the global ExportProvider..
        /// </summary>
        public const string ExportProvider = Prefix + "ExportProvider";
    }
}
