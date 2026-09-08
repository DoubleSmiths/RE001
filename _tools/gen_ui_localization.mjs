// 제작·강화 표시에서 사용하는 누락 키만 보충한다. 기존 행/번역은 보존한다.
import fs from 'node:fs/promises';
import { FileBlob, SpreadsheetFile } from '@oai/artifact-tool';
const path = new URL('../Assets/@Resources/Data/Excel/LocalizationData.xlsx', import.meta.url).pathname.replace(/^\/(\w:)/, '$1');
const out = new URL('../Documents/Verification-20260908/', import.meta.url).pathname.replace(/^\/(\w:)/, '$1');
const wb = await SpreadsheetFile.importXlsx(await FileBlob.load(path));
const sheet = wb.worksheets.items[0];
const values = sheet.getRange('A1:C2000').values;
console.log((await wb.inspect({kind:'table',range:sheet.name+'!A1:C6',tableMaxRows:6,tableMaxCols:3})).ndjson);
const preview = await wb.render({sheetName:sheet.name,range:'A1:C6',scale:1,format:'png'});
await fs.writeFile(out+'localization-before.png',new Uint8Array(await preview.arrayBuffer()));
if (!process.argv.includes('--apply')) process.exit(0);
const rows = [
    ['EquipActionText','장착','EQUIP'],
    ['EquippedText','장착중','EQUIPPED'],
    ['EquipmentStatsText','능력치','STATS'],
    ['ForgeRestoreTitleText','장비 복구','REPAIR EQUIPMENT'],
    ['ForgeRestoreActionText','복구','REPAIR'],
    ['SkipEnhanceAnimationText','영상\n건너뛰기','SKIP\nANIMATION'],
    ['EnhanceDurabilityRiskText','실패 시 내구도 감소량','DURABILITY LOSS ON FAILURE'],
    ['EquipAlreadyOwnedText','이미 보유','OWNED'],
    ['EquipCannotCraftText','제작 불가','UNAVAILABLE'],
    ['RequiredLevelFormat','Lv.{0} 필요','Requires Lv.{0}'],
    ['RequiredGoldFormat','골드 {0:N0} 필요','Requires {0:N0} G'],
    ['RequiredItemFormat','{0} {1}개 필요','Requires {1} {0}'],
    ['MissingItemFormat','{0} 없음','No {0}'],
    ['EquipSlotWeaponText','무기','WEAPON'],
    ['EquipSlotHelmetText','투구','HELMET'],
    ['EquipSlotArmorText','갑옷','ARMOR'],
    ['EquipSlotGlovesText','장갑','GLOVES'],
    ['EquipSlotLegsText','신발','BOOTS'],
    ['EquipSlotAccessoryText','장신구','ACCESSORY'],
    ['PlayerLevelLabelText','레벨','LEVEL'],
    ['GoldLabelText','골드','GOLD'],
    ['DurabilityFullText','내구도 가득','FULL DURABILITY'],
    ['InsufficientMaterialsText','재료 부족','NEED MATERIALS'],
    ['EnhanceMaxText','최대 강화','MAX ENHANCE'],
    ['DurabilityZeroText','내구도 0','DURABILITY 0'],
    ['SelectScrollText','주문서 선택','SELECT SCROLL'],
    ['SelectScrollFirstText','강화에 쓸 주문서를 먼저 선택하세요','Select an enhancement scroll first.'],
    ['EnhanceGoldShortFormat','골드가 부족합니다\n강화 비용 {0:N0} (보유 {1:N0})','Not enough gold\nCost {0:N0} G (have {1:N0})'],
];
let end = values.findLastIndex(r=>r.some(v=>v!==null&&v!=='')) + 1;
const keys = new Set(values.map(r=>r[0]));
const added = rows.filter(r=>!keys.has(r[0]));
if (added.length) sheet.getRange(`A${end+1}:C${end+added.length}`).values=added;
wb.recalculate();
for(let r=0;r<end;r++) if(JSON.stringify(sheet.getRange(`A${r+1}:C${r+1}`).values[0])!==JSON.stringify(values[r])) throw Error('Existing localization changed');
const xlsx=await SpreadsheetFile.exportXlsx(wb);
await xlsx.save(out+'LocalizationData.xlsx');
await fs.copyFile(out+'LocalizationData.xlsx',path);
console.log(`Added ${added.length} localization rows; existing ${end} rows preserved. Run ParseAllLocal and GenerateLocalizeKeys.`);
