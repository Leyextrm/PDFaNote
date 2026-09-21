import json
import os

transcript_path = r'C:\Users\leyex\.gemini\antigravity\brain\b369d447-cd28-4f3c-8cd8-515ccf169610\.system_generated\logs\transcript_full.jsonl'
for line in open(transcript_path, 'r', encoding='utf-8'):
    try:
        step = json.loads(line)
        if step.get('type') == 'TOOL_RESPONSE' and 'tool_responses' in step:
            for tr in step['tool_responses']:
                out = tr.get('output', '')
                if 'public sealed partial class PdfPageView : UserControl' in out:
                    print("Found PdfPageView!")
                    print(f"Content length: {len(out)}")
                    if 'Content truncated' in out:
                        print("Truncated!")
                    with open('PdfPageView.bak', 'w', encoding='utf-8') as f:
                        f.write(out)
    except:
        pass
