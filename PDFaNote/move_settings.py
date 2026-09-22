import re
with open('PdfDocumentView.xaml', 'r', encoding='utf-8-sig') as f:
    text = f.read()

# We want to move the button with SettingsMenu
# The button starts with <Button Content="&#xE712;" and ends with </Button> (including the flyout)
# Let's extract it.
m = re.search(r'<Button Content="&#xE712;".*?</Button>', text, re.DOTALL)
if m:
    settings_btn = m.group(0)
    text = text.replace(settings_btn, '') # remove it from current location
    
    # Also there might be a stray <AppBarSeparator /> right before it
    # I'll just leave it or clean it up. Let's just insert it after BtnFitWidth.
    
    insert_loc = text.find('</Button>', text.find('BtnFitWidth'))
    if insert_loc != -1:
        insert_loc += len('</Button>')
        text = text[:insert_loc] + '\n                <AppBarSeparator />\n                ' + settings_btn + text[insert_loc:]
        with open('PdfDocumentView.xaml', 'w', encoding='utf-8-sig') as f:
            f.write(text)
        print("Moved settings button!")
    else:
        print("Could not find BtnFitWidth")
else:
    print("Could not find settings button")
