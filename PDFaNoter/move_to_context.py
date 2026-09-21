import re

with open('PdfPageView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# Remove ButtonStackPanel
text = re.sub(r'<StackPanel x:Name="ButtonStackPanel".*?</StackPanel>', '', text, flags=re.DOTALL)

# Add ContextFlyout to PageContainer
flyout = r'''<Grid.ContextFlyout>
                <MenuFlyout>
                    <MenuFlyoutItem Text="Insert Blank Page Below" Icon="Add" Click="BtnInsertBlank_Click" />
                    <MenuFlyoutItem Text="Delete This Page" Icon="Delete" Click="BtnDeletePage_Click" />
                </MenuFlyout>
            </Grid.ContextFlyout>
            <Grid.Shadow>'''

text = text.replace('<Grid.Shadow>', flyout)

# Remove the outer MainStackPanel if we want, but let's just leave it and remove its Orientation if needed.
# Actually, since we remove ButtonStackPanel, MainStackPanel only has one child (PageContainer). So it's fine.

with open('PdfPageView.xaml', 'w', encoding='utf-8-sig') as f:
    f.write(text)

with open('PdfPageView.xaml.cs', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# Remove ButtonStackPanel references
text = re.sub(r'ButtonStackPanel\.Margin = new Thickness[^;]+;', '', text)

with open('PdfPageView.xaml.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Moved buttons to context menu")
