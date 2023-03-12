param(
    [string] $Pattern
)

rg.exe --files-with-matches $Pattern `
| Select-Interactive `
    -Preview { rg --line-number --context 10 --color always $Pattern $_ } `
    -Vertical `
    -Height 75% `
    -SplitOffset 6
