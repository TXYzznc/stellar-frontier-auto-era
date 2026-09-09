using System.IO;
using System.Text;
using GameFramework.Editor.DataTableTools;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class DataTableProcessorEditModeTests
    {
        [Test]
        public void GenerateCodeFile_PassesExplicitUserDataToCodeGenerator()
        {
            string directory = Path.Combine(Path.GetTempPath(), "AutoEraDataTableProcessorTests", Path.GetRandomFileName());
            Directory.CreateDirectory(directory);
            string dataTablePath = Path.Combine(directory, "TestTable.txt");
            string templatePath = Path.Combine(directory, "TestTemplate.txt");
            string outputPath = Path.Combine(directory, "Generated.cs");
            object expectedUserData = new object();
            object receivedUserData = null;

            try
            {
                File.WriteAllText(dataTablePath, "Id\tName\nint\tstring\n1\tTest", Encoding.UTF8);
                File.WriteAllText(templatePath, "// generated", Encoding.UTF8);
                DataTableProcessor processor = new DataTableProcessor(dataTablePath, Encoding.UTF8, 0, 1, null, null, 2, 0);
                Assert.That(processor.SetCodeTemplate(templatePath, Encoding.UTF8), Is.True);
                processor.SetCodeGenerator((_, __, userData) => receivedUserData = userData);

                Assert.That(processor.GenerateCodeFile(outputPath, Encoding.UTF8, expectedUserData), Is.True);
                Assert.That(receivedUserData, Is.SameAs(expectedUserData));
                Assert.That(receivedUserData, Is.Not.EqualTo(outputPath));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
