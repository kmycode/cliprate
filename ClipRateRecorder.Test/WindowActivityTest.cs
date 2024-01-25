using ClipRateRecorder.Models.Window;
using System;
using System.Diagnostics;

namespace ClipRateRecorder.Test
{
  [TestClass]
  public class WindowActivityTest
  {
    private DisposableCollection disposables = new();

    [TestCleanup]
    public void CleanUp()
    {
      this.disposables.Dispose();
    }

    // ���݂̃E�B���h�E�̃A�N�e�B�r�e�B���擾�ł��邩
    [TestMethod]
    public void GetCurrentWindowActivityTest()
    {
      this.disposables.Add(new LaunchPaintProcess());

      var beforeCall = DateTime.Now;
      var activity = WindowActivityInspector.GetCurrentActivity();

      Assert.IsTrue(activity.Title.Contains("�y�C���g"));
      Assert.IsTrue(activity.ExePath.ToLower().EndsWith("mspaint.exe"));
      Assert.IsTrue(activity.StartTime >= beforeCall);
    }

    // �A�N�e�B�r�e�B���~�������̃f�[�^�m�F
    [TestMethod]
    public void StopCurrentWindowActivityTest()
    {
      var beforeCall = DateTime.Now;
      var activity = WindowActivityInspector.GetCurrentActivity();

      Assert.IsTrue(activity.StartTime >= beforeCall);
      Assert.AreEqual(activity.Duration, default);

      activity.Stop();
      var afterStop = DateTime.Now;

      Assert.IsTrue(activity.EndTime <= afterStop);
      Assert.IsTrue(activity.Duration.TotalSeconds > 0);
    }

    // �A�N�e�B�r�e�B����DB�ۑ��p�̃f�[�^�𐶐�
    [TestMethod]
    public void GenerateActivityData()
    {
      var activity = WindowActivityInspector.GetCurrentActivity();
      activity.Stop();

      var data = activity.GenerateData();

      Assert.IsTrue(activity.IsValid);
      Assert.IsNotNull(data);
      Assert.AreEqual(data.Title, activity.Title);
      Assert.AreEqual(data.ExePath, activity.ExePath);
      Assert.AreEqual(data.StartTime, activity.StartTime);
      Assert.AreEqual(data.EndTime, activity.EndTime);
    }

    // ��~�O�̃A�N�e�B�r�e�B����DB�ۑ��p�̃f�[�^�𐶐����悤�Ƃ���ƃG���[���o��
    [TestMethod]
    public void TryGenerateInvalidActivityData()
    {
      var activity = WindowActivityInspector.GetCurrentActivity();

      Assert.ThrowsException<InvalidOperationException>(activity.GenerateData);
      Assert.IsFalse(activity.IsValid);
    }

    // �����̓����A�N�e�B�r�e�B���m�̃v���Z�XID���r
    [TestMethod]
    public void CompareSameExePathTest()
    {
      var activity1 = WindowActivityInspector.GetCurrentActivity();
      Task.Delay(100).Wait();
      var activity2 = WindowActivityInspector.GetCurrentActivity();

      Assert.IsTrue(activity1.IsSameWindow(activity2));
    }

    // �����̈قȂ�A�N�e�B�r�e�B���m�̃v���Z�XID���r
    [TestMethod]
    public void CompareDifferentExePathTest()
    {
      var activity1 = WindowActivityInspector.GetCurrentActivity();
      this.disposables.Add(new LaunchPaintProcess());
      var activity2 = WindowActivityInspector.GetCurrentActivity();

      Assert.IsFalse(activity1.IsSameWindow(activity2));
    }
  }
}