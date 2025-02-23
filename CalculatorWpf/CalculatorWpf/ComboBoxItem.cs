namespace CalculatorWpf
{
  /// <summary>
  /// Class object to store in cobo box, label value pairs.
  /// </summary>
  public class ComboBoxItem
  {
    /// <summary>
    /// The text shown in ComboBox.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// The actual value (can be any type), but usually a string.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Ensures Label is displayed.
    /// </summary>
    /// <returns>Label.</returns>
    public override string ToString() => Label;
  }
}
