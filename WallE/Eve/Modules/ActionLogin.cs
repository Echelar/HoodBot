﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Eve.Modules
{
	using System;
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WikiCommon.RequestBuilder;
	using static RobinHood70.WallE.ProjectGlobals;
	using static RobinHood70.WikiCommon.Globals;

	internal class ActionLogin : ActionModule<LoginInput, LoginResult>
	{
		#region Constructors
		public ActionLogin(WikiAbstractionLayer wal)
			: base(wal)
		{
		}
		#endregion

		#region Public Override Properties
		public override int MinimumVersion { get; } = 0;

		public override string Name { get; } = "login";

		public override string FullPrefix { get; } = "lg";
		#endregion

		#region Protected Override Properties
		protected override RequestType RequestType { get; } = RequestType.Post;

		protected override StopCheckMethods StopMethods { get; } = StopCheckMethods.None;
		#endregion

		#region Protected Override Methods
		protected override void AddWarning(string from, string text)
		{
			ThrowNullOrWhiteSpace(text, nameof(text));
			if (!text.StartsWith("Main-account login", StringComparison.Ordinal))
			{
				base.AddWarning(from, text);
			}
		}

		protected override void BuildRequestLocal(Request request, LoginInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			request
				.AddIfNotNull("name", input.UserName)
				.AddHiddenIfNotNull("password", input.Password)
				.AddIfNotNull("domain", input.Domain)
				.AddHiddenIfNotNull("token", input.Token);
		}

		protected override LoginResult DeserializeResult(JToken result)
		{
			ThrowNull(result, nameof(result));
			return new LoginResult(
				result: result.MustHaveString("result"),
				reason: (string?)result["reason"],
				user: (string?)result["lgusername"],
				userId: (long?)result["lguserid"] ?? -1,
				token: (string?)result["token"],
				waitTime: TimeSpan.FromSeconds((int?)result["wait"] ?? 0));
		}
		#endregion
	}
}