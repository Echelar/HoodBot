﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RobinHood70.Robby.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RobinHood70.Robby.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You attempted to use an object from one site with an object from another..
        /// </summary>
        internal static string InvalidSite {
            get {
                return ResourceManager.GetString("InvalidSite", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The attempt to log in to the wiki failed: {0}.
        /// </summary>
        internal static string LoginFailed {
            get {
                return ResourceManager.GetString("LoginFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must load {0} when calling the {1} method..
        /// </summary>
        internal static string ModuleNotLoaded {
            get {
                return ResourceManager.GetString("ModuleNotLoaded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Moving page [[{0}]] to [[{1}]] failed: {{2}}.
        /// </summary>
        internal static string MovePageWarning {
            get {
                return ResourceManager.GetString("MovePageWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid page name: [[{0}]]. Page is an interwiki link that doesn&apos;t resolve to the local wiki..
        /// </summary>
        internal static string PageNameInterwiki {
            get {
                return ResourceManager.GetString("PageNameInterwiki", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is invalid until the site has been initialized..
        /// </summary>
        internal static string SiteNotInitialized {
            get {
                return ResourceManager.GetString("SiteNotInitialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Size value invalid..
        /// </summary>
        internal static string SizeInvalid {
            get {
                return ResourceManager.GetString("SizeInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Source collection is empty - TitleCollection could not be initialized..
        /// </summary>
        internal static string SourceCollectionEmpty {
            get {
                return ResourceManager.GetString("SourceCollectionEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The title cannot be in Talk namespace and followed immediately by another namespace or interwiki prefix..
        /// </summary>
        internal static string TitleDoubleNamespace {
            get {
                return ResourceManager.GetString("TitleDoubleNamespace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The title name cannot be blank..
        /// </summary>
        internal static string TitleInvalid {
            get {
                return ResourceManager.GetString("TitleInvalid", resourceCulture);
            }
        }
    }
}
