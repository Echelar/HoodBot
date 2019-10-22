﻿namespace RobinHood70.HoodBot.Uesp
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WallE.Eve;
	using RobinHood70.WallE.Eve.Modules;
	using RobinHood70.WikiCommon.RequestBuilder;
	using static RobinHood70.WikiCommon.Globals;

	public class PropVariables : PropListModule<VariablesInput, VariableItem>, IGeneratorModule
	{
		#region Constructors
		public PropVariables(WikiAbstractionLayer wal, VariablesInput input)
			: this(wal, input, null)
		{
		}

		public PropVariables(WikiAbstractionLayer wal, VariablesInput input, IPageSetGenerator pageSetGenerator)
			: base(wal, input, pageSetGenerator)
		{
		}
		#endregion

		#region Public Override Properties
		public override int MinimumVersion => 110;

		public override string Name => "metavars";
		#endregion

		#region Protected Override Properties
		protected override string Prefix => "mv";
		#endregion

		#region Public Static Methods
		public static PropVariables CreateInstance(WikiAbstractionLayer wal, IGeneratorInput input, IPageSetGenerator pageSetGenerator) => new PropVariables(wal, input as VariablesInput, pageSetGenerator);

		public static PropVariables CreateInstance(WikiAbstractionLayer wal, IPropertyInput input) => new PropVariables(wal, input as VariablesInput);
		#endregion

		#region Protected Override Methods
		protected override void BuildRequestLocal(Request request, VariablesInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			request
				.Add("var", input.Variables)
				.Add("subset", input.Subsets)
				.Add("limit", this.Limit);
		}

		protected override VariableItem GetItem(JToken result, PageItem page)
		{
			ThrowNull(result, nameof(result));
			var vars = result["vars"].ToObject<Dictionary<string, string>>();
			var subset = (string)result["subset"];
			return new VariableItem(vars, subset);
		}

		protected override ICollection<VariableItem> GetMutableList(PageItem page)
		{
			var varPage = page as VariablesPageItem ?? throw new InvalidOperationException();
			return varPage.Variables as ICollection<VariableItem>;
		}
		#endregion
	}
}