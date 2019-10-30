﻿namespace RobinHood70.HoodBot.Uesp
{
	using System.Collections.Generic;

	public class VariableItem
	{
		#region Constructors
		public VariableItem(IReadOnlyDictionary<string, string> dictionary, string subset)
		{
			this.Dictionary = new VariableDictionary(dictionary);
			this.Subset = subset;
		}
		#endregion

		#region Public Properties
		public VariableDictionary Dictionary { get; }

		public string Subset { get; }
		#endregion
	}
}