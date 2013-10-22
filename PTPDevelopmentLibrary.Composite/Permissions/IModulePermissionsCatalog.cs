using System.Collections.Generic;

namespace PTPDevelopmentLibrary.Permissions
{
    /// <summary>
    /// The contract used to identify the needed functionality from a Permissions Catalog
    /// </summary>
    public interface IModulePermissionsCatalog
    {
        /// <summary>
        /// The sole method needed for a permissions catalog.  Given a list of roles, 
        /// the method should return a list of Module names that are available to the 
        /// set of roles.
        /// </summary>
        /// <param name="userRoles">A <see cref="System.Collections.Generic.List{String}"/> object describing the roles pertaining to the current user.</param>
        /// <returns>A <see cref="System.Collections.Generic.List{String}"/> object containing names of the modules that are available to the current user.</returns>
        List<string> GetValidModules(List<string> userRoles);
    }
}