# DataTransformer CSV → XLSX 마이그레이션 가이드

---

## 1. 전체 변경 범위 요약

현재 `DataTransformer.cs`는 `ReadCsv()`가 `List<List<string>>`을 반환하고,
각 `Parse*` 메서드가 그 결과를 소비하는 구조입니다.
XLSX로 교체해도 이 인터페이스는 그대로 유지되므로 수정 범위가 최소화됩니다.

| 위치 | 변경 내용 |
|------|-----------|
| `ReadCsv()` 메서드 전체 | `ReadXlsx()`로 교체 |
| `ParsePlayerStatusData()` 45번 줄 | `.csv` → `.xlsx` |
| `ParseScriptData()` 90번 줄 | `.csv` → `.xlsx` |
| `Parse*` 내부 필드 매핑 로직 | **변경 없음** |
| `ConvertValue`, `SplitLines`, `WriteJson` | **변경 없음** |

---

## 2. DLL 세팅 (ExcelDataReader)

### 2-1. 라이브러리 선택 이유

| 라이브러리 | 라이선스 | 특이사항 |
|-----------|----------|----------|
| **ExcelDataReader** | MIT | 가볍고 의존성 2개, Unity Editor 궁합 좋음 → **채택** |
| EPPlus 4.x | LGPL | 기능 많음, v5부터 상업 라이선스 |
| NPOI | Apache 2.0 | DLL 여러 개 필요, 무거움 |

### 2-2. NuGet에서 DLL 다운로드

아래 두 패키지가 필요합니다.

| 패키지 | NuGet 주소 |
|--------|-----------|
| ExcelDataReader | https://www.nuget.org/packages/ExcelDataReader |
| ExcelDataReader.DataSet | https://www.nuget.org/packages/ExcelDataReader.DataSet |

**다운로드 방법:**

1. 위 NuGet 페이지에서 우측 `Download package` 클릭 → `.nupkg` 파일 다운로드
2. 파일 확장자를 `.nupkg` → `.zip` 으로 변경
3. 압축 해제 후 `lib/netstandard2.0/` 폴더 안의 `.dll` 파일 꺼내기

필요한 DLL 목록:
```
ExcelDataReader.dll
ExcelDataReader.DataSet.dll
System.Text.Encoding.CodePages.dll   ← ExcelDataReader 의존성
```

> `System.Text.Encoding.CodePages.dll`은 ExcelDataReader .nupkg 압축 해제 시
> `lib/netstandard2.0/` 안에 함께 있거나, NuGet에서 별도로 받아야 합니다.

### 2-3. Unity 프로젝트에 배치

```
Assets/
└── Plugins/
    └── Editor/          ← 이 폴더에 넣어야 Editor 전용으로만 빌드됨
        ├── ExcelDataReader.dll
        ├── ExcelDataReader.DataSet.dll
        └── System.Text.Encoding.CodePages.dll
```

> `Plugins/Editor/` 에 배치하면 런타임 빌드에는 포함되지 않습니다.
> DataTransformer는 EditorWindow이므로 Editor 전용으로 충분합니다.

### 2-4. .dll.meta 파일 설정 확인

Unity가 DLL을 인식한 뒤, 각 `.dll`을 클릭하면 Inspector에서 플랫폼 설정이 보입니다.

- **Include Platforms**: `Editor` 만 체크
- **Exclude Platforms**: 나머지 전부

`Plugins/Editor/` 에 넣으면 Unity가 자동으로 Editor 전용으로 설정하지만,
혹시 빌드 오류가 나면 위 설정을 수동으로 확인합니다.

---

## 3. DataTransformer.cs 코드 수정

### 3-1. using 추가

파일 상단에 아래 두 줄 추가:

```csharp
using ExcelDataReader;
using System.Data;
```

### 3-2. ReadCsv() → ReadXlsx()로 교체

기존 `ReadCsv()` 메서드를 아래로 **통째로 교체**합니다.

