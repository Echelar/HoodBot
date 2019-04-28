﻿namespace RobinHood70.WikiClasses.Parser
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using System.Web.Security.AntiXss;
	using RobinHood70.WikiClasses.Parser.Nodes;
	using static WikiCommon.Globals;

	public class XmlVisitor : IVisitor
	{
		#region Fields
		private readonly StringBuilder builder = new StringBuilder();
		private readonly NodeCollection nodes;
		private readonly bool prettyPrint;
		private int indent;
		#endregion

		#region Constructors
		public XmlVisitor(NodeCollection nodes, bool prettyPrint)
		{
			this.nodes = nodes;
			this.prettyPrint = prettyPrint;
		}
		#endregion

		#region IVisitor Methods
		public void Visit(CommentNode node)
		{
			ThrowNull(node, nameof(node));
			this.BuildValueNode("comment", node.Comment);
		}

		public void Visit(EqualsNode node)
		{
			ThrowNull(node, nameof(node));
			this.Indent();
			this.builder.Append('=');
		}

		public void Visit(HeaderNode node)
		{
			ThrowNull(node, nameof(node));
			this.BuildTag("h", new Dictionary<string, int> { ["level"] = node.Level, ["i"] = node.Index }, node.Title);
		}

		public void Visit(IgnoreNode node)
		{
			ThrowNull(node, nameof(node));
			this.BuildValueNode("ignore", node.Value);
		}

		public void Visit(LinkNode node)
		{
			ThrowNull(node, nameof(node));
			this.BuildTagOpen("link", null, false);
			this.BuildTag("title", null, node.Title); // Title is always emitted, even if empty.
			foreach (var part in node.Parameters)
			{
				part.Accept(this);
			}

			this.BuildTagClose("link");
		}

		public void Visit(NodeCollection nodes)
		{
			foreach (var node in nodes)
			{
				node.Accept(this);
			}
		}

		public void Visit(ParameterNode node)
		{
			ThrowNull(node, nameof(node));
			this.BuildTagOpen("part", null, false);
			if (node.Index == 0)
			{
				this.BuildTag("name", null, node.Name);
				this.Indent();
				this.builder.Append('=');
			}
			else
			{
				this.BuildTag("name", new Dictionary<string, int> { ["index"] = node.Index }, null);
			}

			this.BuildTag("value", null, node.Value);
			this.BuildTagClose("part");
		}

		public void Visit(TagNode node)
		{
			ThrowNull(node, nameof(node));
			this.BuildTagOpen("ext", null, false);
			this.BuildValueNode("name", node.Name);
			this.BuildValueNode("attr", node.Attributes);
			if (node.InnerText != null)
			{
				this.BuildValueNode("inner", node.InnerText);
			}

			if (node.Close != null)
			{
				this.BuildValueNode("close", node.Close);
			}

			this.BuildTagClose("ext");
		}

		public void Visit(TemplateNode node)
		{
			ThrowNull(node, nameof(node));
			var name = node.NodeType == TemplateNodeType.Template ? "template" : "tplarg";
			this.BuildTagOpen(name, node.AtLineStart ? new Dictionary<string, int> { ["lineStart"] = 1 } : null, false);
			this.BuildTag("title", null, node.Title); // Title is always emitted, even if empty.
			foreach (var part in node.Parameters)
			{
				part.Accept(this);
			}

			this.BuildTagClose(name);
		}

		public void Visit(TextNode node)
		{
			ThrowNull(node, nameof(node));
			this.Indent();
			this.builder.Append(AntiXssEncoder.HtmlEncode(node.Text.Replace(' ', '_'), true));
		}
		#endregion

		#region Public Methods
		public string Build()
		{
			this.BuildTagOpen("root", null, false);
			this.nodes.Accept(this);
			this.BuildTagClose("root");

			return this.builder.ToString();
		}
		#endregion

		#region Private Methods
		private void BuildTag(string name, Dictionary<string, int> attributes, NodeCollection inner)
		{
			var selfClosed = inner == null || inner.Count == 0;
			this.BuildTagOpen(name, attributes, selfClosed);
			if (!selfClosed)
			{
				foreach (var node in inner)
				{
					node.Accept(this);
				}

				this.BuildTagClose(name);
			}
		}

		private void BuildTagClose(string name)
		{
			this.indent--;
			this.Indent();
			this.builder.Append("</").Append(name).Append(">\n");
		}

		private void BuildTagOpen(string name, Dictionary<string, int> attributes, bool selfClosed)
		{
			this.Indent();
			this.builder.Append('<').Append(name);
			if (attributes != null)
			{
				foreach (var kvp in attributes)
				{
					this.builder.Append(' ').Append(kvp.Key).Append("=\"").Append(kvp.Value.ToString(CultureInfo.InvariantCulture)).Append('"');
				}
			}

			if (selfClosed)
			{
				this.builder.Append("/>");
			}
			else
			{
				this.builder.Append(">\n");
				this.indent++;
			}
		}

		private void BuildValueNode(string name, string value)
		{
			this.Indent();
			this.builder.Append('<').Append(name).Append('>').Append(AntiXssEncoder.HtmlEncode(value, true)).Append("</").Append(name).Append('>');
		}

		private void Indent()
		{
			if (this.prettyPrint)
			{
				if (this.builder.Length > 0 && this.builder[this.builder.Length - 1] != '\n')
				{
					this.builder.Append('\n');
				}

				this.builder.Append(new string('\t', this.indent));
			}
		}
		#endregion
	}
}