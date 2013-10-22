using System.Collections.ObjectModel;
#if PRISM4
using Microsoft.Practices.Prism.Modularity;
#else
using Microsoft.Practices.Composite.Modularity;
#endif
using System;
using System.Windows.Resources;
using System.Windows;
using System.IO;
using System.Windows.Markup;
using System.Collections.Generic;

namespace PTPDevelopmentLibrary.Permissions
{
    /// <summary>
    /// The object that should be used to describe the Modules that are available to the current user.
    /// </summary>
    /// <example>
    ///     <code>
    /// <![CDATA[
    ///     <Infrastructure:ModulePermissionsCatalog xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///                                              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///                                              xmlns:sys="clr-namespace:System;assembly=mscorlib"
    ///                                              xmlns:Infrastructure="clr-namespace:Dashboards.Infrastructure.ModulePermissions;assembly=Dashboards.Infrastructure"
    ///                                              xmlns:Modularity="clr-namespace:Microsoft.Practices.Composite.Modularity;assembly=Microsoft.Practices.Composite">
    ///         <Infrastructure:ModulePermissionsCatalog.Catalog>
    ///             <Modularity:ModuleCatalog>
    ///                 <Modularity:ModuleInfoGroup Ref="ModuleAXap.xap" InitializationMode="OnDemand">
    ///                     <Modularity:ModuleInfo ModuleName="Module A"
    ///                                            ModuleType="ModuleAProject.ModuleA, ModuleAXap, Version=1.0.0.0"/>
    ///                 </Modularity:ModuleInfoGroup>
    ///             </Modularity:ModuleCatalog>
    ///         </Infrastructure:ModulePermissionsCatalog.Catalog>
    ///         <Infrastructure:ModulePermission ModuleName="Module A" Clause="RoleA AND RoleB NOT RoleC"/>
    ///     </Infrastructure:ModulePermissionsCatalog>
    /// ]]>
    ///     </code>
    /// In this example, the user will be able to access Module A if and only if he is within RoleA and RoleB but not if he/she is in
    /// RoleC.
    /// </example>
    [ContentProperty("Permissions")]
    public class ModulePermissionsCatalog : IModulePermissionsCatalog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ModulePermissionsCatalog() 
        {
            Permissions = new ObservableCollection<ModulePermission>();
        }
        /// <summary>
        /// The container object representing the permissions.
        /// </summary>
        public ObservableCollection<ModulePermission> Permissions { get; set; }
#if PRISM4
        /// <summary>
        /// The IModuleCatalog object containing the <see cref="Microsoft.Practices.Prism.Modularity.ModuleInfo"/> object.
        /// </summary>
#else
        /// <summary>
        /// The IModuleCatalog object containing the <see cref="Microsoft.Practices.Composite.Modularity.ModuleInfo"/> object.
        /// </summary>
#endif
        public IModuleCatalog Catalog { get; set; }
        /// <summary>
        /// Creates a <see cref="ModulePermissionsCatalog"/> object from a <see cref="Uri"/> object point to a loose xaml file.
        /// </summary>
        /// <param name="builderResourceUri">A <see cref="Uri"/> object containing the path to the Xaml resource.</param>
        /// <returns>A <see cref="ModulePermissionsCatalog"/> object.</returns>
        public static ModulePermissionsCatalog LoadFromXaml(Uri builderResourceUri)
        {
            ModulePermissionsCatalog permissionsCatalog = null;
            StreamResourceInfo resourceInfo = Application.GetResourceStream(builderResourceUri);

            if (resourceInfo != null && resourceInfo.Stream != null)
            {
                StreamReader reader = new StreamReader(resourceInfo.Stream);
#if SILVERLIGHT
                permissionsCatalog = (ModulePermissionsCatalog)XamlReader.Load(reader.ReadToEnd());
#else
                permissionsCatalog = (ModulePermissionsCatalog)XamlReader.Load(reader.BaseStream);
#endif
            }

            return permissionsCatalog;
        }

        #region IModulePermissionsCatalog Members

        /// <summary>
        /// Use this method to retrieve a list of module names that are available to a particular set of roles.
        /// </summary>
        /// <param name="userRoles">A <see cref="List{String}"/> object containing a set of roles.</param>
        /// <returns>A <see cref="List{String}"/> object containing a set of Module names that are available to the given set of roles.</returns>
        public List<string> GetValidModules(List<string> userRoles)
        {
            List<string> result = new List<string>();

            foreach (ModulePermission mp in Permissions)
            {
                if (mp.Clause.Evaluate(userRoles))
                    result.Add(mp.ModuleName);
            }

            return result;
        }

        #endregion
    }
}