﻿namespace RobinHood70.WikiCommon.Parser.StackElements
{
	using System;
	using System.Collections.Generic;
	using RobinHood70.CommonCode;
	using RobinHood70.WikiCommon.Parser;

	internal sealed class HeaderElement : StackElement
	{
		#region Fields
		private readonly int length;
		private readonly int startPos;
		#endregion

		#region Constructors
		internal HeaderElement(WikiStack stack, int length)
			: base(stack)
		{
			this.CurrentPiece.Nodes.Add(stack.NodeFactory.TextNode(new string('=', length)));
			this.length = length;
			this.startPos = stack.Index;
		}
		#endregion

		#region Internal Override Properties
		internal override HeaderPiece CurrentPiece { get; } = new HeaderPiece();

		internal override string SearchString => SearchBase;
		#endregion

		#region Public Override Methods
		public override string ToString() => "h" + this.length.ToStringInvariant();
		#endregion

		#region Internal Override Methods
		internal override List<IWikiNode> BreakSyntax() => this.CurrentPiece.Nodes;

		internal override void Parse(char found)
		{
			var stack = this.Stack;
			if (found == '\n')
			{
				var text = stack.Text;
				var piece = this.CurrentPiece;
				var searchStart = stack.Index - text.SpanReverse(WikiStack.CommentWhiteSpace, stack.Index);
				if (searchStart != 0 && searchStart - 1 == piece.CommentEnd)
				{
					searchStart = piece.VisualEnd - text.SpanReverse(WikiStack.CommentWhiteSpace, searchStart);
				}

				var equalsLength = text.SpanReverse('=', searchStart);
				if (equalsLength > 0)
				{
					var endPos = searchStart - equalsLength;
					var count = endPos == this.startPos
						? equalsLength < 3
							? 0
							: Math.Min(6, (equalsLength - 1) / 2)
						: Math.Min(equalsLength, this.length);

					if (count > 0)
					{
						var headerNode = this.Stack.NodeFactory.HeaderNode(count, piece.Nodes);
						stack.Pop();
						stack.Top.CurrentPiece.Nodes.Add(headerNode);
						return;
					}
				}

				stack.Pop();
				stack.Top.CurrentPiece.Merge(piece.Nodes);
			}
			else
			{
				stack.ParseCharacter(found);
			}
		}
		#endregion
	}
}
