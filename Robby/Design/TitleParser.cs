﻿namespace RobinHood70.Robby.Design
{
	using RobinHood70.CommonCode;
	using RobinHood70.WikiCommon;
	using static RobinHood70.CommonCode.Globals;

	/// <summary>This class serves as a light-weight parser to split a wiki title into its constituent parts.</summary>
	public class TitleParser : ILinkTitle
	{
		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="TitleParser"/> class.</summary>
		/// <param name="site">The site.</param>
		/// <param name="fullPageName">Full page name, with namespace.</param>
		public TitleParser(Site site, string fullPageName)
			: this(site, MediaWikiNamespaces.Main, fullPageName, false)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="TitleParser"/> class.</summary>
		/// <param name="site">The site.</param>
		/// <param name="defaultNamespace">The default namespace.</param>
		/// <param name="pageName">Name of the page.</param>
		/// <param name="forceNamespace">If <see langword="true"/>, the namespace specified will always be used, even if the pageName begins with what looks like a namespace or interwiki prefix.</param>
		public TitleParser(Site site, int defaultNamespace, string pageName, bool forceNamespace)
		{
			ThrowNull(site, nameof(site));
			ThrowNull(pageName, nameof(pageName));

			int? nsFinal = null;
			string? originalNs = null;

			// Pipes are not allowed in page names, so if we find one, only parse the first part; the remainder is likely cruft from a category or file link.
			var nameRemaining = pageName.Split(TextArrays.Pipe, 2)[0];

			// Ignore forceNamespace if defaultNamespace is Main, since you cannot have, for example, a Mainspace page titled :Talk:Whatever (or it's an error if you do).
			if (forceNamespace && defaultNamespace != MediaWikiNamespaces.Main)
			{
				nsFinal = defaultNamespace;
				originalNs = site.Namespaces[defaultNamespace].Name;
			}
			else
			{
				// Empty Title can be valid when passed from a null template or link, so even if nameRemaining is or becomes empty, we keep going.
				if (nameRemaining.Length > 0 && nameRemaining[0] == ':')
				{
					this.LeadingColon = true;
					nameRemaining = nameRemaining.Substring(1);
				}

				var split = nameRemaining.Split(TextArrays.Colon, 3);
				if (split.Length >= 2)
				{
					var key = WikiTextUtilities.DecodeAndNormalize(split[0]).Trim();
					if (site.Namespaces.ValueOrDefault(key) is Namespace ns)
					{
						nsFinal = ns.Id;
						originalNs = split[0];
						nameRemaining = split[1] + (split.Length == 3 ? ':' + split[2] : string.Empty);
					}
					else if (site.InterwikiMap != null && site.InterwikiMap.TryGetValue(key, out var iw))
					{
						this.Interwiki = iw;
						this.OriginalInterwikiText = split[0];
						key = WikiTextUtilities.DecodeAndNormalize(split[1]).Trim();
						if (iw.LocalWiki && site.Namespaces.ValueOrDefault(key) is Namespace nsiw)
						{
							nsFinal = nsiw.Id;
							originalNs = split[1];
							nameRemaining = split[2];
							if (nameRemaining.Length == 0)
							{
								nameRemaining = site.MainPageName ?? "Main Page";
							}
						}
						else
						{
							nameRemaining = split[1] + (split.Length == 3 ? ':' + split[2] : string.Empty);
						}
					}
				}
			}

			// If we have a leading colon, but no namespace, then this was meant to override any default namespace and force it to Main space.
			if (nsFinal == null)
			{
				if (this.LeadingColon)
				{
					nsFinal = MediaWikiNamespaces.Main;
				}
				else
				{
					nsFinal = defaultNamespace;
					this.Coerced = true;
				}
			}

			this.Namespace = site.Namespaces[nsFinal.Value];
			this.OriginalNamespaceText = originalNs ?? string.Empty;

			if (nameRemaining.Length == 0)
			{
				this.OriginalPageNameText = string.Empty;
				pageName = string.Empty;
			}
			else
			{
				var split = nameRemaining.Split(TextArrays.Octothorp, 2);
				if (split.Length == 2)
				{
					this.OriginalPageNameText = split[0];
					pageName = WikiTextUtilities.DecodeAndNormalize(split[0]).Trim();
					this.OriginalFragmentText = split[1];
					this.Fragment = WikiTextUtilities.DecodeAndNormalize(split[1]).TrimEnd();
				}
				else
				{
					this.OriginalPageNameText = nameRemaining;
					pageName = WikiTextUtilities.DecodeAndNormalize(nameRemaining).Trim();
				}
			}

			this.PageName = this.Namespace.CapitalizePageName(pageName);
		}
		#endregion

		#region Public Properties

		/// <summary>Gets a value indicating whether the title was coerced into its namespace, or started there to begin with.</summary>
		/// <value><c>true</c> if coerced into the indicated namespace; otherwise, <c>false</c>.</value>
		public bool Coerced { get; }

		/// <summary>Gets the title's fragment (the section or ID to scroll to).</summary>
		/// <value>The fragment.</value>
		public string? Fragment { get; }

		/// <inheritdoc/>
		public string FullPageName => this.Namespace.DecoratedName + this.PageName;

		/// <summary>Gets the interwiki prefix.</summary>
		/// <value>The interwiki prefix.</value>
		public InterwikiEntry? Interwiki { get; }

		/// <summary>Gets a value indicating whether the title had a leading colon.</summary>
		/// <value><see langword="true"/> if there was a leading colon; otherwise, <see langword="false"/>.</value>
		public bool LeadingColon { get; }

		/// <inheritdoc/>
		public Namespace Namespace { get; }

		/// <summary>Gets the fragment text passed to the constructor, after parsing.</summary>
		/// <value>The fragment text.</value>
		/// <remarks>This value can be used to bypass any automatic formatting or name changes caused by using the default Interwiki values, such as case changes. Parsing removes hidden characters and changes unusual spaces to normal spaces.</remarks>
		public string? OriginalFragmentText { get; }

		/// <summary>Gets the interwiki text passed to the constructor, after parsing.</summary>
		/// <value>The interwiki text.</value>
		/// <remarks>This value can be used to bypass any automatic formatting or name changes caused by using the default Interwiki values, such as case changes. Parsing removes hidden characters and changes unusual spaces to normal spaces.</remarks>
		public string? OriginalInterwikiText { get; }

		/// <summary>Gets the namespace text passed to the constructor, after parsing.</summary>
		/// <value>The namespace text.</value>
		/// <remarks>This value can be used to bypass any text changes caused by relying on the Namespace values, such as an alias having been used. Parsing removes hidden characters and changes unusual spaces to normal spaces.</remarks>
		public string OriginalNamespaceText { get; }

		/// <summary>Gets the page name text passed to the constructor, after parsing.</summary>
		/// <value>The page name text.</value>
		/// <remarks>This value can be used to bypass any text changes caused by using the PageName value, such as first-letter casing. Parsing removes hidden characters and changes unusual spaces to normal spaces.</remarks>
		public string OriginalPageNameText { get; }

		/// <inheritdoc/>
		public string PageName { get; }
		#endregion
	}
}