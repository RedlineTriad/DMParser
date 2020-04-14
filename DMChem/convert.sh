#!/usr/bin/fish

mkdir -p $argv[1]
set source (cat $argv[1].source)
set basePath $source[1]

for val in $source[2..-1]
    if string match -qr '#.*' $val
        continue
    end
    set output (echo "$argv[1]/$val" | string replace ".dm" ".yml" )
    echo $output
    dotnet run {$basePath}{$val} > $output
end