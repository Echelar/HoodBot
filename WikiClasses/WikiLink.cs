﻿namespace RobinHood70.WikiClasses
{
	// CONSIDER: Reimplementing this as an IFullTitle in Robby; perhaps keep this as WikiTextLink to act as a non-wiki-specific class. Namespace handling would need to be considered carefully so that short namespace names aren't corrected to their full names. Possibly reimplement Template as well, although that's only really useful for template-like transclusions outside of actual Template space.
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Text.RegularExpressions;
	using RobinHood70.WikiCommon;
	using static RobinHood70.WikiCommon.Globals;

	/// <summary>Represents a wiki link.</summary>
	public class WikiLink
	{
		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="WikiLink"/> class.</summary>
		public WikiLink()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="WikiLink"/> class and attempts to parse the text provided.</summary>
		/// <param name="link">The link text to parse.</param>
		public WikiLink(string link)
		{
			ThrowNull(link, nameof(link));
			link = link.Trim();
			if (link.StartsWith("[[", StringComparison.Ordinal) && link.EndsWith("]]", StringComparison.Ordinal))
			{
				link = link.Substring(2, link.Length - 4);
				link = link.Trim();
			}

			var split = link.Split(TextArrays.Pipe, 2);
			var page = split[0];
			if (split.Length == 2)
			{
				this.DisplayText = split[1];
			}

			if (page[0] == ':')
			{
				this.ForceLink = true;
				page = page.Substring(1);
			}

			split = page.Split(TextArrays.Colon, 2);
			if (split.Length == 1)
			{
				this.Namespace = string.Empty;
			}
			else
			{
				this.Namespace = split[0];
			}

			var pageName = split[split.Length - 1].Trim();
			if (pageName.Length == 0)
			{
				this.PageName = pageName;
			}
			else
			{
				var fragmentSplit = pageName.Split(TextArrays.Octothorp, 2);
				this.PageName = fragmentSplit[0];
				if (fragmentSplit.Length == 2)
				{
					this.Fragment = fragmentSplit[1];
				}
			}
		}

		/// <summary>Initializes a new instance of the <see cref="WikiLink"/> class using the specified values.</summary>
		/// <param name="ns">The namespace.</param>
		/// <param name="pageName">The pagename.</param>
		public WikiLink(string ns, string pageName)
			: this(ns, pageName, pageName)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="WikiLink"/> class using the specified values.</summary>
		/// <param name="ns">The namespace.</param>
		/// <param name="pageName">The pagename.</param>
		/// <param name="displayText">The display text.</param>
		public WikiLink(string ns, string pageName, string displayText)
		{
			this.Namespace = ns;
			this.PageName = pageName;
			this.DisplayText = displayText;
		}

		/// <summary>Initializes a new instance of the <see cref="WikiLink"/> class and populates it from a <see cref="Match"/> found using one of the static <see cref="LinkFinder()"/> methods.</summary>
		/// <param name="linkFinderMatch">The Match generated by a <see cref="LinkFinder()"/> method.</param>
		public WikiLink(Match linkFinderMatch)
		{
			if (linkFinderMatch == null)
			{
				return;
			}

			this.ForceLink = linkFinderMatch.Groups["pre"].Value.Length > 0;
			this.Namespace = linkFinderMatch.Groups["namespace"].Value;
			this.PageName = linkFinderMatch.Groups["pagename"].Value;
			this.Fragment = linkFinderMatch.Groups["fragment"].Value;
			this.DisplayText = linkFinderMatch.Groups["displaytext"].Value;
		}
		#endregion

		#region Public Properties

		/// <summary>Gets or sets the display text (i.e., the value to the right of the pipe).</summary>
		public string DisplayText { get; set; }

		/// <summary>Gets or sets a value indicating whether to force a File or Category to be a link to the page.</summary>
		/// <value><c>true</c> if the wiki link should be forced to be a link; otherwise, <c>false</c>.</value>
		public bool ForceLink { get; set; }

		/// <summary>Gets the full name of the page.</summary>
		public string FullPageName => this.Namespace + ':' + this.PageName;

		/// <summary>Gets or sets the fragment for the link (i.e., the section/anchor).</summary>
		public string Fragment { get; set; }

		/// <summary>Gets or sets the namespace. This value is text only and is not validated.</summary>
		public string Namespace { get; set; }

		/// <summary>Gets or sets the name of the page without the namespace.</summary>
		public string PageName { get; set; }

		/// <summary>Gets the root page of a subpage.</summary>
		/// <remarks>Note that this property returns the pagename up to (but not including) the first slash. This is <em>not necessarily</em> the page directly above, as <c>{{BASEPAGENAME}}</c> would return. For example, <c>Template:Complicated/Subtemplate/Doc</c> would return only <c>Template:Complicated</c>.</remarks>
		public string RootPage => this.Namespace + ':' + this.PageName.Split(TextArrays.Slash, 2)[0];
		#endregion

		#region Public Static Methods

		/// <summary>Finds all links within the given text.</summary>
		/// <param name="text">The text to search.</param>
		/// <returns>An enumeration of all links within the text.</returns>
		/// <remarks>No location information is included, so this is most useful when you simply need to scan links rather than alter them.</remarks>
		public static IEnumerable<WikiLink> FindAllLinks(string text)
		{
			var matches = LinkFinder().Matches(text);
			foreach (Match match in matches)
			{
				yield return new WikiLink(match);
			}
		}

		/// <summary>Determines whether the specified value is a valid link.</summary>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c> if the specified value appears to be a link; otherwise, <c>false</c>.</returns>
		/// <remarks>This is a primitive check for surrounding brackets and may report incorrect values in complex situations.</remarks>
		public static bool IsLink(string value) =>
			value != null &&
			value.Length > 4 &&
			value[0] == '[' &&
			value[1] == '[' &&
			value[value.Length - 2] == ']' &&
			value[value.Length - 1] == ']' &&
			value.Substring(2, value.Length - 4).IndexOfAny(TextArrays.SquareBrackets) == -1;

		/// <summary>Creates a <see cref="Regex"/> to find all links.</summary>
		/// <returns>A <see cref="Regex"/> that finds all links.</returns>
		public static Regex LinkFinder() => LinkFinder(null, null, null, null, null);

		/// <summary>Creates a <see cref="Regex"/> to find all links matching the values provided.</summary>
		/// <param name="namespaces">The namespaces to search for. Use <c>null</c> to match all namespaces.</param>
		/// <param name="pageNames">The pagenames to search for. Use <c>null</c> to match all pagenames.</param>
		/// <param name="displayTexts">The display texts to search for. Use <c>null</c> to match all display texts.</param>
		/// <returns>A <see cref="Regex"/> that finds all links matching the values provided. Note that this will match, for example, any of the pagenames given in any of the namespaces given.</returns>
		public static Regex LinkFinder(IEnumerable<string> namespaces, IEnumerable<string> pageNames, IEnumerable<string> displayTexts) => LinkFinder(null, namespaces, pageNames, displayTexts, null);

		/// <summary>Creates a <see cref="Regex"/> to find all links matching the values provided which also have the specified surrounding text.</summary>
		/// <param name="regexBefore">A <see cref="Regex"/> fragment specifying the text to search for before the link. Use <c>null</c> to ignore the text before the link.</param>
		/// <param name="namespaces">The namespaces to search for. Use <c>null</c> to match all namespaces.</param>
		/// <param name="pageNames">The pagenames to search for. Use <c>null</c> to match all pagenames.</param>
		/// <param name="displayTexts">The display texts to search for. Use <c>null</c> to match all display texts.</param>
		/// <param name="regexAfter">A <see cref="Regex"/> fragment specifying the text to search for after the link. Use <c>null</c> to ignore the text after the link.</param>
		/// <returns>A <see cref="Regex"/> that finds all links matching the values provided. Note that this will match, for example, any of the pagenames given in any of the namespaces given.</returns>
		public static Regex LinkFinder(string regexBefore, IEnumerable<string> namespaces, IEnumerable<string> pageNames, IEnumerable<string> displayTexts, string regexAfter) =>
			LinkFinderRaw(
				regexBefore,
				EnumerableRegex(namespaces, true),
				EnumerableRegex(pageNames, true),
				EnumerableRegex(displayTexts, false),
				regexAfter);

		/// <summary>Creates a <see cref="Regex"/> to find all links matching the values provided which also have the specified surrounding text.</summary>
		/// <param name="regexBefore">A <see cref="Regex"/> fragment specifying the text to search for before the link. Use <c>null</c> to ignore the text before the link.</param>
		/// <param name="regexNamespaces">A <see cref="Regex"/> fragment specifying the namespaces to search for. Use <c>null</c> to match all namespaces.</param>
		/// <param name="regexPageNames">A <see cref="Regex"/> fragment specifying the pagenames to search for. Use <c>null</c> to match all pagenames.</param>
		/// <param name="regexDisplayTexts">A <see cref="Regex"/> fragment specifying the display texts to search for. Use <c>null</c> to match all display texts.</param>
		/// <param name="regexAfter">A <see cref="Regex"/> fragment specifying the text to search for after the link. Use <c>null</c> to ignore the text after the link.</param>
		/// <returns>A <see cref="Regex"/> that finds all links matching the values provided. Note that this will match, for example, any of the pagenames given in any of the namespaces given.</returns>
		public static Regex LinkFinderRaw(string regexBefore, string regexNamespaces, string regexPageNames, string regexDisplayTexts, string regexAfter)
		{
			const string regexWild = @"[^#\|\]]*?";
			const string regexWildNamespace = @"[^:#\|\]]*?";
			if (regexBefore != null)
			{
				regexBefore = @"(?<before>" + regexBefore + ")";
			}

			if (regexNamespaces == null)
			{
				regexNamespaces = regexWildNamespace;
			}

			if (regexPageNames == null)
			{
				regexPageNames = regexWild;
			}

			if (regexDisplayTexts == null)
			{
				regexDisplayTexts = regexWild;
			}

			if (regexAfter != null)
			{
				regexAfter = @"(?<after>" + regexAfter + ")";
			}

			// Use string check in case .*? was passed as a parameter instead of null.
			var nsOptional = regexNamespaces == regexWild ? "?" : string.Empty;
			var displayOptional = regexDisplayTexts == regexWild ? "?" : string.Empty;

			return new Regex(regexBefore + @"\[\[(?<pre>:)?\s*((?<namespace>" + regexNamespaces + "):)" + nsOptional + @"(?<pagename>" + regexPageNames + @")?(\#(?<fragment>.*?))?(\s*\|\s*(?<displaytext>" + regexDisplayTexts + @"))" + displayOptional + @"\s*]]" + regexAfter);
		}
		#endregion

		#region Public Methods

		/// <summary>Builds the wiki link text into the specified <see cref="StringBuilder"/>.</summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append to.</param>
		/// <returns>The <paramref name="builder"/> to allow for method chaining.</returns>
		public StringBuilder Build(StringBuilder builder)
		{
			ThrowNull(builder, nameof(builder));
			builder.Append("[[");
			if (this.ForceLink)
			{
				builder.Append(':');
			}

			builder.Append(this.Namespace);
			if (!string.IsNullOrEmpty(this.Namespace))
			{
				builder.Append(':');
			}

			builder.Append(this.PageName);
			if (!string.IsNullOrEmpty(this.Fragment))
			{
				builder.Append('#');
				builder.Append(this.Fragment);
			}

			if (!string.IsNullOrEmpty(this.DisplayText))
			{
				builder.Append('|');
				builder.Append(this.DisplayText);
			}

			builder.Append("]]");

			return builder;
		}

		/// <summary>Returns a suggested DisplayText value based on the link parts, much like the "pipe trick" on a wiki.</summary>
		/// <returns>A suggested DisplayText value based on the link parts.</returns>
		/// <remarks>This method does not modify the DisplayText in any way.</remarks>
		public string PipeTrick() => this.PipeTrick(false);

		/// <summary>Returns a suggested DisplayText value based on the link parts, much like the "pipe trick" on a wiki.</summary>
		/// <param name="useFragmentIfPresent">if set to <c>true</c>, and a fragment exists, uses the fragment to generate the name, rather than the pagename.</param>
		/// <returns>A suggested DisplayText value based on the link parts.</returns>
		/// <remarks>This method does not modify the DisplayText in any way.</remarks>
		public string PipeTrick(bool useFragmentIfPresent)
		{
			string retval;
			if (useFragmentIfPresent && !string.IsNullOrWhiteSpace(this.Fragment))
			{
				retval = this.Fragment;
			}
			else
			{
				retval = this.PageName ?? string.Empty;
				var split = retval.Split(TextArrays.Comma, 2);
				if (split.Length == 1)
				{
					var lastIndex = retval.LastIndexOf('(');
					if (retval.LastIndexOf(')') > lastIndex)
					{
						retval = retval.Substring(0, lastIndex);
					}
				}
				else
				{
					retval = split[0];
				}
			}

			return retval.Replace('_', ' ').Trim();
		}
		#endregion

		#region Public Override Methods

		/// <summary>Converts the <see cref="WikiLink"/> to its full wiki text.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		/// <remarks>This is a simple wrapper around the <see cref="Build(StringBuilder)"/> method.</remarks>
		public override string ToString() => this.Build(new StringBuilder()).ToString();
		#endregion

		#region Private Methods
		private static string EnumerableRegex(IEnumerable<string> input, bool ignoreInitialCaps)
		{
			if (input != null)
			{
				var sb = new StringBuilder();
				foreach (var name in input)
				{
					sb.Append('|');
					if (name.Length > 0)
					{
						sb.Append("(?i:" + Regex.Escape(name.Substring(0, 1)) + ")");
						if (name.Length > 1)
						{
							var nameRemainder = Regex.Escape(name.Substring(1));
							if (ignoreInitialCaps)
							{
								nameRemainder = nameRemainder.Replace(@"\ ", @"[_\ ]+");
							}

							sb.Append(nameRemainder);
						}
					}
				}

				if (sb.Length > 0)
				{
					return sb.ToString(1, sb.Length - 1);
				}
			}

			return null;
		}
		#endregion
	}
}