internal static class DefineSymbols
{
	public static string ExportSymbol()
	{
		string text = "false";
		string text2 = "false";
		string text3 = "false";
		string text4 = "false";
		string text5 = "false";
		return $"ASSERT = {text}, ENABLE_LOG = {text2}, DEBUG = {text3}, ENABLE_PROFILER = {text4}, TRACE = {text5}";
	}
}
