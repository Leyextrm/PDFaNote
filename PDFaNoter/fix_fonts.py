with open('ToolState.cs', 'r', encoding='utf-8') as f:
    content = f.read()

start_idx = content.find('public static void LoadAvailableFonts()')
end_idx = content.find('public static string GetFontPath')

if start_idx != -1 and end_idx != -1:
    new_method = """public static void LoadAvailableFonts() {
            var tempFonts = new System.Collections.Generic.Dictionary<string, string>();
            try {
                using (var fontsKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts")) {
                    if (fontsKey != null) {
                        foreach (string fontName in fontsKey.GetValueNames()) {
                            string fontFile = fontsKey.GetValue(fontName).ToString();
                            if (!fontFile.Contains("\\")) {
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
                tempFonts["Malgun Gothic"] = "c:\\windows\\fonts\\malgun.ttf";
                tempFonts["Batang"] = "c:\\windows\\fonts\\batang.ttc,0";
                tempFonts["Gulim"] = "c:\\windows\\fonts\\gulim.ttc,0";
                tempFonts["Arial"] = "c:\\windows\\fonts\\arial.ttf";
                tempFonts["Times New Roman"] = "c:\\windows\\fonts\\times.ttf";
                tempFonts["Consolas"] = "c:\\windows\\fonts\\consola.ttf";
            }
            
            var sortedKeys = new System.Collections.Generic.List<string>(tempFonts.Keys);
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
        }
        
        """
    
    content = content[:start_idx] + new_method + content[end_idx:]

with open('ToolState.cs', 'w', encoding='utf-8') as f:
    f.write(content)
