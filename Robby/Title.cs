﻿namespace RobinHood70.Robby
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using Design;
	using RobinHood70.WallE.Base;
	using RobinHood70.WallE.Design;
	using WikiCommon;
	using static Properties.Resources;
	using static WikiCommon.Globals;

	public enum ProtectionLevel
	{
		NoChange,
		None,
		Semi,
		Full,
	}

	/// <summary>Provides a light-weight holder for titles and provides several information and manipulation functions.</summary>
	public class Title : IWikiTitle, IMessageSource
	{
		#region Constants
		// The following is taken from DefaultSettings::$wgLegalTitleChars and always assumes the default setting. I believe this is emitted as part of API:Siteinfo, but I wouldn't trust any kind of automated conversion, so better to just leave it as default, which is what 99.99% of wikis will probably use.
		private const string TitleChars = @"[ %!\""$&'()*,\-.\/0-9:;=?@A-Z\\^_`a-z~+\P{IsBasicLatin}-[()（）]]";
		#endregion

		#region Fields
		private static Regex labelCommaRemover = new Regex(@"\ *([,，]" + TitleChars + @"*?)\Z", RegexOptions.Compiled);
		private static Regex labelParenthesesRemover = new Regex(@"\ *(\(" + TitleChars + @"*?\)|（" + TitleChars + @"*?）)\Z", RegexOptions.Compiled);
		private static Regex bidiText = new Regex(@"[\u200E\u200F\u202A\u202B\u202C\u202D\u202E]", RegexOptions.Compiled); // Taken from MediaWikIWikiTitleCodec->splitTitleString, then converted to Unicode
		private static Regex spaceText = new Regex(@"[ _\xA0\u1680\u180E\u2000-\u200A\u2028\u2029\u202F\u205F\u3000]", RegexOptions.Compiled); // as above, but already Unicode in MW code
		#endregion

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="Title"/> class using the site and full page name.</summary>
		/// <param name="site">The site to which this title belongs.</param>
		/// <param name="fullName">The full name of the page.</param>
		public Title(Site site, string fullName)
			: this(site, fullName, null)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="Title"/> class using the site and full page name.</summary>
		/// <param name="site">The site to which this title belongs.</param>
		/// <param name="fullName">The page name, including the namespace.</param>
		/// <param name="key">The key to use when indexing this page.</param>
		/// <remarks>Absolutely no cleanup or checking is performed when using this version of the constructor. All values are assumed to already have been validated.</remarks>
		public Title(Site site, string fullName, string key)
		{
			ThrowNull(site, nameof(site));
			ThrowNull(fullName, nameof(fullName));
			this.Site = site;
			this.SetNames(fullName);
			this.Key = key ?? this.FullPageName;
		}

		/// <summary>Initializes a new instance of the <see cref="Title"/> class using the site and full page name.</summary>
		/// <param name="site">The site to which this title belongs.</param>
		/// <param name="ns">The namespace to which the page belongs.</param>
		/// <param name="pageName">The name (only) of the page.</param>
		/// <remarks>Absolutely no cleanup or checking is performed when using this version of the constructor. All values are assumed to already have been validated.</remarks>
		public Title(Site site, int ns, string pageName)
		{
			ThrowNull(site, nameof(site));
			ThrowNull(pageName, nameof(pageName));
			this.Site = site;
			this.Namespace = site.Namespaces[ns];
			this.PageName = this.Namespace.CaseSensitive ? Normalize(pageName) : Normalize(pageName).UpperFirst();
			this.Key = this.FullPageName;
		}

		/// <summary>Initializes a new instance of the <see cref="Title"/> class, copying the information from another Title object.</summary>
		/// <param name="title">The Title object to copy from.</param>
		public Title(IWikiTitle title)
		{
			ThrowNull(title, nameof(title));
			this.Site = title.Site;
			this.Namespace = title.Namespace;
			this.PageName = title.PageName;
			this.Key = title.Key;
		}
		#endregion

		#region Public Properties

		/// <summary>Gets the value corresponding to {{BASEPAGENAME}}.</summary>
		public string BasePageName
		{
			get
			{
				if (this.Namespace.AllowsSubpages)
				{
					var subpageLoc = this.PageName.LastIndexOf('/');
					if (subpageLoc >= 0)
					{
						return this.PageName.Substring(0, subpageLoc);
					}
				}

				return this.PageName;
			}
		}

		/// <summary>Gets the value corresponding to {{FULLPAGENAME}}.</summary>
		public string FullPageName => this.Namespace.DecoratedName + this.PageName;

		/// <summary>Gets the original, unaltered key to use for dictionaries and the like.</summary>
		public virtual string Key { get; }

		/// <summary>Gets a name similar to the one that would appear when using the pipe trick on the page (e.g., "Harry Potter (character)" will produce "Harry Potter").</summary>
		public string LabelName => PipeTrick(this.PageName);

		/// <summary>Gets the namespace object for the title.</summary>
		public Namespace Namespace { get; private set; }

		/// <summary>Gets the value corresponding to {{PAGENAME}}.</summary>
		public string PageName { get; private set; }

		/// <summary>Gets the site the title is intended for.</summary>
		public Site Site { get; }

		/// <summary>Gets a Title object for this Title's corresponding subject page. If this Title is a subject page, returns itself.</summary>
		public Title SubjectPage => this.Namespace.Id == this.Namespace.SubjectSpaceId ? this : new Title(this.Site, this.Namespace.SubjectSpaceId, this.PageName);

		/// <summary>Gets the value corresponding to {{SUBPAGENAME}}.</summary>
		public string SubpageName
		{
			get
			{
				if (this.Namespace.AllowsSubpages)
				{
					var subpageLoc = this.PageName.LastIndexOf('/');
					if (subpageLoc >= 0)
					{
						return this.PageName.Substring(subpageLoc);
					}
				}

				return this.PageName;
			}
		}

		/// <summary>Gets a Title object for this Title's corresponding subject page. If this Title is a talk page, returns itself. Returns null for pages which have no associated talk page.</summary>
		public Title TalkPage =>
			this.Namespace.TalkSpaceId == null ? null
			: this.Namespace.Id == this.Namespace.TalkSpaceId.Value ? this
			: new Title(this.Site, this.Namespace.TalkSpaceId.Value, this.PageName);
		#endregion

		#region Public Static Methods

		/// <summary>Identical to the constructor with the same signature, but allows that the page name may have the namespace prepended to it and compensates accordingly.</summary>
		/// <param name="site">The site to which this title belongs.</param>
		/// <param name="ns">The namespace to which the page belongs.</param>
		/// <param name="pageName">The name of the page, with or without the corresponding namespace prefix.</param>
		/// <returns>A Title object with the given name in the given namespace.</returns>
		public static Title ForcedNamespace(Site site, int ns, string pageName)
		{
			var retval = new Title(site, pageName);
			if (retval.Namespace.Id != ns)
			{
				retval = new Title(site, ns, pageName);
			}

			return retval;
		}

		/// <summary>Builds the full name of the page from the namespace and page name, accounting for Main space.</summary>
		/// <param name="ns">The namespace of the page.</param>
		/// <param name="pageName">The page name.</param>
		/// <param name="fragment">The fragment (section title/anchor) to include.</param>
		/// <returns>The full name of the page from the namespace and page name, accounting for Main space.</returns>
		public static string NameFromParts(Namespace ns, string pageName, string fragment) => ns?.DecoratedName + pageName + (fragment == null ? string.Empty : "#" + fragment);

		/// <summary>Removes bidirectional text markers and replaces space-like characters with spaces.</summary>
		/// <param name="text">The text to normalize.</param>
		/// <returns>The original text with bidirectional text markers removed and space-like characters converted to spaces.</returns>
		public static string Normalize(string text) => spaceText.Replace(bidiText.Replace(text, string.Empty), " ").Trim();

		/// <summary>Gets a name similar to the one that would appear when using the pipe trick on the page (e.g., "Harry Potter (character)" will produce "Harry Potter").</summary>
		/// <param name="pageName">The name of the page, without namespace or fragment text.</param>
		/// <remarks>This doesn't precisely match the pipe trick logic - they differ in their handling of some abnormal page names. For example, with page names of "User:(Test)", ":(Test)", and "(Test)", the pipe trick gives "User:", ":", and "(Test)", respectively. Since this routine ignores the namespace completely and checks for empty return values, it returns "(Test)" consistently in all three cases.</remarks>
		/// <returns>The text with the final paranthetical and/or comma-delimited text removed. Note: like the MediaWiki equivalent, when both are present, this will remove text of the form "(text), text", but text of the form ", text (text)" will become ", text". The text should already have been cleaned using Fixup().</returns>
		public static string PipeTrick(string pageName)
		{
			pageName = labelCommaRemover.Replace(pageName, string.Empty, 1, 1);
			pageName = labelParenthesesRemover.Replace(pageName, string.Empty, 1, 1);
			return pageName;
		}
		#endregion

		#region Public Methods

		public bool CreateProtect(string reason, ProtectionLevel protectionLevel, DateTime expiry)
		{
			if (protectionLevel != ProtectionLevel.NoChange)
			{
				var protection = new ProtectInputItem("create", ProtectionWord(protectionLevel)) { Expiry = expiry };
				return this.Protect(reason, new[] { protection });
			}

			return false;
		}

		public bool CreateUnprotect(string reason)
		{
			var protection = new ProtectInputItem("create", ProtectionWord(ProtectionLevel.None));
			return this.Protect(reason, new[] { protection });
		}

		public bool Delete(string reason)
		{
			ThrowNull(reason, nameof(reason));
			if (!this.Site.AllowEditing)
			{
				return true;
			}

			var input = new DeleteInput(this.FullPageName)
			{
				Reason = reason
			};

			var result = this.Site.AbstractionLayer.Delete(input);
			return result.LogId > 0;
		}

		/// <summary>Functionally equivalent to Equals(Title), but IEquatable breaks spectacularly on non-sealed types, so is not implemented.</summary>
		/// <param name="title">The title, or derived type, to compare to.</param>
		/// <returns>True if all of Site, Namespace, PageName, and Key are identical between the two Titles.</returns>
		public bool IsIdenticalTo(IWikiTitle title) => this.IsSameAs(title) && this.Key == title?.Key;

		/// <summary>Checks to see if this Title resolves to the same page as another Title. Key is ignored.</summary>
		/// <param name="title">The title, or derived type, to compare to.</param>
		/// <returns>True if all of Site, Namespace, and PageName are identical between the two Titles.</returns>
		public bool IsSameAs(IWikiTitle title) =>
			title != null &&
			this.Site == title.Site &&
			this.Namespace == title.Namespace &&
			this.PageName == title.PageName;

		public Dictionary<string, string> Move(string to, string reason, bool suppressRedirect) => this.Move(to, reason, false, false, suppressRedirect);

		public Dictionary<string, string> Move(Title to, string reason, bool suppressRedirect) => this.Move(to.FullPageName, reason, false, false, suppressRedirect);

		public Dictionary<string, string> Move(string to, string reason, bool moveTalk, bool moveSubpages, bool suppressRedirect)
		{
			ThrowNull(to, nameof(to));
			ThrowNull(reason, nameof(reason));
			if (!this.Site.AllowEditing)
			{
				return new Dictionary<string, string> { [this.FullPageName] = to };
			}

			var input = new MoveInput(this.FullPageName, to)
			{
				IgnoreWarnings = true,
				MoveSubpages = moveSubpages,
				MoveTalk = moveTalk,
				NoRedirect = suppressRedirect,
				Reason = reason
			};

			var retval = new Dictionary<string, string>();
			MoveResult result;
			try
			{
				result = this.Site.AbstractionLayer.Move(input);
			}
			catch (WikiException e)
			{
				this.Site.PublishWarning(this, e.Info);
				return null;
			}

			foreach (var item in result)
			{
				if (item.Error != null)
				{
					this.Site.PublishWarning(this, CurrentCulture(MovePageWarning, this.FullPageName, to, item.Error.Info));
				}
				else
				{
					retval.Add(item.From, item.To);
				}
			}

			this.Rename(to);
			return retval;
		}

		public Dictionary<string, string> Move(Title to, string reason, bool moveTalk, bool moveSubpages, bool suppressRedirect) => this.Move(to.FullPageName, reason, moveTalk, moveSubpages, suppressRedirect);

		public bool Protect(string reason, ProtectionLevel createProtection, string relativeExpiry)
		{
			if (createProtection != ProtectionLevel.NoChange)
			{
				var protection = new ProtectInputItem("create", ProtectionWord(createProtection)) { ExpiryRelative = relativeExpiry };
				return this.Protect(reason, new[] { protection });
			}

			return false;
		}

		public bool Protect(string reason, ProtectionLevel editProtection, ProtectionLevel moveProtection, DateTime expiry)
		{
			var protections = new List<ProtectInputItem>(2);
			if (editProtection != ProtectionLevel.NoChange)
			{
				protections.Add(new ProtectInputItem("edit", ProtectionWord(editProtection)) { Expiry = expiry });
			}

			if (moveProtection != ProtectionLevel.NoChange)
			{
				protections.Add(new ProtectInputItem("move", ProtectionWord(moveProtection)) { Expiry = expiry });
			}

			return this.Protect(reason, protections);
		}

		public bool Protect(string reason, ProtectionLevel editProtection, ProtectionLevel moveProtection, string relativeExpiry)
		{
			if (relativeExpiry == null)
			{
				relativeExpiry = "indefinite";
			}

			var protections = new List<ProtectInputItem>(2);
			if (editProtection != ProtectionLevel.NoChange)
			{
				protections.Add(new ProtectInputItem("edit", ProtectionWord(editProtection)) { ExpiryRelative = relativeExpiry });
			}

			if (moveProtection != ProtectionLevel.NoChange)
			{
				protections.Add(new ProtectInputItem("move", ProtectionWord(moveProtection)) { ExpiryRelative = relativeExpiry });
			}

			return this.Protect(reason, protections);
		}

		/// <summary>Renames the title.</summary>
		/// <param name="fullName">The full page name to rename to.</param>
		public void Rename(string fullName)
		{
			ThrowNull(fullName, nameof(fullName));
			this.SetNames(fullName);
		}

		public bool Unprotect(string reason, bool editUnprotect, bool moveUnprotect)
		{
			var protections = new List<ProtectInputItem>(3);
			if (editUnprotect)
			{
				protections.Add(new ProtectInputItem("edit", ProtectionWord(ProtectionLevel.None)));
			}

			if (moveUnprotect)
			{
				protections.Add(new ProtectInputItem("move", ProtectionWord(ProtectionLevel.None)));
			}

			return this.Protect(reason, protections);
		}
		#endregion

		#region Public Override Methods

		/// <summary>Returns a string that represents the current Title.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString() => this.FullPageName;
		#endregion

		#region Private Static Methods
		private static string ProtectionWord(ProtectionLevel level)
		{
			switch (level)
			{
				case ProtectionLevel.None:
					return "all";
				case ProtectionLevel.Semi:
					return "autoconfirmed";
				case ProtectionLevel.Full:
					return "sysop";
			}

			return null;
		}
		#endregion

		#region Private Methods
		private bool Protect(string reason, ICollection<ProtectInputItem> protections)
		{
			if (protections.Count == 0)
			{
				return false;
			}

			if (!this.Site.AllowEditing)
			{
				return true;
			}

			var input = new ProtectInput(this.FullPageName)
			{
				Protections = protections,
				Reason = reason
			};
			var result = this.Site.AbstractionLayer.Protect(input);

			return result.Protections.Count == protections.Count;
		}

		private void SetNames(string fullName)
		{
			var split = fullName.Split(new[] { ':' }, 2);
			string pageName;
			if (split.Length == 2 && this.Site.Namespaces.TryGetValue(split[0], out var ns))
			{
				pageName = split[1].TrimStart();
				this.Namespace = ns;
			}
			else
			{
				pageName = fullName;
				this.Namespace = this.Site.Namespaces[MediaWikiNamespaces.Main];
			}

			split = pageName.Split(new char[] { '#' }, 2);
			this.PageName = this.Namespace.CaseSensitive ? Normalize(pageName) : Normalize(pageName).UpperFirst();
		}
		#endregion
	}
}