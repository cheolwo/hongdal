[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$scriptDirectory = if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    Join-Path (Get-Location).Path "eng"
}
else {
    $PSScriptRoot
}
$repositoryRoot = Split-Path -Parent $scriptDirectory
$promptRoot = Join-Path `
    $repositoryRoot `
    "docs\Content\AppContextImagePrompts"
$packRoot = Join-Path $promptRoot "packs"
$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)

$variants = @(
    [pscustomobject]@{
        Code = "wide-context"
        Title = "넓은 생활·업무 환경"
        AspectRatio = "16:9"
        Addition = "주요 행동이 일어나는 주변 생활권과 작업 환경을 넓게 함께 보여 주되, 간판이나 표지판, 화면 글자 없이 사람과 사물의 관계만으로 맥락이 이해되게 구성한다."
    },
    [pscustomobject]@{
        Code = "hands-materials"
        Title = "손과 재료의 세부"
        AspectRatio = "4:3"
        Addition = "손, 농수산물, 포장 재료와 업무 도구의 질감을 가까이 보여 주고, 얼굴이나 문서보다 실제 행동과 재료가 중심이 되도록 절제된 구도로 표현한다."
    },
    [pscustomobject]@{
        Code = "role-handoff"
        Title = "역할 간 안전한 인계"
        AspectRatio = "4:3"
        Addition = "서로 다른 역할의 두세 사람이 물품이나 빈 상자를 안전하게 인계하며 함께 확인하는 순간을 보여 주고, 자동 확정이나 위계가 아니라 상호 확인의 분위기를 만든다."
    },
    [pscustomobject]@{
        Code = "mobile-focus"
        Title = "모바일 세로형 중심 장면"
        AspectRatio = "3:4"
        Addition = "모바일 세로 crop에 맞춰 핵심 인물과 사물을 중앙에 두고 상하 여백을 확보한다. 휴대기기 화면, 버튼, 숫자, 글자는 보이지 않게 하고 행동 자체로 의미가 전달되게 한다."
    },
    [pscustomobject]@{
        Code = "quiet-state"
        Title = "차분한 대기 상태"
        AspectRatio = "3:4"
        Addition = "사람이 많지 않은 차분한 공간에 다음 행동을 기다리는 도구와 물품을 배치해 빈 상태나 대기 상태를 표현하고, 경고 문구나 텍스트 없이 다시 시작할 수 있는 안정감을 준다."
    },
    [pscustomobject]@{
        Code = "exception-review"
        Title = "예외 상황 공동 검토"
        AspectRatio = "4:3"
        Addition = "참여자들이 문제가 생긴 물품이나 절차를 함께 살펴보고 선택 가능한 대안을 비교한다. 붉은 경고문이나 책임 추궁 대신 색과 거리, 사물 배치로 신중한 검토를 표현한다."
    },
    [pscustomobject]@{
        Code = "time-sequence"
        Title = "시간 흐름이 보이는 과정"
        AspectRatio = "16:9"
        Addition = "하나의 넓은 화면 안에서 준비, 진행, 인계의 시간 흐름이 자연스러운 공간 변화로 읽히게 한다. 화살표, 단계 번호, 캡션 없이 사람의 움직임과 물품 위치로 순서를 표현한다."
    },
    [pscustomobject]@{
        Code = "inclusive-access"
        Title = "누구나 참여 가능한 장면"
        AspectRatio = "4:3"
        Addition = "다양한 연령과 이동 방식의 사람이 동등한 눈높이에서 참여하도록 통로와 작업 높이, 거리감을 배려해 구성하고, 특정 외모나 배경을 역할 능력과 연결하지 않는다."
    },
    [pscustomobject]@{
        Code = "evidence-boundary"
        Title = "실물과 정보 표현의 분리"
        AspectRatio = "16:9"
        Addition = "실제 농수산물이나 업무 물품은 자연스럽게 묘사하고, 정보는 글자 없는 단순한 색면과 기하 도형으로만 암시한다. 증명서, 가격표, 계약서나 공공기관 보증처럼 보이는 요소는 넣지 않는다."
    }
)

$catalog = Get-Content `
    -LiteralPath (Join-Path $promptRoot "catalog.v1.json") `
    -Raw `
    -Encoding UTF8 | ConvertFrom-Json
$expansionEntries = @()

foreach ($entry in $catalog.packs) {
    $pilotPath = Join-Path $promptRoot $entry.path
    $pilot = Get-Content `
        -LiteralPath $pilotPath `
        -Raw `
        -Encoding UTF8 | ConvertFrom-Json
    $scenes = @()
    $sequence = 6

    foreach ($baseScene in $pilot.scenes) {
        foreach ($variant in $variants) {
            $scenes += [ordered]@{
                sequence = $sequence
                code = "$($baseScene.code)-$($variant.Code)"
                titleKo = "$($baseScene.titleKo) · $($variant.Title)"
                promptKo = "$($baseScene.promptKo) $($variant.Addition) 이미지 안에는 제목, 캡션, 글자, 숫자, 로고, 화면 인터페이스를 그리지 않는다."
                aspectRatio = $variant.AspectRatio
                resolution = "1K"
                routeRefs = @($baseScene.routeRefs)
            }
            $sequence++
        }
    }

    if ($scenes.Count -ne 45 -or $sequence -ne 51) {
        throw "Expansion scene count is invalid for $($pilot.packId)."
    }

    $expansion = [ordered]@{
        schemaVersion = 1
        packId = $pilot.packId
        status = "ApprovedForBatch"
        promptVersion = 2
        model = $pilot.model
        expectedSceneCount = 45
        sceneNumberStart = 6
        basePromptKo = "$($pilot.basePromptKo) 이미지 자체는 순수한 시각 자산으로 만들고 제목판, 설명문, 표, 앱 화면, 대시보드 UI를 합성하지 않는다."
        contextChecklist = @($pilot.contextChecklist) + @(
            "이미지에 읽을 수 있는 글자·숫자·가상 UI가 없는지 확인한다."
        )
        avoidExpressions = @($pilot.avoidExpressions) + @(
            "제목, 캡션, 단계 번호, 가상 앱 화면, 대시보드, 표와 읽을 수 있는 문자"
        )
        scenes = $scenes
    }

    $fileName = "$($pilot.packId).expansion.v2.json"
    $outputPath = Join-Path $packRoot $fileName
    [System.IO.File]::WriteAllText(
        $outputPath,
        ($expansion | ConvertTo-Json -Depth 12),
        $utf8WithoutBom)
    $expansionEntries += [ordered]@{
        packId = $pilot.packId
        targetApp = $entry.targetApp
        path = "packs/$fileName"
        sceneNumberStart = 6
        sceneCount = 45
    }
}

$expansionCatalog = [ordered]@{
    schemaVersion = 1
    provider = "GoogleGemini"
    model = "gemini-3.1-flash-lite-image"
    resolution = "1K"
    packCount = 13
    scenesPerPack = 45
    totalScenes = 585
    sceneNumberStart = 6
    status = "ApprovedForBatch"
    packs = $expansionEntries
}
[System.IO.File]::WriteAllText(
    (Join-Path $promptRoot "catalog.expansion.v2.json"),
    ($expansionCatalog | ConvertTo-Json -Depth 8),
    $utf8WithoutBom)

Write-Output "Generated 13 expansion packs with 585 scenes."
