param(
    [string] $Pattern
)

rg.exe --files-with-matches $Pattern `
| Select-Interactive -Preview { rg --line-number --context 10 --color always $Pattern $_ }
