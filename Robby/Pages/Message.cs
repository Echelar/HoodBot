﻿namespace RobinHood70.Robby.Pages
{
	using Robby.Design;
	using WallE.Base;

	public class Message : Page
	{
		private string defaultMessage = null;

		public Message(Site site, string fullPageName, PageLoadOptions loadOptions)
			: base(site, fullPageName, loadOptions)
		{
		}

		public string DefaultMessage
		{
			get
			{
				if (this.defaultMessage == null && !this.Invalid)
				{
					this.LoadDefault();
				}

				return this.defaultMessage;
			}

			protected set => this.defaultMessage = value;
		}

		public override void Populate(PageItem pageItem)
		{
			if (pageItem.Flags.HasFlag(PageFlags.Missing))
			{
				this.LoadDefault();
			}
			else
			{
				base.Populate(pageItem);
			}
		}

		private void LoadDefault()
		{
			var input = new AllMessagesInput() { Messages = new[] { this.PageName } };
			var result = this.Site.AbstractionLayer.AllMessages(input);
			if (result.Count == 1)
			{
				var message = result[0];
				this.Invalid = false;
				this.Missing = message.Flags.HasFlag(MessageFlags.Missing);
				var content = message.Content ?? string.Empty;
				this.DefaultMessage = content;
				this.Text = content;
			}
			else
			{
				this.Invalid = true;
			}
		}
	}
}