using System.Collections.Generic;

namespace CalculatorWpf
{
  /// <summary>
  /// Stores state information to save in data.json.
  /// </summary>
  internal class DataJson
  {
    /// <summary>
    /// Tan bar is the contents of the result bar as text.
    /// </summary>
    public string TanBar = "";

    /// <summary>
    /// History is the list of strings in the History dropdown (including first label).
    /// </summary>
    public List<string> History = new List<string>();
  }
}
