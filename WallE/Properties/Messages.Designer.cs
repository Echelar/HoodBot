﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RobinHood70.WallE.Properties {
    using System;
    
    
    /// <summary>A strongly-typed resource class, for looking up localized strings, etc.</summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>Returns the cached ResourceManager instance used by this class.</summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RobinHood70.WallE.Properties.Messages", typeof(Messages).Assembly);
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
        
        /// <summary>Looks up a localized string similar to {0}: {1}.</summary>
        internal static string ColonText {
            get {
                return ResourceManager.GetString("ColonText", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to {0}, {1}.</summary>
        internal static string CommaText {
            get {
                return ResourceManager.GetString("CommaText", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to The requested item ({0}) was found, but is not of the expected module type ({1} instead of {2})..</summary>
        internal static string IncorrectModuleType {
            get {
                return ResourceManager.GetString("IncorrectModuleType", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to You cannot search for an empty string..</summary>
        internal static string InvalidSearchString {
            get {
                return ResourceManager.GetString("InvalidSearchString", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to You appear to have the mono CreateZStream bug. Please see http://stackoverflow.com/a/32958861/502255 to fix the issue before using this library. Alternatively, you can set the static FormClient.DefaultAcceptEncoding to null or an empty string before creating a FormClient instance..</summary>
        internal static string MonoCreateZStreamBug {
            get {
                return ResourceManager.GetString("MonoCreateZStreamBug", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to There was an attempt to add a piped parameter value, but an existing value with the same name ({0}) exists and is not a piped parameter..</summary>
        internal static string NotAPipedParameter {
            get {
                return ResourceManager.GetString("NotAPipedParameter", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to This property is settable only for Interface compatibility; it is never intended to be set..</summary>
        internal static string NotSettable {
            get {
                return ResourceManager.GetString("NotSettable", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to {0} / {1}.</summary>
        internal static string PerText {
            get {
                return ResourceManager.GetString("PerText", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to A stop was requested because {0}..</summary>
        internal static string StopRequested {
            get {
                return ResourceManager.GetString("StopRequested", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to You cannot set the Watchlist parameter for uploads to &quot;Unwatch&quot;..</summary>
        internal static string UploadUnwatchInvalid {
            get {
                return ResourceManager.GetString("UploadUnwatchInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The site returned an error.
        ///Code: {0}
        ///Info: {1}.
        /// </summary>
        internal static string WikiExceptionGeneral {
            get {
                return ResourceManager.GetString("WikiExceptionGeneral", resourceCulture);
            }
        }
        
        /// <summary>Looks up a localized string similar to (Warning) {0}: {1}.</summary>
        internal static string WikiWarning {
            get {
                return ResourceManager.GetString("WikiWarning", resourceCulture);
            }
        }
    }
}
