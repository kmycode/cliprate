using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRate
{
  internal readonly struct WindowTitle
  {
    public string Title { get; }

    public WindowTitle(string? title)
    {
      this.Title = title ?? string.Empty;
    }

    public bool IsPauseWindow
      => this.Title == "ClipRate" ||
      (this.Title.EndsWith("pixiv - Google Chrome") && !this.Title.Contains("ウマ娘"));

    public bool IsWorkingWindow
      => string.IsNullOrWhiteSpace(this.Title) ||
      this.Title == "CLIP STUDIO PAINT" ||
      this.Title.EndsWith("- CLIP STUDIO PAINT") ||
      this.Title == "サブツール詳細" ||
      this.Title == "CLIP STUDIO" ||
      this.Title == "Eagle" ||
      this.Title.EndsWith("- OneNote") ||
      this.Title.EndsWith("- メモ帳") ||
      this.Title.EndsWith("DesignDoll") ||
      this.Title.Contains("Adobe Acrobat Reader");
  }
}
