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

    // 現在のウィンドウのアクティビティが取得できるか
    [TestMethod]
    public void GetCurrentWindowActivityTest()
    {
      this.disposables.Add(new LaunchPaintProcess());

      var beforeCall = DateTime.Now;
      var activity = WindowActivityInspector.GetCurrentActivity();

      Assert.IsTrue(activity.Title.Contains("ペイント"));
      Assert.IsTrue(activity.ExePath.ToLower().EndsWith("mspaint.exe"));
      Assert.IsTrue(activity.StartTime >= beforeCall);
    }

    // アクティビティを停止した時のデータ確認
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

    // アクティビティからDB保存用のデータを生成
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

    // 停止前のアクティビティからDB保存用のデータを生成しようとするとエラーが出る
    [TestMethod]
    public void TryGenerateInvalidActivityData()
    {
      var activity = WindowActivityInspector.GetCurrentActivity();

      Assert.ThrowsException<InvalidOperationException>(activity.GenerateData);
      Assert.IsFalse(activity.IsValid);
    }

    // 複数の同じアクティビティ同士のプロセスIDを比較
    [TestMethod]
    public void CompareSameExePathTest()
    {
      var activity1 = WindowActivityInspector.GetCurrentActivity();
      Task.Delay(100).Wait();
      var activity2 = WindowActivityInspector.GetCurrentActivity();

      Assert.IsTrue(activity1.IsSameWindow(activity2));
    }

    // 複数の異なるアクティビティ同士のプロセスIDを比較
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