﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Eve.Modules
{
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WikiCommon.RequestBuilder;
	using static RobinHood70.WikiCommon.Globals;

	// MWVERSION: 1.29
	internal class ActionUnblock : ActionModuleValued<UnblockInput, UnblockResult>
	{
		#region Constructors
		public ActionUnblock(WikiAbstractionLayer wal)
			: base(wal)
		{
		}
		#endregion

		#region Public Override Properties
		public override int MinimumVersion { get; } = 112;

		public override string Name { get; } = "unblock";
		#endregion

		#region Protected Override Properties
		protected override RequestType RequestType { get; } = RequestType.Post;
		#endregion

		#region Protected Override Methods
		protected override void BuildRequestLocal(Request request, UnblockInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			request
				.AddIfPositive("id", input.Id)
				.AddIfPositive("userid", input.UserId)
				.AddIfNotNull("user", input.User)
				.AddIfNotNull("reason", input.Reason)
				.Add("tags", input.Tags)
				.AddHidden("token", input.Token);
		}

		protected override UnblockResult DeserializeResult(JToken result)
		{
			ThrowNull(result, nameof(result));
			var userNode = result.MustHave("user");

			string user;
			long userId;
			if (userNode.Type == JTokenType.Object)
			{
				// Deals with https://phabricator.wikimedia.org/T45518 in MW 1.18 and early versions of 1.19/1.20
				user = userNode.MustHaveString("mName");
				userId = (long?)userNode["mId"] ?? 0;
			}
			else
			{
				user = (string?)userNode ?? string.Empty;
				userId = (long?)result["userid"] ?? 0;
			}

			return new UnblockResult(
				id: (long)result.MustHave("id"),
				user: user,
				userId: userId,
				reason: (string?)result["reason"]);
		}
		#endregion
	}
}