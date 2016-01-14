using System;
using Xunit;

internal class StringFormatWithSpec
{
	// Tests taken from Phill's blog.
	public static string Format(string format, object o)
	{
		return format.FormatWith(o);
	}

	[Fact]
	public void StringFormat_WithMultipleExpressions_FormatsThemAll()
	{
		//arrange
		var o = new { foo = 123.45, bar = 42, baz = "hello" };

		//act
		string result = Format("{foo} {foo} {bar}{baz}", o);

		//assert
		Assert.Equal("123.45 123.45 42hello", result);
	}

	[Fact]
	public void StringFormat_WithDoubleEscapedCurlyBraces_DoesNotFormatString()
	{
		//arrange
		var o = new { foo = 123.45 };

		//act
		string result = Format("{{{{foo}}}}", o);

		//assert
		Assert.Equal("{{foo}}", result);
	}

	[Fact]
	public void StringFormat_WithFormatSurroundedByDoubleEscapedBraces_FormatsString()
	{
		//arrange
		var o = new { foo = 123.45 };

		//act
		string result = Format("{{{{{foo}}}}}", o);

		//assert
		Assert.Equal("{{123.45}}", result);
	}

	[Fact]
	public void Format_WithEscapeSequence_EscapesInnerCurlyBraces()
	{
		var o = new { foo = 123.45 };

		//act
		string result = Format("{{{foo}}}", o);

		//assert
		Assert.Equal("{123.45}", result);
	}

	[Fact]
	public void Format_WithEmptyString_ReturnsEmptyString()
	{
		var o = new { foo = 123.45 };

		//act
		string result = Format(string.Empty, o);

		//assert
		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void Format_WithNoFormats_ReturnsFormatStringAsIs()
	{
		var o = new { foo = 123.45 };

		//act
		string result = Format("a b c", o);

		//assert
		Assert.Equal("a b c", result);
	}

	[Fact]
	public void Format_WithFormatType_ReturnsFormattedExpression()
	{
		var o = new { foo = 123.45 };

		//act
		string result = Format("{foo:#.#}", o);

		//assert
		Assert.Equal("123.5", result);
	}

	[Fact]
	public void Format_WithSubProperty_ReturnsValueOfSubProperty()
	{
		var o = new { foo = new { bar = 123.45 } };

		//act
		string result = Format("{foo.bar:#.#}ms", o);

		//assert
		Assert.Equal("123.5ms", result);
	}

	[Fact]
	public void Format_WithFormatNameNotInObject_ThrowsFormatException()
	{
		//arrange
		var o = new { foo = 123.45 };

		//act, assert
		Assert.Throws<FormatException>(() => Format("{bar}", o));
	}

	[Fact]
	public void Format_WithNoEndFormatBrace_ThrowsFormatException()
	{
		//arrange
		var o = new { foo = 123.45 };

		//act, assert
		Assert.Throws<FormatException>(() => Format("{bar", o));
	}

	[Fact]
	public void Format_WithEscapedEndFormatBrace_ThrowsFormatException()
	{
		//arrange
		var o = new { foo = 123.45 };


		//act, assert
		Assert.Throws<FormatException>(() => Format("{foo}}", o));
	}

	[Fact]
	public void Format_WithDoubleEscapedEndFormatBrace_ThrowsFormatException()
	{
		//arrange
		var o = new { foo = 123.45 };

		//act, assert
		Assert.Throws<FormatException>(() => Format("{foo}}}}bar", o));
	}

	[Fact]
	public void Format_WithDoubleEscapedEndFormatBraceWhichTerminatesString_ThrowsFormatException()
	{
		//arrange
		var o = new { foo = 123.45 };

		//act, assert
		Assert.Throws<FormatException>(() => Format("{foo}}}}", o));
	}

	[Fact]
	public void Format_WithEndBraceFollowedByEscapedEndFormatBraceWhichTerminatesString_FormatsCorrectly()
	{
		var o = new { foo = 123.45 };

		//act
		string result = Format("{foo}}}", o);

		//assert
		Assert.Equal("123.45}", result);
	}

	[Fact]
	public void Format_WithEndBraceFollowedByEscapedEndFormatBrace_FormatsCorrectly()
	{
		var o = new { foo = 123.45 };

		//act
		string result = Format("{foo}}}bar", o);

		//assert
		Assert.Equal("123.45}bar", result);
	}

	[Fact]
	public void Format_WithEndBraceFollowedByDoubleEscapedEndFormatBrace_FormatsCorrectly()
	{
		var o = new { foo = 123.45 };

		//act
		string result = Format("{foo}}}}}bar", o);

		//assert
		Assert.Equal("123.45}}bar", result);
	}

	[Fact]
	public void Format_WithNullFormatString_ThrowsArgumentNullException()
	{
		//arrange, act, assert
		Assert.Throws<ArgumentNullException>(() => Format(null, 123));
	}

	[Fact]
	public void Format_WithExpressionReturningNull_DoesNotThrowException()
	{
		//arrange
		var expr = Format("{foo}", new { foo = (object)null });

		//assert
		Assert.Equal(string.Empty, expr);
	}

	[Fact]
	public void Eval_WithNamedExpression_EvalsPropertyOfExpression()
	{
		//act
		var result = Format("{foo}", new { foo = 123 });

		//assert
		Assert.Equal("123", result);
	}

	[Fact]
	public void Eval_WithNamedExpressionAndFormat_EvalsPropertyOfExpression()
	{
		//act
		var result = Format("{foo:#.##}", new { foo = 1.23456 });

		//assert
		Assert.Equal("1.23", result);
	}
}
