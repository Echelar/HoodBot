﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Eve.Modules
{
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WikiCommon.RequestBuilder;
	using static RobinHood70.WikiCommon.Globals;

	internal class ListAllCategories : ListModule<AllCategoriesInput, AllCategoriesItem>, IGeneratorModule
	{
		#region Constructors
		public ListAllCategories(WikiAbstractionLayer wal, AllCategoriesInput input)
			: this(wal, input, null)
		{
		}

		public ListAllCategories(WikiAbstractionLayer wal, AllCategoriesInput input, IPageSetGenerator pageSetGenerator)
			: base(wal, input, pageSetGenerator)
		{
		}
		#endregion

		#region Public Override Properties
		public override int MinimumVersion { get; } = 112;

		public override string Name { get; } = "allcategories";
		#endregion

		#region Protected Override Properties
		protected override string Prefix { get; } = "ac";
		#endregion

		#region Public Static Methods
		public static ListAllCategories CreateInstance(WikiAbstractionLayer wal, IGeneratorInput input, IPageSetGenerator pageSetGenerator) => new ListAllCategories(wal, input as AllCategoriesInput, pageSetGenerator);
		#endregion

		#region Protected Override Methods
		protected override void BuildRequestLocal(Request request, AllCategoriesInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			request
				.AddIfNotNull("from", input.From)
				.AddIfNotNull("to", input.To)
				.AddIfNotNull("prefix", input.Prefix)
				.AddIf("dir", "descending", input.SortDescending)
				.AddIf("min", input.MinCount, input.MinCount >= 0)
				.AddIf("max", input.MaxCount, input.MaxCount >= 0)
				.AddFlags("prop", input.Properties)
				.Add("limit", this.Limit);
		}

		protected override AllCategoriesItem GetItem(JToken result)
		{
			ThrowNull(result, nameof(result));
			var item = new AllCategoriesItem()
			{
				Title = (string)result.AsBCContent("category"),
				Hidden = result["hidden"].AsBCBool(),
				Size = (int?)result["size"] ?? 0,
				Pages = (int?)result["pages"] ?? 0,
				Files = (int?)result["files"] ?? 0,
				Subcategories = (int?)result["subcats"] ?? 0,
			};
			return item;
		}
		#endregion
	}
}