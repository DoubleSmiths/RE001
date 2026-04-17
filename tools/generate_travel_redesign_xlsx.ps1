$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Escape-XmlText {
    param([string]$Value)
    if ($null -eq $Value) { return "" }
    return [System.Security.SecurityElement]::Escape($Value)
}

function Get-ColumnName {
    param([int]$Index)
    $name = ""
    while ($Index -gt 0) {
        $mod = ($Index - 1) % 26
        $name = [char](65 + $mod) + $name
        $Index = [math]::Floor(($Index - 1) / 26)
    }
    return $name
}

function New-WorksheetXml {
    param([object[][]]$Rows)

    $rowXml = New-Object System.Collections.Generic.List[string]
    for ($r = 0; $r -lt $Rows.Count; $r++) {
        $cellXml = New-Object System.Collections.Generic.List[string]
        for ($c = 0; $c -lt $Rows[$r].Count; $c++) {
            $value = $Rows[$r][$c]
            if ($null -eq $value -or $value -eq "") { continue }

            $cellRef = "{0}{1}" -f (Get-ColumnName ($c + 1)), ($r + 1)
            if ($value -is [int] -or $value -is [long] -or $value -is [double] -or $value -is [float] -or $value -is [decimal]) {
                $cellXml.Add("<c r=`"$cellRef`"><v>$value</v></c>")
            }
            else {
                $escaped = Escape-XmlText ([string]$value)
                $cellXml.Add("<c r=`"$cellRef`" t=`"inlineStr`"><is><t xml:space=`"preserve`">$escaped</t></is></c>")
            }
        }

        $rowXml.Add("<row r=`"$($r + 1)`">$($cellXml -join '')</row>")
    }

@"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheetData>
    $($rowXml -join "`n    ")
  </sheetData>
</worksheet>
"@
}

function Write-Entry {
    param(
        [System.IO.Compression.ZipArchive]$Zip,
        [string]$EntryName,
        [string]$Content
    )

    $entry = $Zip.CreateEntry($EntryName)
    $writer = New-Object System.IO.StreamWriter($entry.Open(), [System.Text.UTF8Encoding]::new($false))
    $writer.Write($Content)
    $writer.Dispose()
}

$travelAreaRows = @(
    @(
        "DataId","Name","Description","SpriteName","SceneName","RecommendPower","PreviewMonsterIds","PreviewDropItemIds","Order","DefaultBtnId","EntryBtnId","UnlockSummary",
        "Category","Depth","ParentId","RootId","NodeType","IsSelectable","IsEnterable","PanelType","Note"
    ),
    @(1000,"TownText","TravelTownCategoryDescText","","",0,"NONE","NONE",1,"BTN_TRAVEL_TOWN","","TravelTownCategoryDescText","TOWN",1,0,1000,"CATEGORY",0,0,"ROOT","root category"),
    @(1100,"PlaceNameTownAsdalText","PlaceNameTownAsdalDescText","travel_asdal_thumb","",0,"NONE","NONE",1,"BTN_MAP_ASDAL","","PlaceNameTownAsdalDescText","TOWN",2,1000,1000,"GROUP",1,0,"MIDDLE","middle group asdal"),
    @(1110,"PlaceNameTownAsdalSouthGateText","PlaceNameTownAsdalSouthGateDescText","travel_asdal_thumb","Village1",0,"NONE","NONE",1,"BTN_MAP_ASDAL_SOUTH_GATE","BTN_ENTER_ASDAL_SOUTH_GATE","PlaceNameTownAsdalSouthGateDescText","TOWN",3,1100,1000,"DETAIL",1,1,"DETAIL","detail entry asdal"),
    @(2000,"ExploreText","TravelExploreCategoryDescText","","",0,"NONE","NONE",2,"BTN_EXPLORE","","TravelExploreCategoryDescText","EXPLORE",1,0,2000,"CATEGORY",0,0,"ROOT","root category"),
    @(2100,"BreezingWindForestNameText","BreezingWindForestDescText","travel_bwf_thumb","",0,"10001,10002","10001,10002",1,"BTN_MAP_BREEZING_WIND_FOREST","","BreezingWindForestUnlockSummaryText","EXPLORE",2,2000,2000,"GROUP",1,0,"MIDDLE","middle group area"),
    @(2110,"BreezingWindForestEntranceText","BreezingWindForestEntranceDescText","travel_bwf_thumb","Stage_BreezingWindForest_Entrance",0,"10001,10002","10001,10002",1,"BTN_MAP_BREEZING_WIND_FOREST_ENTRANCE","BTN_STAGE_BREEZING_WIND_FOREST_ENTRANCE","BreezingWindForestEntranceDescText","EXPLORE",3,2100,2000,"DETAIL",1,1,"DETAIL","detail entry"),
    @(2200,"WhisperingCaveNameText","WhisperingCaveDescText","travel_cave_thumb","",120,"10003,10004","10003,20001",2,"BTN_MAP_WHISPERING_CAVE","","WhisperingCaveUnlockSummaryText","EXPLORE",2,2000,2000,"GROUP",1,0,"MIDDLE","middle group area"),
    @(2210,"WhisperingCaveEntranceText","WhisperingCaveEntranceDescText","travel_cave_thumb","Stage_WhisperingCave_Entrance",120,"10003,10004","10003,20001",1,"BTN_MAP_WHISPERING_CAVE_ENTRANCE","BTN_STAGE_WHISPERING_CAVE_ENTRANCE","WhisperingCaveEntranceDescText","EXPLORE",3,2200,2000,"DETAIL",1,1,"DETAIL","detail entry"),
    @(2300,"RuinedSanctumNameText","RuinedSanctumDescText","travel_sanctum_thumb","",250,"10005,10006","20002,30001",3,"BTN_MAP_RUINED_SANCTUM","","RuinedSanctumUnlockSummaryText","EXPLORE",2,2000,2000,"GROUP",1,0,"MIDDLE","middle group area"),
    @(2310,"RuinedSanctumEntranceText","RuinedSanctumEntranceDescText","travel_sanctum_thumb","Stage_RuinedSanctum_Entrance",250,"10005,10006","20002,30001",1,"BTN_MAP_RUINED_SANCTUM_ENTRANCE","BTN_STAGE_RUINED_SANCTUM_ENTRANCE","RuinedSanctumEntranceDescText","EXPLORE",3,2300,2000,"DETAIL",1,1,"DETAIL","detail entry")
)

$travelConditionRows = @(
    @("DataId","Order","ConditionType","Target","Value","FailMessage","Scope","Note"),
    @(2100,1,"NONE","",0,"","GROUP","starter area"),
    @(2200,1,"QUEST_CLEAR","3",1,"TravelUnlockText","GROUP","quest unlock"),
    @(2200,2,"BTN_UNLOCKED","UNLOCK_TRAVEL_BUTTON",1,"TravelUnlockText","GROUP","button unlock"),
    @(2300,1,"ITEM_REQUIRED","20002",1,"NeedRegularPermitText","GROUP","permit required"),
    @(2110,1,"NONE","",0,"","DETAIL","parent unlock"),
    @(2210,1,"NONE","",0,"","DETAIL","parent unlock"),
    @(2310,1,"NONE","",0,"","DETAIL","parent unlock")
)

$travelNotifyRows = @(
    @("DataId","Order","BtnId","NotifyType","Note"),
    @(2100,1,"BTN_MAP_BREEZING_WIND_FOREST","GROUP","group notify"),
    @(2110,1,"BTN_STAGE_BREEZING_WIND_FOREST_ENTRANCE","ENTRY","entry notify"),
    @(2200,1,"BTN_MAP_WHISPERING_CAVE","GROUP","group notify"),
    @(2210,1,"BTN_STAGE_WHISPERING_CAVE_ENTRANCE","ENTRY","entry notify"),
    @(2300,1,"BTN_MAP_RUINED_SANCTUM","GROUP","group notify"),
    @(2310,1,"BTN_STAGE_RUINED_SANCTUM_ENTRANCE","ENTRY","entry notify")
)

$refRows = @(
    @("Sheet","Rule","Description"),
    @("TravelArea","Compatibility","Keep the first 12 columns for parser compatibility and append new hierarchy columns after them."),
    @("TravelArea","Depth","1=root category, 2=middle group, 3=detail entry"),
    @("TravelArea","ParentId","Parent DataId. Root category uses 0."),
    @("TravelArea","RootId","Shared root node id."),
    @("TravelArea","NodeType","CATEGORY/GROUP/DETAIL"),
    @("TravelArea","PanelType","ROOT/MIDDLE/DETAIL ui stage mapping."),
    @("TravelAreaCondition","Scope","GROUP locks a middle card. DETAIL locks a detail entry."),
    @("TravelAreaNotify","NotifyType","GROUP marks middle card. ENTRY marks final enter button.")
)

$worksheets = @(
    @{ Name = "TravelArea"; Rows = $travelAreaRows },
    @{ Name = "TravelAreaCondition"; Rows = $travelConditionRows },
    @{ Name = "TravelAreaNotify"; Rows = $travelNotifyRows },
    @{ Name = "_Ref"; Rows = $refRows }
)

$contentTypes = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
  <Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
  <Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet2.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet3.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet4.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
</Types>
"@

$rootRels = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>
</Relationships>
"@

$workbookSheets = New-Object System.Collections.Generic.List[string]
for ($i = 0; $i -lt $worksheets.Count; $i++) {
    $sheetId = $i + 1
    $sheetName = Escape-XmlText $worksheets[$i].Name
    $workbookSheets.Add("<sheet name=`"$sheetName`" sheetId=`"$sheetId`" r:id=`"rId$sheetId`"/>")
}

$workbookXml = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    $($workbookSheets -join "`n    ")
  </sheets>
</workbook>
"@

$workbookRels = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet3.xml"/>
  <Relationship Id="rId4" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet4.xml"/>
  <Relationship Id="rId5" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>
"@

$stylesXml = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts>
  <fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills>
  <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
  <cellXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/></cellXfs>
  <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
</styleSheet>
"@

$coreXml = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:dcterms="http://purl.org/dc/terms/" xmlns:dcmitype="http://purl.org/dc/dcmitype/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <dc:creator>Codex</dc:creator>
  <cp:lastModifiedBy>Codex</cp:lastModifiedBy>
  <dcterms:created xsi:type="dcterms:W3CDTF">2026-04-17T00:00:00Z</dcterms:created>
  <dcterms:modified xsi:type="dcterms:W3CDTF">2026-04-17T00:00:00Z</dcterms:modified>
</cp:coreProperties>
"@

$appXml = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes">
  <Application>Codex</Application>
</Properties>
"@

function New-TravelWorkbook {
    param([string]$OutputPath)

    if (Test-Path $OutputPath) {
        Remove-Item $OutputPath -Force
    }

    $fileStream = [System.IO.File]::Open($OutputPath, [System.IO.FileMode]::CreateNew)
    try {
        $zip = New-Object System.IO.Compression.ZipArchive($fileStream, [System.IO.Compression.ZipArchiveMode]::Create, $false)
        try {
            Write-Entry $zip "[Content_Types].xml" $contentTypes
            Write-Entry $zip "_rels/.rels" $rootRels
            Write-Entry $zip "xl/workbook.xml" $workbookXml
            Write-Entry $zip "xl/_rels/workbook.xml.rels" $workbookRels
            Write-Entry $zip "xl/styles.xml" $stylesXml
            Write-Entry $zip "docProps/core.xml" $coreXml
            Write-Entry $zip "docProps/app.xml" $appXml

            for ($i = 0; $i -lt $worksheets.Count; $i++) {
                $sheetXml = New-WorksheetXml -Rows $worksheets[$i].Rows
                Write-Entry $zip ("xl/worksheets/sheet{0}.xml" -f ($i + 1)) $sheetXml
            }
        }
        finally {
            $zip.Dispose()
        }
    }
    finally {
        $fileStream.Dispose()
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$excelRoot = Get-ChildItem -Path (Join-Path $repoRoot "Documents") -File -Recurse -Filter "TravelData.xlsx" |
    Select-Object -First 1 -ExpandProperty DirectoryName

if ([string]::IsNullOrEmpty($excelRoot)) {
    throw "TravelData.xlsx directory not found."
}

$recommendedRoot = Join-Path $excelRoot "recommended_excels"

$targets = @(
    (Join-Path $excelRoot "TravelData_redesign.xlsx"),
    (Join-Path $recommendedRoot "TravelData.xlsx")
)

foreach ($target in $targets) {
    New-TravelWorkbook -OutputPath $target
}

Write-Output "TRAVEL_REDESIGN_XLSX_CREATED"
