using System;
using Xunit;

namespace Clide
{
	public class GuardSpec
	{
		[Fact]
		public void when_null_variable_reference_passed_then_throws_with_parameter_name ()
		{
			string value = null;

			var ex = Assert.Throws<ArgumentNullException>(() => Guard.NotNull("value", value));

			Assert.Equal ("value", ex.ParamName);
		}

		[Fact]
		public void when_null_parameter_passed_then_throws_with_parameter_name ()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => Do(null));

			Assert.Equal ("value", ex.ParamName);
		}

		[Fact]
		public void when_null_string_passed_then_throws ()
		{
			string value = null;

			var ex  = Assert.Throws<ArgumentNullException>(() => Guard.NotNullOrEmpty("value", value));

			Assert.Equal ("value", ex.ParamName);
		}

		[Fact]
		public void when_non_empty_string_passed_then_no_op ()
		{
			var value = "foo";

			Guard.NotNullOrEmpty ("value", value);
		}

		[Fact]
		public void when_non_null_string_passed_then_no_op ()
		{
			var value = "foo";

			Guard.NotNull ("value", value);
		}

		[Fact]
		public void when_empty_string_passed_then_throws ()
		{
			string value = String.Empty;

			var ex = Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty("value", value));

			Assert.Equal ("value", ex.ParamName);
		}

		[Fact]
		public void when_value_is_valid_then_no_op ()
		{
			var value = "foo";

			Guard.IsValid ("value", value, s => true, "Invalid");
		}

		[Fact]
		public void when_value_is_invalid_then_throws ()
		{
			var value = "foo";

			var ex = Assert.Throws<ArgumentException>(() => Guard.IsValid("value", value, s => false, "Invalid"));

			Assert.Equal ("value", ex.ParamName);
		}

		[Fact]
		public void when_valid_is_invalid_then_throws_with_format ()
		{
			var value = "foo";

			var ex = Assert.Throws<ArgumentException>(() => Guard.IsValid("value", value, s => false, "Invalid {0}", "bar"));

			Assert.Equal ("value", ex.ParamName);
			Assert.StartsWith("Invalid bar", ex.Message);
		}

		[Fact]
		public void when_valid_is_valid_with_format_then_no_op ()
		{
			var value = "foo";

			Guard.IsValid ("value", value, s => true, "Invalid {0}", "bar");
		}

		private void Do (string value)
		{
			Guard.NotNull ("value", value);
		}
	}
}