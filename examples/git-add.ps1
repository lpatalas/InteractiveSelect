git status --short `
| ForEach-Object {
    [pscustomobject]@{
        FullText = $_
        FilePath = $_.Substring(3)
    }
} `
| Select-Interactive -Property FullText -Preview {
    git -c color.ui=always diff $_.FilePath
} `
| ForEach-Object {
    git add $_.FilePath
}
