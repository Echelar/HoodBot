﻿namespace RobinHood70.Robby
{
	using System.Collections.Generic;
	using WallE.Base;

	/// <summary>Represents a MediaWiki magic word.</summary>
	public class MagicWord
	{
		internal MagicWord(MagicWordsItem word)
		{
			// Assumes dictionary will hold Id.
			this.CaseSensitive = word.CaseSensitive;
			this.Aliases = new HashSet<string>(word.Aliases);
		}

		/// <summary>Gets any aliases for the word.</summary>
		/// <value>The list of aliases.</value>
		public IReadOnlyCollection<string> Aliases { get; }

		/// <summary>Gets a value indicating whether the magic word is case-sensitive.</summary>
		/// <value><see langword="true" /> if the magic word is case-sensitive; otherwise, <see langword="false" />.</value>
		public bool CaseSensitive { get; }
	}
}
