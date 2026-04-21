"""Inspect QuestData.xlsx — print sheet names and the rows we care about."""
from pathlib import Path
import openpyxl

XLSX = Path(r"C:\stopresent\Unity\RE001\Assets\@Resources\Data\Excel\QuestData.xlsx")
wb = openpyxl.load_workbook(XLSX, data_only=False)

for ws in wb.worksheets:
    print(f"\n=== Sheet: {ws.title} ({ws.max_row}r x {ws.max_column}c) ===")
    for r in range(1, ws.max_row + 1):
        row = [ws.cell(r, c).value for c in range(1, ws.max_column + 1)]
        print(f"  R{r}: {row}")
