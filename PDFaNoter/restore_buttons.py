import re

with open('PdfPageView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

button_stack_xml = '''
        <StackPanel x:Name="ButtonStackPanel" Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0,10,0,10" Spacing="10">
            <Button Content="&#xE710;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Insert Blank Page Below" Click="BtnInsertBlank_Click" Width="44" Height="44" FontSize="18" />
            <Button Content="&#xE74D;" FontFamily="Segoe Fluent Icons" ToolTipService.ToolTip="Delete This Page" Click="BtnDeletePage_Click" Width="44" Height="44" FontSize="18" />
        </StackPanel>'''

# Insert it before </StackPanel> of MainStackPanel
text = text.replace('    </StackPanel>\n</UserControl>', button_stack_xml + '\n    </StackPanel>\n</UserControl>')

with open('PdfPageView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)

with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

events_code = '''
        public event EventHandler InsertPageRequested;
        public event EventHandler DeletePageRequested;

        private void BtnInsertBlank_Click(object sender, RoutedEventArgs e)
        {
            InsertPageRequested?.Invoke(this, EventArgs.Empty);
        }

        private void BtnDeletePage_Click(object sender, RoutedEventArgs e)
        {
            DeletePageRequested?.Invoke(this, EventArgs.Empty);
        }
        
        public void SetLayoutMode(bool isHorizontal)
        {
            if (isHorizontal)
            {
                MainStackPanel.Orientation = Orientation.Vertical;
                ButtonStackPanel.Margin = new Thickness(0, 10, 0, 10);
                PageContainer.Margin = new Thickness(0);
            }
            else
            {
                MainStackPanel.Orientation = Orientation.Horizontal;
                ButtonStackPanel.Margin = new Thickness(20, 0, 20, 0);
                PageContainer.Margin = new Thickness(84, 0, 0, 0);
            }
        }
'''

# Insert it before the last closing brace
last_brace_idx = text.rfind('}')
if last_brace_idx != -1:
    last_brace_idx = text.rfind('}', 0, last_brace_idx)
    if last_brace_idx != -1:
        text = text[:last_brace_idx] + events_code + text[last_brace_idx:]

with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

print("Restored UI buttons and events")
