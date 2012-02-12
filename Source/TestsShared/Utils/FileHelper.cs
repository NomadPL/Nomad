using System;

namespace TestsShared.Utils
{
	/// <summary>
	///		Helps transforming the namespace of the class into 
	/// source path to the class containting such class
	/// 
	/// </summary>
	public static class FileHelper
	{
		private const string KEY = "%asm%";

		/// <summary>
		///		Gets the path from the the type
		/// <para>
		///		Example
		///		<code>
		///			org.foo.bar.MyClass 
		///		</code>
		///		transforms into
		///		<code>
		///		org/foo/bar/MyClass.cs
		/// </code>
		/// </para>
		/// </summary>
		public static string GetClassSourceFile(Type type)
		{
			string ns = type.FullName;
			string nsPath = GetInnerPath(ns, type);
			string finalPath = nsPath + ".cs";
			return finalPath;
		}


		/// <summary>
		///		Gets the path from the namespace of the coresponding type
		/// </summary>
		public static string GetNamespaceSourceFolder(Type type)
		{
			string ns = type.Namespace;
			string nsPath = GetInnerPath(ns, type);
			return nsPath;
		}

		private static string GetInnerPath(string ns, Type type)
		{
			// from type name remove the possible asssembly name which can have '.'
			string asmName = type.Assembly.GetName().Name;
			ns = ns.Replace(asmName, KEY);
			string nsPath = ns.Replace('.', '\\');
			nsPath = nsPath.Replace(KEY, asmName);
			return nsPath;
		}
	}
}