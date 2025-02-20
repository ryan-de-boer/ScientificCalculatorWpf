using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorWpf
{
  public class ComboBoxItem
  {
    public string Label { get; set; }  // The text shown in ComboBox
    public object Value { get; set; }  // The actual value (can be any type)

    public override string ToString() => Label; // Ensures Label is displayed
  }
}
