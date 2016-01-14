// Originally appeared in http://haacked.com/archive/2009/01/14/named-formats-redux.aspx
// Authored by Henri Wiechers
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

/// <summary>
/// Requires a reference to System.Web.
/// </summary>
[EditorBrowsable (EditorBrowsableState.Never)]
public static partial class StringExtensions
{
	/// <summary>
	/// Formats the string with the given source object. 
	/// Expression like {Id} are replaced with the corresponding 
	/// property value in the <paramref name="source"/>. Supports 
	/// all <c>DataBinder.Eval</c> expressions formats 
	/// for property access.
	/// </summary>
	/// <param name="format" this="true">The string to format</param>
	/// <param name="source">The source object to apply to format</param>
	public static string FormatWith (this string format, object source)
	{
		if (format == null)
			throw new ArgumentNullException ("format");

		var result = new StringBuilder(format.Length * 2);

		using (var reader = new StringReader (format)) {
			var expression = new StringBuilder();
			var @char = -1;

			var state = State.OutsideExpression;
			do {
				switch (state) {
					case State.OutsideExpression:
						@char = reader.Read ();
						switch (@char) {
							case -1:
								state = State.End;
								break;
							case '{':
								state = State.OnOpenBracket;
								break;
							case '}':
								state = State.OnCloseBracket;
								break;
							default:
								result.Append ((char)@char);
								break;
						}
						break;
					case State.OnOpenBracket:
						@char = reader.Read ();
						switch (@char) {
							case -1:
								throw new FormatException ();
							case '{':
								result.Append ('{');
								state = State.OutsideExpression;
								break;
							default:
								expression.Append ((char)@char);
								state = State.InsideExpression;
								break;
						}
						break;
					case State.InsideExpression:
						@char = reader.Read ();
						switch (@char) {
							case -1:
								throw new FormatException ();
							case '}':
								result.Append (OutExpression (source, expression.ToString ()));
								expression.Length = 0;
								state = State.OutsideExpression;
								break;
							default:
								expression.Append ((char)@char);
								break;
						}
						break;
					case State.OnCloseBracket:
						@char = reader.Read ();
						switch (@char) {
							case '}':
								result.Append ('}');
								state = State.OutsideExpression;
								break;
							default:
								throw new FormatException ();
						}
						break;
					default:
						throw new InvalidOperationException ("Invalid state.");
				}
			} while (state != State.End);
		}

		return result.ToString ();
	}

	private static string OutExpression (object source, string expression)
	{
		var format = "";
		var colonIndex = expression.IndexOf(':');

		if (colonIndex > 0) {
			format = expression.Substring (colonIndex + 1);
			expression = expression.Substring (0, colonIndex);
		}

		try {
			if (string.IsNullOrEmpty (format))
				return (DataBinder.Eval (source, expression) ?? "").ToString ();
			else
				return DataBinder.Eval (source, expression, "{0:" + format + "}") ?? "";
		} catch (HttpException) {
			throw new FormatException ("Failed to format '" + expression + "'.");
		}
	}

	private enum State
	{
		OutsideExpression,
		OnOpenBracket,
		InsideExpression,
		OnCloseBracket,
		End
	}
}