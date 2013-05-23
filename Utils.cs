using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterOutlookReminder
{
  static class Utils
  {
    public static string Shorten(this string input, int length)
    {
      return input == null || input.Length < length ? input : input.Substring(0, length);
    }

  }
}
