﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Eve.Modules
{
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WallE.RequestBuilder;
	using static RobinHood70.WikiCommon.Globals;

	internal class ListPagesWithProp : ListModule<PagesWithPropertyInput, PagesWithPropertyItem>, IGeneratorModule
	{
		#region Constructors
		public ListPagesWithProp(WikiAbstractionLayer wal, PagesWithPropertyInput input)
			: base(wal, input)
		{
		}
		#endregion

		#region Protected Internal Override Properties
		public override int MinimumVersion { get; } = 121;

		public override string Name { get; } = "pageswithprop";
		#endregion

		#region Public Override Properties
		protected override string Prefix { get; } = "pwp";
		#endregion

		#region Public Override Methods
		protected override void BuildRequestLocal(Request request, PagesWithPropertyInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			request
				.AddIfNotNull("propname", input.PropertyName)
				.AddFlags("prop", input.Properties)
				.AddIf("dir", "descending", input.SortDescending)
				.Add("limit", this.Limit);
		}

		protected override PagesWithPropertyItem GetItem(JToken result)
		{
			if (result == null)
			{
				return null;
			}

			var item = new PagesWithPropertyItem();
			item.GetWikiTitle(result);
			item.Value = (string)result["value"];

			return item;
		}
		#endregion
	}
}