```csharp
/// <summary>xlsx 파일을 읽어 List<List<string>> 형태로 반환</summary>
/// <param name="filePath">xlsx 파일 절대 경로</param>
/// <param name="sheetIndex">읽을 시트 인덱스 (기본값 0 = 첫 번째 시트)</param>
static List<List<string>> ReadXlsx(string filePath, int sheetIndex = 0)
{
    // Windows에서 코드 페이지 인코딩 등록 (ExcelDataReader 필수)
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

    var rows = new List<List<string>>();

    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    using (var reader = ExcelReaderFactory.CreateReader(stream))
    {
        DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = false  // 헤더를 코드에서 직접 건너뜀 (기존 CSV 방식 유지)
            }
        });

        if (sheetIndex >= dataSet.Tables.Count)
        {
            Debug.LogError($"[DataTransformer] 시트 인덱스 {sheetIndex} 가 존재하지 않습니다.");
            return rows;
        }

        DataTable table = dataSet.Tables[sheetIndex];

        foreach (DataRow dataRow in table.Rows)
        {
            var row = new List<string>();
            foreach (var cell in dataRow.ItemArray)
                row.Add(cell?.ToString() ?? "");
            rows.Add(row);
        }
    }

    return rows;
}
```

### 3-3. 각 Parse 메서드 호출부 수정

**ParsePlayerStatusData()**
```csharp
// Before
var rows = ReadCsv($"{Application.dataPath}{EXCEL_PATH}/PlayerStatusData.csv");

// After
var rows = ReadXlsx($"{Application.dataPath}{EXCEL_PATH}/PlayerStatusData.xlsx");
```

**ParseScriptData()**
```csharp
// Before
var rows = ReadCsv($"{Application.dataPath}{EXCEL_PATH}/ScriptData.csv");

// After
var rows = ReadXlsx($"{Application.dataPath}{EXCEL_PATH}/ScriptData.xlsx");
```

### 3-4. SplitLines 유지 여부

xlsx 셀 내 줄바꿈은 `\n`으로 들어오므로 기존 `SplitLines()` 그대로 사용 가능합니다.
변경 없음.

---

## 4. xlsx 파일 작성 규칙

기존 CSV와 동일한 구조를 xlsx 첫 번째 시트에 작성합니다.

### PlayerStatusData.xlsx

| 행 | 내용 |
|----|------|
| Row 0 | 영문 필드명 (헤더) |
| Row 1 | 타입 |
| Row 2 | 한글 설명 |
| Row 3+ | 실제 데이터 |

### ScriptData.xlsx

| 행 | 내용 |
|----|------|
| Row 0 | 영문 필드명 (헤더) |
| Row 1+ | 실제 데이터 |

> **셀 내 줄바꿈**: Excel에서 `Alt + Enter`로 입력.
> ExcelDataReader가 `\n`으로 읽어오므로 `SplitLines()`에서 정상 파싱됩니다.

---

## 5. 작업 순서 체크리스트

- [ ] NuGet에서 `ExcelDataReader`, `ExcelDataReader.DataSet` .nupkg 다운로드
- [ ] `.nupkg` → `.zip` 변경 후 압축 해제, `netstandard2.0/` 폴더에서 DLL 추출
- [ ] `Assets/Plugins/Editor/` 폴더에 DLL 3개 배치
- [ ] Unity Editor에서 DLL Inspector → Platforms: Editor only 확인
- [ ] `DataTransformer.cs` 상단에 `using` 2개 추가
- [ ] `ReadCsv()` → `ReadXlsx()`로 교체
- [ ] 각 Parse 메서드 호출부 파일 확장자 변경
- [ ] 기존 `.csv` 파일을 `.xlsx`로 변환하여 동일 경로에 배치
- [ ] Unity Tools/ParseExcel 실행 후 JSON 정상 생성 확인

---

*작성일: 2026-03-23*
