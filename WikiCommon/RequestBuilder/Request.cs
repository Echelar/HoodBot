﻿namespace RobinHood70.WikiCommon.RequestBuilder
{
	using System;
	using static RobinHood70.WikiCommon.Globals;

	#region Public Enumerations

	/// <summary>A combination of the HTTP method and the content type.</summary>
	public enum RequestType
	{
		/// <summary>HTTP GET method with form-urlencoded data.</summary>
		Get,

		/// <summary>HTTP POST method with form-urlencoded data.</summary>
		Post,

		/// <summary>HTTP POST method with multipart form data.</summary>
		PostMultipart
	}
	#endregion

	/// <summary>Builds a form query parameter by parameter. Provides methods to add parameters of built-in types and convert them to what MediaWiki expects.</summary>
	/// <seealso cref="ParameterCollection" />
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "More logical name.")]
	public class Request : ParameterCollection
	{
		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="Request" /> class.</summary>
		/// <param name="baseUri">The base URI.</param>
		/// <param name="requestType">The request type.</param>
		/// <param name="supportsUnitSeparator">if set to <see langword="true" /> [supports unit separator].</param>
		public Request(Uri baseUri, RequestType requestType, bool supportsUnitSeparator)
		{
			ThrowNull(baseUri, nameof(baseUri));
			this.Uri = baseUri;
			this.Type = requestType;
			this.SupportsUnitSeparator = supportsUnitSeparator;
		}
		#endregion

		#region Public Properties

		/// <summary>Gets a value indicating whether the wiki supports \x1F unit separators.</summary>
		/// <value><see langword="true" /> if the wiki supports \x1F unit separators; otherwise, <see langword="false" />.</value>
		public bool SupportsUnitSeparator { get; } = false;

		/// <summary>Gets or sets the request type. Defaults to <see cref="RequestType.Get" />.</summary>
		/// <value>The requesty type.</value>
		public RequestType Type { get; set; }

		/// <summary>Gets the base URI for the request.</summary>
		/// <value>The base URI for the request.</value>
		public Uri Uri { get; }
		#endregion

		#region Public Methods

		/// <summary>Builds the request using the specified IParameterVisitor.</summary>
		/// <param name="visitor">The visitor.</param>
		public void Build(IParameterVisitor visitor)
		{
			foreach (var param in this)
			{
				param.Accept(visitor);
			}
		}
		#endregion

		#region Public Override Methods

		/// <summary>Returns a displayable value for the request.</summary>
		/// <returns>An <i>unencoded</i>, displayable Uri-like string with hidden and binary values treated appropriately.</returns>
		public override string ToString() => RequestVisitorDisplay.Build(this);
		#endregion
	}
}
