using Microsoft.VisualStudio.TestTools.UnitTesting;
using TARC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TARC.Compiler.Tests
{
	[TestClass()]
	public class MarkupCompilerTests
	{
		[TestMethod()]
		public void CompileArchiveTest()
		{
			new MarkupCompiler().CompileArchive(@"B:\Text");
		}
	}
}