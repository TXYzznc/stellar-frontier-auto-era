using System;
using System.IO;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(AutoEra.Tests.PlayMode.AutoEraOperationsUiFormNativeResultCallback))]

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// Writes the exact NUnit result of the UIForm PlayMode regression to Temp so it survives the Editor domain reload.
    /// </summary>
    public sealed class AutoEraOperationsUiFormNativeResultCallback : ITestRunCallback
    {
        public const string TestFullName = "AutoEra.Tests.PlayMode.AutoEraOperationsUiFormPlayModeTests.OperationsForms_OpenAndCloseThroughUiExtension";

        public void RunStarted(ITest testsToRun)
        {
        }

        public void RunFinished(ITestResult testResults)
        {
        }

        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
            if (!string.Equals(result.FullName, TestFullName, StringComparison.Ordinal))
            {
                return;
            }

            string resultDirectory = Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).FullName, "Temp", "AutoEraTestResults");
            Directory.CreateDirectory(resultDirectory);

            string destinationPath = Path.Combine(resultDirectory, "operations-uiform-playmode.xml");
            string temporaryPath = destinationPath + ".tmp";
            File.WriteAllText(temporaryPath, result.ToXml(true).OuterXml);
            File.Copy(temporaryPath, destinationPath, true);
            File.Delete(temporaryPath);
        }
    }
}
