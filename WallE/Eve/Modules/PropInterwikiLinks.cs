﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Eve.Modules
{
	using System;
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WallE.RequestBuilder;
	using static RobinHood70.WikiCommon.Globals;

	internal class PropInterwikiLinks : PropListModule<InterwikiLinksInput, InterwikiTitleItem>
	{
		#region Constructors
		public PropInterwikiLinks(WikiAbstractionLayer wal, InterwikiLinksInput input)
			: base(wal, input)
		{
		}
		#endregion

		#region Public Override Properties
		public override int MinimumVersion { get; } = 117;

		public override string Name { get; } = "iwlinks";
		#endregion

		#region Protected Override Properties
		protected override string Prefix { get; } = "iw";
		#endregion

		#region Public Static Methods
		public static PropInterwikiLinks CreateInstance(WikiAbstractionLayer wal, IPropertyInput input) => new PropInterwikiLinks(wal, input as InterwikiLinksInput);
		#endregion

		#region Protected Override Methods
		protected override void BuildRequestLocal(Request request, InterwikiLinksInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			request
				.AddIf("url", input.Properties.HasFlag(InterwikiLinksProperties.Url), this.SiteVersion < 124)
				.AddFlagsIf("prop", input.Properties, this.SiteVersion >= 124)
				.AddIfNotNull("prefix", input.Prefix)
				.AddIfNotNull("title", input.Title)
				.AddIf("dir", "descending", input.SortDescending)
				.Add("limit", this.Limit);
		}

		protected override InterwikiTitleItem GetItem(JToken result) => result == null
			? null
			: new InterwikiTitleItem()
			{
				InterwikiPrefix = (string)result["prefix"],
				Title = (string)result.AsBCContent("title"),
				Url = (Uri)result["url"],
			};

		protected override void GetResultsFromCurrentPage() => this.ResetItems(this.Output.InterwikiLinks);

		protected override void SetResultsOnCurrentPage() => this.Output.InterwikiLinks = this.Items;
		#endregion
	}
}
