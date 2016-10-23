

using System;

namespace CommonTypesLibrary {

	public class TextEventArgs : EventArgs {

		private readonly string _text;

		public TextEventArgs(string text) {
			_text = text;
		}

		public string Text {
			get { return _text; }
		}
	}
}