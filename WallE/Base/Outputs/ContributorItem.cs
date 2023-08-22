﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Base
{
	public class ContributorItem
	{
		#region Constructors
		internal ContributorItem(string name, long userId)
		{
			this.Name = name;
			this.UserId = userId;
		}
		#endregion

		#region Public Properties
		public string Name { get; }

		public long UserId { get; }
		#endregion

		#region Public Override Methods
		public override string ToString() => this.Name;
		#endregion
	}
}