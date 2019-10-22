﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Eve.Modules
{
	using System.Collections.Generic;
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WikiCommon.RequestBuilder;
	using static RobinHood70.WikiCommon.Globals;

	internal class PropDuplicateFiles : PropListModule<DuplicateFilesInput, DuplicateFileItem>, IGeneratorModule
	{
		#region Constructors
		public PropDuplicateFiles(WikiAbstractionLayer wal, DuplicateFilesInput input)
			: this(wal, input, null)
		{
		}

		public PropDuplicateFiles(WikiAbstractionLayer wal, DuplicateFilesInput input, IPageSetGenerator pageSetGenerator)
			: base(wal, input, pageSetGenerator)
		{
		}
		#endregion

		#region Public Override Properties
		public override int MinimumVersion { get; } = 114;

		public override string Name { get; } = "duplicatefiles";
		#endregion

		#region Protected Override Properties
		protected override string Prefix { get; } = "df";
		#endregion

		#region Public Static Methods
		public static PropDuplicateFiles CreateInstance(WikiAbstractionLayer wal, IGeneratorInput input, IPageSetGenerator pageSetGenerator) => new PropDuplicateFiles(wal, input as DuplicateFilesInput, pageSetGenerator);

		public static PropDuplicateFiles CreateInstance(WikiAbstractionLayer wal, IPropertyInput input) => new PropDuplicateFiles(wal, input as DuplicateFilesInput);
		#endregion

		#region Protected Override Methods
		protected override void BuildRequestLocal(Request request, DuplicateFilesInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			request
				.Add("localonly", input.LocalOnly)
				.AddIf("dir", "descending", input.SortDescending)
				.Add("limit", this.Limit);
		}

		protected override DuplicateFileItem? GetItem(JToken result, PageItem page) => result == null
			? null
			: new DuplicateFileItem(
				name: result.MustHaveString("name"),
				shared: result["shared"].ToBCBool(),
				timestamp: result.MustHaveDate("timestamp"),
				user: result.MustHaveString("user"));

		protected override ICollection<DuplicateFileItem> GetMutableList(PageItem page) => (ICollection<DuplicateFileItem>)page.DuplicateFiles;
		#endregion
	}
}
