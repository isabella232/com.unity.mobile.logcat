using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using Unity.Android.Logcat;
using UnityEditor.Android;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.TestTools;

internal class AndroidLogcatFakeMessageProvider : IAndroidLogcatMessageProvider
{
    private ADB m_ADB;
    private string m_Filter;
    private AndroidLogcat.Priority m_Priority;
    private int m_PackageID;
    private string m_LogPrintFormat;
    private string m_DeviceId;
    private Action<string> m_LogCallbackAction;
    private bool m_Started;

    private List<string> m_FakeMessages;

    internal AndroidLogcatFakeMessageProvider(ADB adb, string filter, AndroidLogcat.Priority priority, int packageID, string logPrintFormat, string deviceId, Action<string> logCallbackAction)
    {
        m_ADB = adb;
        m_Filter = filter;
        m_Priority = priority;
        m_PackageID = packageID;
        m_LogPrintFormat = logPrintFormat;
        m_DeviceId = deviceId;
        m_LogCallbackAction = logCallbackAction;

        m_FakeMessages = new List<string>();
        m_Started = false;
    }

    public void SupplyFakeMessage(string message)
    {
        m_FakeMessages.Add(message);
        if (m_Started)
            FlushFakeMessages();
    }

    private void FlushFakeMessages()
    {
        foreach (var m in m_FakeMessages)
        {
            m_LogCallbackAction(m);
        }
        m_FakeMessages.Clear();
    }

    public void Start()
    {
        m_Started = true;
        FlushFakeMessages();
    }

    public void Stop()
    {
        m_Started = false;
    }

    public void Kill()
    {
    }

    public bool HasExited
    {
        get
        {
            return false;
        }
    }
}

internal abstract class AndroidLogcatFakeDevice : IAndroidLogcatDevice
{
    internal override string Manufacturer
    {
        get { return "Undefined"; }
    }

    internal override string Model
    {
        get { return "Undefined"; }
    }


    internal override string ABI
    {
        get { return "Undefined"; }
    }

    internal override string Id
    {
        get { return "FakeDevice"; }
    }
}

internal class AndroidLogcatFakeDevice90 : AndroidLogcatFakeDevice
{
    internal override int APILevel
    {
        get { return 28; }
    }
    internal override Version OSVersion
    {
        get { return new Version(9, 0); }
    }
}

internal class AndroidLogcatFakeDevice60 : AndroidLogcatFakeDevice
{
    internal override int APILevel
    {
        get { return 23; }
    }
    internal override Version OSVersion
    {
        get { return new Version(6, 0); }
    }
}


internal class AndroidLogcatMessageProvideTests
{
    private AndroidLogcatTestRuntime m_Runtime;

    public void InitRuntime()
    {
        if (m_Runtime != null)
            throw new Exception("Runtime was not shutdown by previous test?");
        m_Runtime = new AndroidLogcatTestRuntime();
        m_Runtime.Initialize();
    }

    public void ShutdownRuntime()
    {
        if (m_Runtime == null)
            throw new Exception("Runtime was not created?");
        m_Runtime.Shutdown();
        m_Runtime = null;
    }


    [Test]
    public void RegexFilterCorrectlyFormed()
    {
        var devices = new AndroidLogcatFakeDevice[] {new AndroidLogcatFakeDevice60(), new AndroidLogcatFakeDevice90()};
        var filter = ".*abc";
        InitRuntime();

        foreach (var device in devices)
        {
            foreach (var isRegexEnabled in new[] {true, false})
            { 
                var logcat = new AndroidLogcat(m_Runtime, null, device, -1, AndroidLogcat.Priority.Verbose, ".*abc", isRegexEnabled, new string[] { });
                var message = string.Format("Failure with {0} device, regex enabled: {1}", device.GetType().FullName, isRegexEnabled.ToString());

                if (device.SupportsFilteringByRegex)
                {
                    if (isRegexEnabled)
                        Assert.IsTrue(logcat.Filter.Equals(filter), message);
                    else
                        Assert.IsTrue(logcat.Filter.Equals(Regex.Escape(filter)), message);
                }
                else
                {
                    Assert.IsTrue(logcat.Filter.Equals(filter), message);
                }
            }
        }

        ShutdownRuntime();
    }
}
