using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CalculatorWpf
{
  /// <summary>
  /// WPF Main Window for Calculator.
  /// </summary>
  public partial class MainWindow : Window
  {
    /// <summary>
    /// Number of items to store in history. Should be 20.
    /// </summary>
    private const int NUM_HISTORY = 20;

    /// <summary>
    /// Whether the next number button click clears the result bar.
    /// </summary>
    private bool m_nextNButtonClears = false;

    /// <summary>
    /// Constructor.
    /// </summary>
    public MainWindow()
    {
      InitializeComponent();
      webView.Visibility = Visibility.Collapsed;

      TanBar.KeyDown += TanBar_KeyDown;
      webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;

      DisplayCombo.Items.Add("Decimal (Mixed Notation)");
      DisplayCombo.Items.Add("Decimal (Scientific Notation)");
      DisplayCombo.Items.Add("Decimal (Engineering Notation)");
      DisplayCombo.Items.Add("Hexadecimal");
      DisplayCombo.Items.Add("Octal");
      DisplayCombo.Items.Add("Binary");
      DisplayCombo.SelectedIndex = 0;
      DisplayCombo.SelectionChanged += DisplayCombo_SelectionChanged;

      HistoryCombo.Items.Add("History:");
      HistoryCombo.SelectedIndex = 0;

      ConstantCombo.Items.Add(new ComboBoxItem { Label = "Math Constants:", Value = "Math Constants:" });

      ConstantCombo.Items.Add(new ComboBoxItem { Label = "last result", Value = "ans " });
      ConstantCombo.Items.Add(new ComboBoxItem { Label = "e", Value = "E " });
      ConstantCombo.Items.Add(new ComboBoxItem { Label = "ln(10)", Value = "LN10 " });
      ConstantCombo.Items.Add(new ComboBoxItem { Label = "ln(2)", Value = "LN2 " });
      ConstantCombo.Items.Add(new ComboBoxItem { Label = "log10(e)", Value = "LOG10E " });
      ConstantCombo.Items.Add(new ComboBoxItem { Label = "log2(e)", Value = "LOG2E " });
      ConstantCombo.Items.Add(new ComboBoxItem { Label = "π", Value = "PI " });
      ConstantCombo.Items.Add(new ComboBoxItem { Label = "sqrt(1 / 2)", Value = "SQRT1_2 " });
      ConstantCombo.Items.Add(new ComboBoxItem { Label = "sqrt(2)", Value = "SQRT2 " });
      ConstantCombo.SelectedIndex = 0;
      ConstantCombo.SelectionChanged += ConstantCombo_SelectionChanged;

      ReadFunctionCombo();

      HistoryCombo.SelectionChanged += HistoryCombo_SelectionChanged;

      this.Activated += MainWindow_Activated;

      LoadData();

      this.Closing += MainWindow_Closing;

      // Button callbacks in code so XamlViewer works.
      PadC.Click += PadC_Click;
      LeftBracket.Click += LeftBracket_Click;
      RightBracket.Click += RightBracket_Click;
      Plus.Click += Plus_Click;
      N7.Click += N7_Click;
      N8.Click += N8_Click;
      N9.Click += N9_Click;
      N4.Click += N4_Click;
      N5.Click += N5_Click;
      N6.Click += N6_Click;
      N1.Click += N1_Click;
      N2.Click += N2_Click;
      N3.Click += N3_Click;
      N0.Click += N0_Click;
      Minus.Click += Minus_Click;
      Times.Click += Times_Click;
      Divide.Click += Divide_Click;
      EE.Click += EE_Click;
      DecimalPoint.Click += DecimalPoint_Click;
      Equals.Click += Equals_Click;
      Ampersand.Click += Ampersand_Click;
      Pipe.Click += Pipe_Click;
      Power.Click += Power_Click;
      Not.Click += Not_Click;
      ShiftLeft.Click += ShiftLeft_Click;
      ShiftRight.Click += ShiftRight_Click;
      Percent.Click += Percent_Click;
      Comma.Click += Comma_Click;
    }

    /// <summary>
    /// Saves data when window closes.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="args">Not used.</param>
    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
      SaveData();
    }

    /// <summary>
    /// Saves state data to data.json.
    /// </summary>
    private void SaveData()
    {
      DataJson data = new DataJson();
      data.TanBar = TanBar.Text;
      data.History = new List<String>();
      for (int i = 0; i < HistoryCombo.Items.Count; i++)
      {
        data.History.Add(HistoryCombo.Items[i].ToString());
      }
      string jsonText = JsonConvert.SerializeObject(data);
      File.WriteAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data.json"), jsonText);
    }

    /// <summary>
    /// Loads state data from data.json.
    /// </summary>
    private void LoadData()
    {
      string jsonPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data.json");
      if (!File.Exists(jsonPath))
      {
        return;
      }
      string jsonText = File.ReadAllText(jsonPath);
      DataJson data = JsonConvert.DeserializeObject<DataJson>(jsonText);

      TanBar.Text = data.TanBar;
      HistoryCombo.Items.Clear();
      foreach (string item in data.History)
      {
        HistoryCombo.Items.Add(item);
      }
      HistoryCombo.SelectedIndex = 0;
    }

    /// <summary>
    /// Reads the customized javscript functions from Functions.txt.
    /// </summary>
    /// <returns>Functions as text.</returns>
    private string ReadFunctions()
    {
      string functionsPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Functions.txt");
      if (!File.Exists(functionsPath))
      {
        return "";
      }
      string functionsText = File.ReadAllText(functionsPath);
      return functionsText;
    }

    /// <summary>
    /// Reads the customized function combo values from FunctionsCombo.txt.
    /// </summary>
    private void ReadFunctionCombo()
    {
      string functionsComboPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "FunctionsCombo.txt");
      if (!File.Exists(functionsComboPath))
      {
        return;
      }

      string[] lines = File.ReadAllLines(functionsComboPath);
      foreach (string line in lines)
      {
        string[] tokens = line.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        FunctionCombo.Items.Add(new ComboBoxItem { Label = tokens[0], Value = tokens[1] });
      }
      FunctionCombo.SelectedIndex = 0;
      FunctionCombo.SelectionChanged += FunctionCombo_SelectionChanged;
    }

    /// <summary>
    /// When clicked on a text button (not number) adds to result bar and sets cursor to end and sets focus.
    /// </summary>
    /// <param name="text">Text to add to result bar. Specify empty string to add no text.</param>
    private void Click_Text(string text)
    {
      m_nextNButtonClears = false;
      TanBar.Text += text;
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    /// <summary>
    /// Sets focus to result bar when window activated.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void MainWindow_Activated(object? sender, EventArgs e)
    {
      Click_Text("");
    }

    /// <summary>
    /// When function combo selection changes we add the text to the result bar.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void FunctionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (FunctionCombo.SelectedIndex >= 1 && FunctionCombo.SelectedIndex < FunctionCombo.Items.Count)
      {
        m_nextNButtonClears = false;
        ComboBoxItem item = (ComboBoxItem)FunctionCombo.Items[FunctionCombo.SelectedIndex];
        TanBar.Text += item.Value;
        FunctionCombo.SelectedIndex = 0;
        Click_Text("");
      }
    }

    /// <summary>
    /// When constant combo selection changes we add the text to the result bar.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void ConstantCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (ConstantCombo.SelectedIndex >= 1 && ConstantCombo.SelectedIndex < ConstantCombo.Items.Count)
      {
        m_nextNButtonClears = false;
        ComboBoxItem item = (ComboBoxItem)ConstantCombo.Items[ConstantCombo.SelectedIndex];
        TanBar.Text += item.Value;
        ConstantCombo.SelectedIndex = 0;
        Click_Text("");
      }
    }

    /// <summary>
    /// When history combo selection changes we set the text to the result bar.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void HistoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (HistoryCombo.SelectedIndex >= 1 && HistoryCombo.SelectedIndex < HistoryCombo.Items.Count)
      {
        m_nextNButtonClears = false;
        TanBar.Text = HistoryCombo.Items[HistoryCombo.SelectedIndex].ToString();
        Click_Text("");
      }
    }

    /// <summary>
    /// When display combo selection changes we apply the change to the result bar.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private async void DisplayCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      Apply();
    }

    /// <summary>
    /// Prepares the common javascript code when webview finished initializing.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private async void WebView_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
    {
      if (e.IsSuccess)
      {
        await webView.EnsureCoreWebView2Async(); // Ensure WebView2 is fully initialized.

        String fns = @"
function round_extra_sf(f){
    var s=f.toPrecision(14);
	s=s.replace(/^([\+\-0-9\\.]*[1-9\.])0+((?:e[0-9\+\-]+)?)$/g,'$1$2');
	s=s.replace(/\.((?:e[0-9\+\-]+)?)$/g,'$1');
    return s;
}

function to_sci(s,eng){
	var the_exp=0;
	var is_negative=false;
	if(s.length>0&&s.charAt(0)=='-'){
		is_negative=true;
		s=s.substring(1,s.length);
	}
new RegExp('[eE]');
s.split(',');
	var regex_splitter=s.split(new RegExp('[eE]'));
	if(regex_splitter.length>1){
		the_exp=parseInt(regex_splitter[1]);
		s=regex_splitter[0];
	}
	regex_splitter=s.split(/[\.]/);
	if(regex_splitter.length>1){
		s=regex_splitter[0]+regex_splitter[1];
		the_exp+=regex_splitter[0].length-1;
	}else{
		the_exp+=s.length-1;
	}
	var leading_zeros=0;
	for(leading_zeros=0;leading_zeros<s.length&&s.charAt(leading_zeros)=='0';leading_zeros++){
		the_exp=the_exp-1;
	}
	s=s.substring(leading_zeros,s.length);
	var move_dec;
	if(eng){
		if(the_exp>=0){
			move_dec=(the_exp%3)+1;
		}else{
			move_dec=4-((-the_exp)%3);
			if(move_dec==4){
				move_dec=1;
			}
		}
		the_exp-=(move_dec-1);
	}else{
		move_dec=1;
	}
	var trailing_zeros='';
	for(var i=s.length;i<move_dec;i++){
		trailing_zeros+='0';
	}
	return(
		(is_negative?'-':'')+
		((s.length==0)?'0':s.substring(0,move_dec))+
		((s.length<=move_dec)?trailing_zeros:('.'+s.substring(move_dec,s.length)))+
		((s.length==0||the_exp==0)?'':('e'+the_exp))
	);
}

var digit_array=new Array('0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f');
function to_hex(n){
	var hex_result=''
	var the_start=true;
	for(var i=32;i>0;){
		i-=4;
		var one_digit=(n>>i)&0xf;
		if(!the_start||one_digit!=0){
			the_start=false;
			hex_result+=digit_array[one_digit];
		}
	}
	return '0x'+(hex_result==''?'0':hex_result);
}

function to_octal(n){
	var oct_result=''
	var the_start=true;
	for(var i=33;i>0;){
		i-=3;
		var one_digit=(n>>i)&0x7;
		if(!the_start||one_digit!=0){
			the_start=false;
			oct_result+=digit_array[one_digit];
		}
	}
	return '0'+(oct_result==''?'0':oct_result);
}

function to_bin(n){
	var bin_result=''
	var the_start=true;
	for(var i=32;i>0;){
		i-=1;
		var one_digit=(n>>i)&0x1;
		if(!the_start||one_digit!=0){
			the_start=false;
			bin_result+=digit_array[one_digit];
		}
	}
	return '0b'+(bin_result==''?'0':bin_result);
}

var E=Math.E;
var LN10=Math.LN10;
var LN2=Math.LN2;
var LOG10E=Math.LOG10E;
var LOG2E=Math.LOG2E;
var PI=Math.PI;
var SQRT1_2=Math.SQRT1_2;
var SQRT2=Math.SQRT2;
//Functions.txt
";
        fns += ReadFunctions();
        Execute(fns);
      }
    }

    /// <summary>
    /// Executes the contents of the result bar.
    /// </summary>
    private async void Apply()
    {
      String input = TanBar.Text;

      String result = await webView.ExecuteScriptAsync("eval(" + input + ")");
      if (result == "null")
      {
        MessageBox.Show("Invalid input: " + input, "Error");
        return;
      }
      TanBar.Text = result;
      TanBar.CaretIndex = TanBar.Text.Length;

      if (HistoryCombo.Items.Count-1 >= NUM_HISTORY)
      {
        HistoryCombo.Items.RemoveAt(HistoryCombo.Items.Count-1);
      }
      List<int> removeThese = new List<int>();
      for (int i = HistoryCombo.Items.Count-1; i>0;--i)
      {
        if (HistoryCombo.Items[i].ToString() == input)
        {
          removeThese.Add(i);
        }
      }
      foreach (int indexRemove in removeThese)
      {
        HistoryCombo.Items.RemoveAt(indexRemove);
      }
      HistoryCombo.Items.Insert(1, input);
      HistoryCombo.SelectedIndex = 0;

      await Execute("var ans=" + result);

      string fullScript = @"
    var last_result = " + result + @";
    last_result = last_result.toString();
    var should_display = " + DisplayCombo.SelectedIndex + @";
    var int_val = parseInt(last_result);
    var float_val = parseFloat(last_result);
    var last_printed = '';

    if (!isNaN(float_val) && should_display == 1) {
        last_printed = to_sci(last_result, false);
    } else if (!isNaN(float_val) && should_display == 2) {
        last_printed = to_sci(last_result, true);
    } else if (!isNaN(int_val) && should_display == 3) {
        last_printed = to_hex(int_val);
    } else if (!isNaN(int_val) && should_display == 4) {
        last_printed = to_octal(int_val);
    } else if (!isNaN(int_val) && should_display == 5) {
        last_printed = to_bin(int_val);
    } else {
        last_printed = round_extra_sf(float_val);
    }

    last_printed;
";

      String fResult = await webView.ExecuteScriptAsync(fullScript);
      TanBar.Text = RemoveQuotes(fResult);

      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    /// <summary>
    /// On Keydown of the result bar, when hit Enter we apply the result, on ESC we clear the display.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private async void TanBar_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        Apply();
      }
      else if (e.Key == Key.Escape)
      {
        // Clear.
        m_nextNButtonClears = false;
        TanBar.Text = "";
        TanBar.Focus();
      }
    }

    /// <summary>
    /// Removes the surrounding quotes from a javascript result.
    /// </summary>
    /// <param name="str">String to remote quotes from.</param>
    /// <returns>String with quotes removed.</returns>
    private String RemoveQuotes(String str)
    {
      return str.Replace("\"", "");
    }

    /// <summary>
    /// Executes a javascript block.
    /// </summary>
    /// <param name="text">Text to execute.</param>
    /// <returns>Result of call.</returns>
    private async Task<String> Execute(String text)
    {
      String result = await webView.ExecuteScriptAsync(text);
      return result;
    }

    /// <summary>
    /// Evals given javascript text.
    /// </summary>
    /// <param name="text">Text to eval.</param>
    /// <returns>Result of eval.</returns>
    private async Task<String> Eval(String text)
    {
      String result = await webView.ExecuteScriptAsync("eval(" + text + ")");
      return result;
    }

    /// <summary>
    /// Loads webpage when WebView is loaded.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private async void WebView_Loaded(object sender, RoutedEventArgs e)
    {
      await webView.EnsureCoreWebView2Async(); // Ensure WebView2 is initialized.
      webView.Source = new Uri("https://www.google.com.au/"); // Load webpage.
    }

    /// <summary>
    /// Toggles the visibility of the WebView browser.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private async void ShowBrowserMenu_Click(object sender, RoutedEventArgs e)
    {
      if (webView.Visibility == Visibility.Visible)
      {
        webView.Visibility = Visibility.Collapsed;
      }
      else
      {
        webView.Visibility = Visibility.Visible;
      }
    }

    /// <summary>
    /// Closes window when exit menu selected.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void ExitMenu_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    /// <summary>
    /// Triggered when click the C button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void PadC_Click(object sender, RoutedEventArgs e)
    {
      TanBar.Text = "";
      Click_Text("");
    }

    /// <summary>
    /// Triggered when click the ( button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void LeftBracket_Click(object sender, RoutedEventArgs e)
    {
      Click_Text("(");
    }

    /// <summary>
    /// Triggered when click the ) button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void RightBracket_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(")");
    }

    /// <summary>
    /// Triggered when click the + button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Plus_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" + ");
    }

    /// <summary>
    /// Handles clicking on a number. Optionally clears, sets text, caret to end and focus.
    /// </summary>
    /// <param name="n">Number on the button.</param>
    private void Click_N(int n)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += n;
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    /// <summary>
    /// Triggered when click the 7 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N7_Click(object sender, RoutedEventArgs e)
    {
      Click_N(7);
    }

    /// <summary>
    /// Triggered when click the 8 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N8_Click(object sender, RoutedEventArgs e)
    {
      Click_N(8);
    }

    /// <summary>
    /// Triggered when click the 9 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N9_Click(object sender, RoutedEventArgs e)
    {
      Click_N(9);
    }

    /// <summary>
    /// Triggered when click the - button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Minus_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" - ");
    }

    /// <summary>
    /// Triggered when click the 4 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N4_Click(object sender, RoutedEventArgs e)
    {
      Click_N(4);
    }

    /// <summary>
    /// Triggered when click the 5 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N5_Click(object sender, RoutedEventArgs e)
    {
      Click_N(5);
    }

    /// <summary>
    /// Triggered when click the 6 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N6_Click(object sender, RoutedEventArgs e)
    {
      Click_N(6);
    }

    /// <summary>
    /// Triggered when click the * button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Times_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" * ");
    }

    /// <summary>
    /// Triggered when click the 1 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N1_Click(object sender, RoutedEventArgs e)
    {
      Click_N(1);
    }

    /// <summary>
    /// Triggered when click the 2 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N2_Click(object sender, RoutedEventArgs e)
    {
      Click_N(2);
    }

    /// <summary>
    /// Triggered when click the 3 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N3_Click(object sender, RoutedEventArgs e)
    {
      Click_N(3);
    }

    /// <summary>
    /// Triggered when click the / button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Divide_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" / ");
    }

    /// <summary>
    /// Triggered when click the EE button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void EE_Click(object sender, RoutedEventArgs e)
    {
      Click_Text("e");
    }

    /// <summary>
    /// Triggered when click the 0 button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void N0_Click(object sender, RoutedEventArgs e)
    {
      Click_N(0);
    }

    /// <summary>
    /// Triggered when click the . button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void DecimalPoint_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(".");
    }

    /// <summary>
    /// Triggered when click the = button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Equals_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = true;
      Apply();
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    /// <summary>
    /// Triggered when click the & button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Ampersand_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" & ");
    }

    /// <summary>
    /// Triggered when click the | button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Pipe_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" | ");
    }

    /// <summary>
    /// Triggered when click the ^ button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Power_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" ^ ");
    }

    /// <summary>
    /// Triggered when click the ~ button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Not_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" ~ ");
    }

    /// <summary>
    /// Triggered when click the << button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void ShiftLeft_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" << ");
    }

    /// <summary>
    /// Triggered when click the >> button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void ShiftRight_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" >> ");
    }

    /// <summary>
    /// Triggered when click the % button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Percent_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(" % ");
    }

    /// <summary>
    /// Triggered when click the , button.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void Comma_Click(object sender, RoutedEventArgs e)
    {
      Click_Text(", ");
    }
  }
}
