namespace PTPDevelopmentLibrary.Permissions
{
#if PRISM4
    /// <summary>
    /// Represents the permissions needed for a single <see cref="Microsoft.Practices.Prism.Modularity.ModuleInfo"/> object.
    /// </summary>
#else
    /// <summary>
    /// Represents the permissions needed for a single <see cref="Microsoft.Practices.Composite.Modularity.ModuleInfo"/> object.
    /// </summary>
#endif
    public class ModulePermission
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ModulePermission() 
        { }
#if PRISM4
        /// <summary>
        /// The boolean clause used to determine the <see cref="Microsoft.Practices.Prism.Modularity.ModuleInfo"/> availability.
        /// </summary>
#else
        /// <summary>
        /// The boolean clause used to determine the <see cref="Microsoft.Practices.Composite.Modularity.ModuleInfo"/> availability.
        /// </summary>
#endif
        public PermissionClause Clause { get; set; }
        /// <summary>
        /// The name of the target Module.
        /// </summary>
        public string ModuleName { get; set; }
    }
}