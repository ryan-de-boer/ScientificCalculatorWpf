using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using String = System.String;

namespace CalculatorWpf
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private enum eDisplay
    {
      DecimalMixed = 0,
      DecimalSci = 1,
      DecimalEng = 2,
      Hex = 3,
      Octal = 4,
      Binary = 5
    }
    private eDisplay m_display = eDisplay.DecimalMixed;

    private const int NUM_HISTORY = 20; //should be 20

    public MainWindow()
    {
    InitializeComponent();
      webView.Visibility = Visibility.Collapsed;
//      TanBar.FontSize = 16;
      TanBar.KeyDown += TanBar_KeyDown;

      this.Loaded += MainWindow_Loaded;
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
//      ConstantCombo.Items.Add("Math Constants:");
//      ConstantCombo.SelectedIndex = 0;
//      FunctionCombo.Items.Add("Math Functions:");
//      FunctionCombo.SelectedIndex = 0;


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

      /*
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "Math Functions:", Value = "Math Functions:" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "abs", Value = "abs(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "acos", Value = "acos(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "asin", Value = "asin(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "atan", Value = "atan(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "atan2", Value = "atan2(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "ceil", Value = "ceil(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "cos", Value = "cos(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "exp", Value = "exp(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "floor", Value = "floor(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "log", Value = "log(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "log10", Value = "log10(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "max", Value = "max(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "min", Value = "min(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "pow", Value = "pow(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "random", Value = "random(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "round", Value = "round(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "sin", Value = "sin(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "sqrt", Value = "sqrt(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "tan", Value = "tan(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "kmToMiles", Value = "kmToMiles(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "milesToKm", Value = "milesToKm(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "footPoundsToNm", Value = "footPoundsToNm(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "nmToFootPounds", Value = "nmToFootPounds(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "feetToMetres", Value = "feetToMetres(" });
      FunctionCombo.Items.Add(new ComboBoxItem { Label = "metresToFeet", Value = "metresToFeet(" });
      FunctionCombo.SelectedIndex = 0;
      FunctionCombo.SelectionChanged += FunctionCombo_SelectionChanged;
      */
      ReadFunctionCombo();

      HistoryCombo.SelectionChanged += HistoryCombo_SelectionChanged;


      this.Activated += MainWindow_Activated;

      LoadData();

      this.Closing += MainWindow_Closing;
    }

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

    private void MainWindow_Activated(object? sender, EventArgs e)
    {
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void FunctionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (FunctionCombo.SelectedIndex >= 1 && FunctionCombo.SelectedIndex < FunctionCombo.Items.Count)
      {
        m_nextNButtonClears = false;
        ComboBoxItem item = (ComboBoxItem)FunctionCombo.Items[FunctionCombo.SelectedIndex];
        TanBar.Text += item.Value;
        FunctionCombo.SelectedIndex = 0;
        TanBar.CaretIndex = TanBar.Text.Length;
        TanBar.Focus();
      }
    }

    private void ConstantCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (ConstantCombo.SelectedIndex >= 1 && ConstantCombo.SelectedIndex < ConstantCombo.Items.Count)
      {
        m_nextNButtonClears = false;
        ComboBoxItem item = (ComboBoxItem)ConstantCombo.Items[ConstantCombo.SelectedIndex];
        TanBar.Text += item.Value;
        ConstantCombo.SelectedIndex = 0;
        TanBar.CaretIndex = TanBar.Text.Length;
        TanBar.Focus();
      }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
      SaveData();
    }

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

    private void HistoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (HistoryCombo.SelectedIndex >= 1 && HistoryCombo.SelectedIndex < HistoryCombo.Items.Count)
      {
        m_nextNButtonClears = false;
        TanBar.Text = HistoryCombo.Items[HistoryCombo.SelectedIndex].ToString();
        TanBar.CaretIndex = TanBar.Text.Length;
        TanBar.Focus();
      }
    }

    private async void DisplayCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      Apply();
    }

    private async void WebView_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
    {
      if (e.IsSuccess)
      {
        await webView.EnsureCoreWebView2Async(); // Ensure WebView2 is fully initialized

        String fns = @"
function round_extra_sf(f){
    var s=f.toPrecision(14);
	s=s.replace(/^([\+\-0-9\\.]*[1-9\.])0+((?:e[0-9\+\-]+)?)$/g,'$1$2');
	s=s.replace(/\.((?:e[0-9\+\-]+)?)$/g,'$1');
    return s;
}

function to_sci(s,eng){
//s = s.toString();
//alert('A');
	var the_exp=0;
	var is_negative=false;
	if(s.length>0&&s.charAt(0)=='-'){
		is_negative=true;
		s=s.substring(1,s.length);
	}
//alert('B');
new RegExp('[eE]');
//alert('B.A');
//alert('BB:'+s);
s.split(',');
//alert('B.B');
	var regex_splitter=s.split(new RegExp('[eE]'));
//alert('B.1');
	if(regex_splitter.length>1){
		the_exp=parseInt(regex_splitter[1]);
		s=regex_splitter[0];
	}
//alert('B.2');
	regex_splitter=s.split(/[\.]/);
	if(regex_splitter.length>1){
		s=regex_splitter[0]+regex_splitter[1];
		the_exp+=regex_splitter[0].length-1;
//alert('B.3');
	}else{
		the_exp+=s.length-1;
//alert('B.4');
	}
//alert('C');
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
//alert('D');
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

  //  string[] digit_array = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };
  //  private string to_hex(int n)
  //  {
  //    string hex_result = "";
    
  //bool the_start = true;
  //    for (int i = 32; i > 0;)
  //    {
  //      i -= 4;
  //      var one_digit = (n >> i) & 0xf;
  //      if (!the_start || one_digit != 0)
  //      {
  //        the_start = false;
  //        hex_result += digit_array[one_digit];
  //      }
  //    }
  //    return "0x" + (hex_result == "" ? "0" : hex_result);
  //  }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {

    }

    private async void Apply()
    {
      String input = TanBar.Text;

      String result = await webView.ExecuteScriptAsync("eval(" + input + ")");
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


      //        await Execute("var int_val = parseInt(ans);");
      //        await Execute("var to_print = to_hex(int_val);");

      //        string displayScript = @"

      //    window.last_result = " + result + @";
      //    window.should_display = " + DisplayCombo.SelectedIndex + @";
      //		window.int_val=parseInt(last_result);
      //		window.float_val=parseFloat(last_result);
      //		window.to_print='';
      //		if(''+window.float_val!='NaN'&&window.should_display==1){
      //			window.to_print=to_sci(window.last_result,false);
      //		}else if(''+window.float_val!='NaN'&&window.should_display==2){
      //			window.to_print=to_sci(window.last_result,true);
      //		}else if(''+window.int_val!='NaN'&&window.should_display==3){
      //			window.to_print=to_hex(window.int_val);
      //		}else if(''+window.int_val!='NaN'&&window.should_display==4){
      //			window.to_print=to_octal(window.int_val);
      //		}else if(''+window.int_val!='NaN'&&window.should_display==5){
      //			window.to_print=to_bin(window.int_val);
      //		}else{
      //			window.to_print=round_extra_sf(window.float_val);
      //		}
      //		window.last_printed=window.to_print;
      //";
      //        await Execute(displayScript);

      //        // Wait for script execution to complete
      //        await Task.Delay(50);

      //        //String r3 = await Execute("last_printed;");
      //        //TanBar.Text = RemoveQuotes(r3);

      //        String result4 = await webView.ExecuteScriptAsync("window.last_printed");
      //        TanBar.Text = RemoveQuotes(result4);



      string fullScript = @"
    var last_result = " + result + @";
    last_result = last_result.toString();
    var should_display = " + DisplayCombo.SelectedIndex + @";
    var int_val = parseInt(last_result);
    var float_val = parseFloat(last_result);
    var last_printed = '';

    if (!isNaN(float_val) && should_display == 1) {
//alert('1'+last_printed);
        last_printed = to_sci(last_result, false);
//alert('2'+last_printed);
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

//alert(last_printed);
    last_printed;
";

      String result4 = await webView.ExecuteScriptAsync(fullScript);
      TanBar.Text = RemoveQuotes(result4);

      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();

      //works:
      //TanBar.Text = to_hex((int)Convert.ToDouble(result));

      //works:
      //String r2 = await Execute("to_hex(int_val);");
      //TanBar.Text = RemoveQuotes(r2);
      int a = 1;
      a++;

    }

    private async void TanBar_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        Apply();
      }
      else if (e.Key == Key.Escape)
      {
        //Clear
        m_nextNButtonClears = false;
        TanBar.Text = "";
        TanBar.Focus();
      }
    }

    private String RemoveQuotes(String str)
    {
      return str.Replace("\"", "");
    }

    private async Task<String> Execute(String text)
    {
      String result = await webView.ExecuteScriptAsync(text);
      return result;
    }

    private async Task<String> Eval(String text)
    {
      String result = await webView.ExecuteScriptAsync("eval(" + text + ")");
      return result;
    }

    private async void WebView_Loaded(object sender, RoutedEventArgs e)
    {
      await webView.EnsureCoreWebView2Async(); // Ensure WebView2 is initialized
      webView.Source = new Uri("https://www.google.com.au/"); // Load webpage
    }

    private async void ExecuteJavaScript(object sender, RoutedEventArgs e)
    {
      // Executes JavaScript in the WebView2 instance
      //await webView.ExecuteScriptAsync("alert('Hello from WebView2!');");
      //      String result = await webView.ExecuteScriptAsync("eval((2+1)*Math.pow(2,2))");
      //      MessageBox.Show(result);

      if (webView.Visibility == Visibility.Visible)
      {
        webView.Visibility = Visibility.Collapsed;
      }
      else
      {
        webView.Visibility = Visibility.Visible;
      }

    }

    private void ExitMenu_Click(object sender, RoutedEventArgs e)
    {
      Close();

    }

    private void PadC_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text = "";
      TanBar.Focus();
    }

    private void LeftBracket_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += "(";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void RightBracket_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += ")";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Plus_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " + ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N7_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "7";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N8_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "8";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N9_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "9";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Minus_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " - ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N4_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "4";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N5_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "5";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N6_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "6";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Times_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " * ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N1_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "1";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N2_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "2";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N3_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "3";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Divide_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " / ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();

    }

    private void EE_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += "e";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void N0_Click(object sender, RoutedEventArgs e)
    {
      if (m_nextNButtonClears)
      {
        TanBar.Text = "";
      }
      TanBar.Text += "0";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void DecimalPoint_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += ".";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private bool m_nextNButtonClears = false;

    private void Equals_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = true;
      Apply();
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Ampersand_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " & ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Pipe_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " | ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Power_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " ^ ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Not_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " ~ ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void ShiftLeft_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " << ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void ShiftRight_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " >> ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Percent_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += " % ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }

    private void Comma_Click(object sender, RoutedEventArgs e)
    {
      m_nextNButtonClears = false;
      TanBar.Text += ", ";
      TanBar.CaretIndex = TanBar.Text.Length;
      TanBar.Focus();
    }
  }
}
