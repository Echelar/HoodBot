﻿namespace RobinHood70.WikiClasses.Parser
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using RobinHood70.WikiClasses.Parser.Nodes;
	using RobinHood70.WikiClasses.Parser.StackElements;
	using static System.FormattableString;

	// Not a .NET Stack<T> mostly for closer parity with the original PHP version, plus it significantly outperforms the built-in one. Top, being a property, also provides a significant debugging advantage over Peek().
	internal class WikiStack
	{
		#region Internal Constants
		internal const string CommentWhiteSpace = " \t";
		#endregion

		#region Private Constants
		private const int StartSize = 4;
		private const string IncludeOnlyTag = "includeonly";
		private const string NoIncludeTag = "noinclude";
		private const string OnlyIncludeTag = "onlyinclude";
		private const string OnlyIncludeTagClose = "</" + OnlyIncludeTag + ">";
		private const string OnlyIncludeTagOpen = "<" + OnlyIncludeTag + ">";
		#endregion

		#region Static Fields
		private static readonly HashSet<string> AllowMissingEndTag = new HashSet<string> { IncludeOnlyTag, NoIncludeTag, OnlyIncludeTag };
		#endregion

		#region Fields
		private readonly bool enableOnlyInclude;
		private readonly HashSet<string> ignoredElements = new HashSet<string>();
		private readonly HashSet<string> ignoredTags = new HashSet<string>();
		private readonly HashSet<string> noMoreClosingTag = new HashSet<string>();
		private readonly int textLength;
		private readonly Regex tagsRegex;
		private StackElement[] array;
		private int count;
		private bool findOnlyinclude;
		#endregion

		#region Constructors
		public WikiStack(string text, ICollection<string> tagList, bool? include)
		{
			this.array = new StackElement[StartSize];
			this.count = 0;
			this.Push(new RootElement(this));

			this.Text = text;
			this.textLength = text.Length;
			this.enableOnlyInclude = text.Contains(OnlyIncludeTagOpen);
			this.findOnlyinclude = this.enableOnlyInclude;
			var allTags = new HashSet<string>(tagList);
			switch (include)
			{
				case true:
					this.ignoredTags.UnionWith(new[] { IncludeOnlyTag, "/" + IncludeOnlyTag });
					this.ignoredElements.Add(NoIncludeTag);
					allTags.Add(NoIncludeTag);
					break;
				case false:
					this.ignoredTags.UnionWith(new[] { NoIncludeTag, "/" + NoIncludeTag, OnlyIncludeTag, "/" + OnlyIncludeTag });
					this.ignoredElements.Add(IncludeOnlyTag);
					allTags.Add(IncludeOnlyTag);
					break;
				default:
					// TODO: Figure out what's correct here. This is only a preliminary guess. Intended for parsing raw pages where inclusion is irrelevant.
					allTags.UnionWith(AllowMissingEndTag);
					break;
			}

			allTags.UnionWith(this.ignoredTags);
			var regexTags = new List<string>(allTags.Count);
			foreach (var tag in allTags)
			{
				regexTags.Add(Regex.Escape(tag));
			}

			regexTags.Sort();
			this.tagsRegex = new Regex(@"\G(" + string.Join("|", regexTags) + @")", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}
		#endregion

		#region Public Properties
		public int HeadingIndex { get; set; } = 1;

		public int Index { get; set; }

		public string Text { get; }

		public StackElement Top { get; private set; }
		#endregion

		#region Private Properties
		private char CurrentCharacter => this.Text[this.Index];
		#endregion

		#region Public Methods
		public NodeCollection Merge()
		{
			var root = this.array[0].CurrentPiece;
			for (var i = 1; i < this.count; i++)
			{
				root.Merge(this.array[i].BreakSyntax());
			}

			for (var i = 0; i < root.Count; i++)
			{
				if (root[i] is HeaderNode hNode && !hNode.Confirmed)
				{
					hNode.Confirmed = true;
				}
			}

			return root;
		}

		public void Parse(char found)
		{
			switch (found)
			{
				case '\n':
					this.Top.CurrentPiece.AddLiteral("\n");
					this.Index++;
					this.ParseLineStart();
					break;
				case '<':
					if (this.enableOnlyInclude && string.Compare(this.Text, this.Index, OnlyIncludeTagClose, 0, OnlyIncludeTagClose.Length, StringComparison.OrdinalIgnoreCase) == 0)
					{
						this.findOnlyinclude = true;
					}
					else if (string.Compare(this.Text, this.Index + 1, "!--", 0, 3, StringComparison.Ordinal) == 0)
					{
						if (this.FoundComment())
						{
							this.ParseLineStart();
						}
					}
					else
					{
						var tagMatch = this.tagsRegex.Match(this.Text, this.Index + 1);
						if (!tagMatch.Success || !this.FoundTag(tagMatch.Groups[1].Value))
						{
							this.Top.CurrentPiece.AddLiteral("<");
							this.Index++;
						}
					}

					break;
				case '{':
					var countCurly = this.Text.Span('{', this.Index);
					if (countCurly >= 2)
					{
						this.Push(new TemplateElement(this, countCurly, this.Index > 0 && this.Text[this.Index - 1] == '\n'));
					}
					else
					{
						this.Top.CurrentPiece.AddLiteral(new string('{', countCurly));
					}

					this.Index += countCurly;
					break;
				case '[':
					// Repetitive with above, but slightly faster than combining, on average.
					var countSquare = this.Text.Span('[', this.Index);
					if (countSquare >= 2)
					{
						this.Push(new LinkElement(this, countSquare));
					}
					else
					{
						this.Top.CurrentPiece.AddLiteral(new string('[', countSquare));
					}

					this.Index += countSquare;
					break;
				default:
					throw new InvalidOperationException(Invariant($"Found unexpected character '{this.CurrentCharacter}' at position {this.Index}."));
			}
		}

		public void Pop()
		{
			if (this.count-- < 2)
			{
				throw new InvalidOperationException("Stack is empty or you attempted to pop the root node.");
			}

			this.array[this.count] = null;
			this.Top = this.array[this.count - 1];
		}

		public void Preprocess()
		{
			if (this.textLength > 0 && this.Text[0] == '=')
			{
				this.ParseLineStart();
			}

			do
			{
				if (this.findOnlyinclude)
				{
					var startPos = this.Text.IndexOf(OnlyIncludeTagOpen, this.Index, StringComparison.OrdinalIgnoreCase);
					if (startPos == -1)
					{
						this.Top.CurrentPiece.Add(new IgnoreNode(this.Text.Substring(this.Index)));
						break;
					}

					var tagEndPos = startPos + OnlyIncludeTagOpen.Length; // past-the-end
					this.Top.CurrentPiece.Add(new IgnoreNode(this.Text.Substring(this.Index, tagEndPos - this.Index)));
					this.Index = tagEndPos;
					this.findOnlyinclude = false;
				}

				var search = this.Top.SearchString;
				var literalOffset = this.Text.IndexOfAny(search.ToCharArray(), this.Index);
				if (literalOffset == -1)
				{
					literalOffset = this.textLength;
				}

				if (literalOffset != this.Index)
				{
					this.Top.CurrentPiece.AddLiteral(this.Text.Substring(this.Index, literalOffset - this.Index));
					this.Index = literalOffset;
					if (this.Index >= this.textLength)
					{
						break;
					}
				}

				this.Top.Parse(this.CurrentCharacter);
			}
			while (this.Index < this.textLength);

			var lastHeader = this.Top as HeaderElement;
			lastHeader?.Parse('\n');
		}

		public void Push(StackElement item)
		{
			if (this.count == this.array.Length)
			{
				var newArray = new StackElement[this.count << 1];
				Array.Copy(this.array, newArray, this.count);
				this.array = newArray;
			}

			this.array[this.count++] = item;
			this.Top = item;
		}
		#endregion

		#region Private Methods

		// Returns true if comment(s) are surrounded by NewLines, so caller knows whether to check for a possible header.
		private bool FoundComment()
		{
			var piece = this.Top.CurrentPiece;
			var endPos = this.Text.IndexOf("-->", this.Index + 4, StringComparison.Ordinal) + 3;
			if (endPos == 2)
			{
				piece.Add(new CommentNode(this.Text.Substring(this.Index)));
				this.Index = this.textLength;
				return false;
			}

			var comments = new List<Comment>();
			var wsStart = this.Index - this.Text.SpanReverse(CommentWhiteSpace, this.Index);
			var closing = endPos;
			var wsEnd = wsStart;
			do
			{
				var length = this.Text.Span(CommentWhiteSpace, closing);
				comments.Add(new Comment(wsEnd, closing, length));
				wsEnd = closing + length;
				closing = string.Compare(this.Text, wsEnd, "<!--", 0, 4, StringComparison.Ordinal) == 0
					? this.Text.IndexOf("-->", wsEnd + 4, StringComparison.Ordinal) + 3
					: 2;
			}
			while (closing != 2);

			var retval = false;
			int startPos;
			Comment cmt;
			if (wsStart > 0 && wsEnd < this.textLength && this.Text[wsStart - 1] == '\n' && this.Text[wsEnd] == '\n')
			{
				var wsLength = this.Index - wsStart;
				if (wsLength > 0)
				{
					var last = piece[piece.Count - 1] as TextNode;
					var lastValue = last.Text;
					if (lastValue.SpanReverse(CommentWhiteSpace, lastValue.Length) == wsLength)
					{
						last.Text = lastValue.Substring(0, lastValue.Length - wsLength);
					}
				}

				var lastComment = comments.Count - 1;
				for (var j = 0; j < lastComment; j++)
				{
					cmt = comments[j];
					piece.Add(new CommentNode(this.Text.Substring(cmt.Start, cmt.End - cmt.Start + cmt.WhiteSpaceLength)));
				}

				cmt = comments[lastComment];
				startPos = cmt.Start;
				endPos = cmt.End + cmt.WhiteSpaceLength + 1; // Grab the trailing \n

				retval = true;
			}
			else
			{
				// Since we have the comments all gathered up and everything between them is known to be text, don't backtrack, add them all here.
				var lastComment = comments.Count - 1;
				for (var j = 0; j < lastComment; j++)
				{
					cmt = comments[j];
					var start = j == 0 ? this.Index : cmt.Start;
					piece.Add(new CommentNode(this.Text.Substring(start, cmt.End - start)));
					piece.Add(new TextNode(this.Text.Substring(cmt.End, cmt.WhiteSpaceLength)));
				}

				cmt = comments[lastComment];
				startPos = lastComment == 0 ? this.Index : cmt.Start;
				endPos = cmt.End;
			}

			if (piece.CommentEnd != wsStart - 1)
			{
				piece.VisualEnd = wsStart;
			}

			piece.CommentEnd = endPos - 1;
			piece.Add(new CommentNode(this.Text.Substring(startPos, endPos - startPos)));
			this.Index = endPos;

			return retval;
		}

		// Returns true if a valid tag was found.
		private bool FoundTag(string tagOpen)
		{
			var piece = this.Top.CurrentPiece;
			var tagNameLower = tagOpen.ToLowerInvariant();
			var attrStart = this.Index + tagNameLower.Length + 1;
			var tagEndPos = this.Text.IndexOf('>', attrStart);
			if (tagEndPos == -1)
			{
				piece.AddLiteral("<");
				this.Index++;
				return false;
			}

			if (this.ignoredTags.Contains(tagNameLower))
			{
				piece.Add(new IgnoreNode(this.Text.Substring(this.Index, tagEndPos - this.Index + 1)));
				this.Index = tagEndPos + 1;
				return true;
			}

			var tagStartPos = this.Index;
			int attrEnd;
			string tagClose;
			string inner;
			if (this.Text[tagEndPos - 1] == '/')
			{
				inner = null;
				tagClose = null;
				attrEnd = tagEndPos - 1;
				this.Index = tagEndPos + 1;
			}
			else
			{
				attrEnd = tagEndPos;
				var findClosing = new Regex(@"</" + tagNameLower + @"\s*>", RegexOptions.IgnoreCase);
				Match match;
				if (!this.noMoreClosingTag.Contains(tagNameLower) && (match = findClosing.Match(this.Text, tagEndPos + 1)).Success)
				{
					inner = this.Text.Substring(tagEndPos + 1, match.Index - tagEndPos - 1);
					tagClose = match.Value;
					this.Index = match.Index + match.Length;
				}
				else if (AllowMissingEndTag.Contains(tagNameLower))
				{
					inner = this.Text.Substring(tagEndPos + 1);
					tagClose = null;
					this.Index = this.textLength;
				}
				else
				{
					this.Index = tagEndPos + 1;
					piece.AddLiteral(this.Text.Substring(tagStartPos, this.Index - tagStartPos));
					this.noMoreClosingTag.Add(tagNameLower);
					return true;
				}
			}

			if (this.ignoredElements.Contains(tagNameLower))
			{
				piece.Add(new IgnoreNode(this.Text.Substring(tagStartPos, this.Index - tagStartPos)));
			}
			else
			{
				var attr = attrEnd > attrStart ? this.Text.Substring(attrStart, attrEnd - attrStart) : null;
				piece.Add(new TagNode(tagOpen, attr, inner, tagClose));
			}

			return true;
		}

		private void ParseLineStart()
		{
			var equalsCount = this.Text.Span('=', this.Index, 6);

			// DWIM (for count==1 check): This looks kind of like a name/value separator. Let's let the equals handler have it and break the potential heading. This is heuristic, but AFAICT the methodsfor completely correct disambiguation are very complex.
			// Using LastIndexOf as a very minor optimization, since we know the = is added last for a PairedElement, but won't break if this is not the case in the future.
			if (equalsCount > 1 || (equalsCount == 1 && this.Top.SearchString.LastIndexOf('=') == -1))
			{
				this.Push(new HeaderElement(this, equalsCount));
				this.Index += equalsCount;
			}
		}
		#endregion

		#region Private Structures
		private struct Comment
		{
			public Comment(int start, int end, int wsLength)
			{
				this.Start = start;
				this.End = end;
				this.WhiteSpaceLength = wsLength;
			}

			public int End { get; set; }

			public int Start { get; set; }

			public int WhiteSpaceLength { get; set; }
		}
		#endregion
	}
}