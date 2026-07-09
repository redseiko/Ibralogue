using System.Collections.Generic;

namespace Ibralogue.Parser
{
	/// <summary>
	/// Shared helpers for parsing the <c>Name(arg1, arg2)</c> invocation syntax
	/// used by commands, inline functions, and preprocessor directives.
	/// </summary>
	internal static class InvocationSyntax
	{
		/// <summary>
		/// Extracts the name portion from a <c>"Name(args)"</c> string.
		/// Returns the full trimmed value when no parentheses are present.
		/// </summary>
		public static string ExtractName(string value)
		{
			int parenIndex = value.IndexOf('(');
			return parenIndex >= 0 ? value.Substring(0, parenIndex).Trim() : value.Trim();
		}

		/// <summary>
		/// Extracts the raw argument string from between the parentheses in
		/// a <c>"Name(args)"</c> value. Returns an empty string when no
		/// parentheses are present.
		/// </summary>
		public static string ExtractRawArgument(string value)
		{
			int open = value.IndexOf('(');
			int close = value.LastIndexOf(')');
			if (open >= 0 && close > open)
				return value.Substring(open + 1, close - open - 1);
			return "";
		}

		/// <summary>
		/// Splits a raw comma-separated argument string into individually trimmed parts.
		/// Returns an empty list for null, empty, or whitespace-only input.
		/// </summary>
		public static List<string> SplitArguments(string rawArgs)
		{
			List<string> result = new List<string>();
			if (string.IsNullOrEmpty(rawArgs) || rawArgs.Trim().Length == 0)
				return result;

			bool inDoubleQuotes = false;
			bool inSingleQuotes = false;
			bool isEscaped = false;
			int parenDepth = 0;
			int startIndex = 0;

			for (int i = 0; i < rawArgs.Length; i++)
			{
				char c = rawArgs[i];

				if (c == '"' && !inSingleQuotes && !isEscaped)
					inDoubleQuotes = !inDoubleQuotes;
				else if (c == '\'' && !inDoubleQuotes && !isEscaped)
					inSingleQuotes = !inSingleQuotes;
				else if (c == '(' && !inDoubleQuotes && !inSingleQuotes)
					parenDepth++;
				else if (c == ')' && !inDoubleQuotes && !inSingleQuotes)
					parenDepth--;
				else if (c == ',' && !inDoubleQuotes && !inSingleQuotes && parenDepth == 0)
				{
					string part = rawArgs.Substring(startIndex, i - startIndex).Trim();
					if (part.Length > 0)
						result.Add(StripQuotes(part));
					startIndex = i + 1;
				}

				if (c == '\\')
					isEscaped = !isEscaped;
				else
					isEscaped = false;
			}

			if (startIndex < rawArgs.Length)
			{
				string part = rawArgs.Substring(startIndex).Trim();
				if (part.Length > 0)
					result.Add(StripQuotes(part));
			}

			return result;
		}

		/// <summary>
		/// Strips surrounding double or single quotes from a string, if present.
		/// </summary>
		public static string StripQuotes(string value)
		{
			if (value.Length >= 2)
			{
				if ((value[0] == '"' && value[value.Length - 1] == '"') ||
					(value[0] == '\'' && value[value.Length - 1] == '\''))
				{
					return value.Substring(1, value.Length - 2);
				}
			}
			return value;
		}
	}
}
