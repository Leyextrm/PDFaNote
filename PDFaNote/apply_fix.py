import json
import sys

with open('ToolState.cs.recovered', 'r', encoding='utf-8') as f:
    text = f.read()

# Replace List with ObservableCollection
text = text.replace('public static List<string> SupportedFonts { get; } = new List<string>();', 
                    'public static System.Collections.ObjectModel.ObservableCollection<string> SupportedFonts { get; } = new System.Collections.ObjectModel.ObservableCollection<string>();')

# Rewrite LoadAvailableFonts safely
old_load = '''        public static void LoadAvailableFonts() {
            try {
                using (var fontsKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts")) {
                    if (fontsKey != null) {
                        foreach (string fontName in fontsKey.GetValueNames()) {
                            string fontFile = fontsKey.GetValue(fontName).ToString();
                            if (!fontFile.Contains("\\\\")) {
                                fontFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fontFile);
                            }
                            
                            if (fontFile.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) || 
                                fontFile.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase)) {
                                
                                string cleanName = fontName.Replace(" (TrueType)", "").Replace(" (OpenType)", "");
                                if (fontFile.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase)) {
                                    fontFile += ",0";
                                }
                                
                                if (!AvailableFonts.ContainsKey(cleanName)) {
                                    AvailableFonts.Add(cleanName, fontFile);
                                }
                            }
                        }
                    }
                }
            } catch { }
            
            if (AvailableFonts.Count == 0) {
                AvailableFonts["Malgun Gothic"] = "c:\\\\windows\\\\fonts\\\\malgun.ttf";
                AvailableFonts["Batang"] = "c:\\\\windows\\\\fonts\\\\batang.ttc,0";
                AvailableFonts["Gulim"] = "c:\\\\windows\\\\fonts\\\\gulim.ttc,0";
                AvailableFonts["Arial"] = "c:\\\\windows\\\\fonts\\\\arial.ttf";
                AvailableFonts["Times New Roman"] = "c:\\\\windows\\\\fonts\\\\times.ttf";
                AvailableFonts["Consolas"] = "c:\\\\windows\\\\fonts\\\\consola.ttf";
            }
            
            SupportedFonts.AddRange(AvailableFonts.Keys);
            SupportedFonts.Sort();
        }'''

new_load = '''        public static void LoadAvailableFonts() {
            var tempFonts = new Dictionary<string, string>();
            try {
                using (var fontsKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts")) {
                    if (fontsKey != null) {
                        foreach (string fontName in fontsKey.GetValueNames()) {
                            string fontFile = fontsKey.GetValue(fontName).ToString();
                            if (!fontFile.Contains("\\\\")) {
                                fontFile = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts), fontFile);
                            }
                            
                            if (fontFile.EndsWith(".ttf", System.StringComparison.OrdinalIgnoreCase) || 
                                fontFile.EndsWith(".ttc", System.StringComparison.OrdinalIgnoreCase)) {
                                
                                string cleanName = fontName.Replace(" (TrueType)", "").Replace(" (OpenType)", "");
                                if (fontFile.EndsWith(".ttc", System.StringComparison.OrdinalIgnoreCase)) {
                                    fontFile += ",0";
                                }
                                
                                if (!tempFonts.ContainsKey(cleanName)) {
                                    tempFonts.Add(cleanName, fontFile);
                                }
                            }
                        }
                    }
                }
            } catch { }
            
            if (tempFonts.Count == 0) {
                tempFonts["Malgun Gothic"] = "c:\\\\windows\\\\fonts\\\\malgun.ttf";
                tempFonts["Batang"] = "c:\\\\windows\\\\fonts\\\\batang.ttc,0";
                tempFonts["Gulim"] = "c:\\\\windows\\\\fonts\\\\gulim.ttc,0";
                tempFonts["Arial"] = "c:\\\\windows\\\\fonts\\\\arial.ttf";
                tempFonts["Times New Roman"] = "c:\\\\windows\\\\fonts\\\\times.ttf";
                tempFonts["Consolas"] = "c:\\\\windows\\\\fonts\\\\consola.ttf";
            }
            
            var sortedKeys = new List<string>(tempFonts.Keys);
            sortedKeys.Sort();
            
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dispatcher == null && App.MainWindow != null) dispatcher = App.MainWindow.DispatcherQueue;
            
            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() => {
                    foreach (var kvp in tempFonts) {
                        AvailableFonts[kvp.Key] = kvp.Value;
                    }
                    SupportedFonts.Clear();
                    foreach (var k in sortedKeys) {
                        SupportedFonts.Add(k);
                    }
                });
            }
            else
            {
                foreach (var kvp in tempFonts) {
                    AvailableFonts[kvp.Key] = kvp.Value;
                }
                SupportedFonts.Clear();
                foreach (var k in sortedKeys) {
                    SupportedFonts.Add(k);
                }
            }
        }'''

# Do manual replacement to avoid regex issues
idx = text.find('        public static void LoadAvailableFonts() {')
idx2 = text.find('        public static string GetFontPath(string fontFamily) {')
if idx != -1 and idx2 != -1:
    text = text[:idx] + new_load + '\n\n' + text[idx2:]
else:
    print("Could not find LoadAvailableFonts block")

text = text.replace('LoadAvailableFonts();', 'System.Threading.Tasks.Task.Run(() => LoadAvailableFonts());')

with open('ToolState.cs', 'w', encoding='utf-8') as f:
    f.write(text)

