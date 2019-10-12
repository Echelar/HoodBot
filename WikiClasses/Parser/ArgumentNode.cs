﻿namespace RobinHood70.WikiClasses.Parser
{
	using System.Collections;
	using System.Collections.Generic;
	using static RobinHood70.WikiCommon.Globals;

	/// <summary>Represents a template argument, such as <c>{{{1|}}}</c>.</summary>
	public class ArgumentNode : IWikiNode, IEnumerable<NodeCollection>
	{
		#region Fields
		private readonly List<NodeCollection> extraValues = new List<NodeCollection>();
		#endregion

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="ArgumentNode"/> class and parses the values provided.</summary>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		public ArgumentNode(string name, string defaultValue)
		{
			var parsedName = WikiTextParser.Parse(name ?? throw ArgumentNull(nameof(name)));
			var parsedValue = WikiTextParser.Parse(defaultValue ?? throw ArgumentNull(nameof(defaultValue)));
			this.Name = new NodeCollection(this, parsedName);
			this.HandleDefaultValue(parsedValue);
		}

		/// <summary>Initializes a new instance of the <see cref="ArgumentNode"/> class.</summary>
		/// <param name="title">The title.</param>
		/// <param name="defaultValue">The default value. May be null or an empty collection. If populated, this should preferentially be either a single ParameterNode or a collection of IWikiNodes representing the default value itself. For compatibility with MediaWiki, it can also be a list of parameter nodes, in which case, these will be added as individual entries to the AllValues collection.</param>
		public ArgumentNode(IEnumerable<IWikiNode> title, IEnumerable<IWikiNode> defaultValue)
		{
			this.Name = new NodeCollection(this, title ?? throw ArgumentNull(nameof(title)));
			this.HandleDefaultValue(defaultValue);
		}
		#endregion

		#region Public Properties

		/// <summary>Gets the default value.</summary>
		/// <value>The default value. This will be <see langword="null"/> if there is no default value.</value>
		/// <remarks>In order to prevent the possibility of DefaultValue being set to a NodeCollection from another object, it cannot be set directly. Use the provided methods to add or remove default values. You may also trim extraneous values from the object (only available by iterating the ArgumentNode itself).</remarks>
		public NodeCollection? DefaultValue { get; private set; }

		/// <summary>Gets any additional values after the default value (e.g., the b in {{{1|a|b}}}).</summary>
		/// <value>The extra values.</value>
		public IReadOnlyList<NodeCollection> ExtraValues => this.extraValues;

		/// <summary>Gets the name of the argument.</summary>
		/// <value>The argument name.</value>
		public NodeCollection Name { get; }
		#endregion

		#region Public Methods

		/// <summary>Accepts a visitor to process the node.</summary>
		/// <param name="visitor">The visiting class.</param>
		public void Accept(IWikiNodeVisitor visitor) => visitor?.Visit(this);

		/// <summary>Adds a default value. If one exists, this will overwrite it.</summary>
		/// <param name="value">The value to add.</param>
		public void AddDefaultValue(params IWikiNode[] value) => this.AddDefaultValue(value as IEnumerable<IWikiNode>);

		/// <summary>Adds a default value. If one exists, this will overwrite it.</summary>
		/// <param name="value">The value to add.</param>
		public void AddDefaultValue(IEnumerable<IWikiNode> value) => this.DefaultValue = new NodeCollection(this, value);

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<NodeCollection> GetEnumerator()
		{
			if (this.Name != null)
			{
				yield return this.Name;
			}

			foreach (var value in this.extraValues)
			{
				yield return value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		/// <summary>Removes the default value.</summary>
		public void RemoveDefaultValue() => this.DefaultValue = null;

		/// <summary>Trims all extra values from the argument.</summary>
		public void TrimExtraValues() => this.extraValues.Clear();
		#endregion

		#region Public Override Methods

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString()
		{
			var retval = "{{{arg";
			var def = this.DefaultValue;
			if (def != null)
			{
				retval += '|';
				if (def.Count > 0)
				{
					retval += "default";
				}

				if (this.extraValues.Count > 0)
				{
					retval += $"|Extra Count = {this.extraValues.Count}";
				}
			}

			return retval + "}}}";
		}
		#endregion

		#region Private Methods
		private void AddParameterNode(ParameterNode parameter)
		{
			var collection = new NodeCollection(this);
			if (parameter.Name != null)
			{
				foreach (var node in parameter.Name)
				{
					collection.AddLast(node);
				}

				collection.AddLast(new EqualsNode());
			}

			foreach (var node in parameter.Value)
			{
				collection.AddLast(node);
			}

			if (this.DefaultValue == null)
			{
				this.DefaultValue = collection;
			}
			else
			{
				this.extraValues.Add(collection);
			}
		}

		private void HandleDefaultValue(IEnumerable<IWikiNode> defaultValue)
		{
			if (defaultValue is IEnumerable<ParameterNode> parameters)
			{
				foreach (var value in parameters)
				{
					this.AddParameterNode(value);
				}
			}
			else if (defaultValue is ParameterNode parameter)
			{
				this.AddParameterNode(parameter);
			}
			else if (defaultValue != null)
			{
				this.extraValues.Add(new NodeCollection(this, defaultValue));
			}
		}
		#endregion
	}
}