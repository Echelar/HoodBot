﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Base
{
	using System;

	#region Public Enumerations
	[Flags]
	public enum BlockUserFlags
	{
		None = 0,
		AllowUserTalk = 1,
		AnonymousOnly = 1 << 1,
		AutoBlock = 1 << 2,
		HideName = 1 << 3,
		NoCreate = 1 << 4,
		NoEmail = 1 << 5,
		WatchUser = 1 << 6,
		Reblock = 1 << 7,
	}
	#endregion

	public class BlockInput
	{
		#region Constructors
		public BlockInput(string user) => this.User = user;

		public BlockInput(long userId) => this.UserId = userId;
		#endregion

		#region Public Properties
		public DateTime? Expiry { get; set; }

		public string ExpiryRelative { get; set; }

		public BlockUserFlags Flags { get; set; }

		public string Reason { get; set; }

		public string Token { get; set; }

		public string User { get; }

		public long UserId { get; }
		#endregion
	}
}
