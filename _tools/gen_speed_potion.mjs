// 신속 물약의 옛 재료 ID를 현재 아이템 ID로 교정한다. 기존 시트/서식 유지.
import { FileBlob, SpreadsheetFile } from '@oai/artifact-tool';
import fs from 'node:fs/promises';
const path = new URL('../Assets/@Resources/Data/Excel/SpeedPotionData.xlsx', import.meta.url);
const wb = await SpreadsheetFile.importXlsx(await FileBlob.load(path.pathname.replace(/^\/(\w:)/, '$1')));
const sheet = wb.worksheets.getItem('SpeedPotionData');
const before = sheet.getRange('A1:L7').values;
sheet.getRange('K4:K7').values = [['10001'], ['10001'], ['10001'], ['10001,10002']];
wb.recalculate();
const after = sheet.getRange('A1:L7').values;
for (let r = 0; r < before.length; r++)
    for (let c = 0; c < before[r].length; c++)
        if (!(r >= 3 && c === 10) && before[r][c] !== after[r][c]) throw Error('Unrelated cell changed');
const output = await SpreadsheetFile.exportXlsx(wb);
const destination = path.pathname.replace(/^\/(\w:)/, '$1');
const scratch = new URL('../Documents/Verification-20260908/SpeedPotionData.xlsx', import.meta.url).pathname.replace(/^\/(\w:)/, '$1');
await output.save(scratch);
await fs.copyFile(scratch, destination);
console.log('SpeedPotionData: 4 recipes repaired; other cells preserved. Run ParseAllLocal.');
